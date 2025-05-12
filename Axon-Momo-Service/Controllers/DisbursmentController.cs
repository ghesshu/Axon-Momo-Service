using System.Text.RegularExpressions;
using Axon_Momo_Service.Data;
using Axon_Momo_Service.Features.Auth;
using Axon_Momo_Service.Features.Common;
using Axon_Momo_Service.Features.Disbursment;
using Axon_Momo_Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Axon_Momo_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisbursmentController : BaseController
    {
        // 1. BC Authorize
        [HttpPost("v1_0/bc-authorize")]
        public async Task<IActionResult> BcAuthorize(
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromForm] BcAuthorizeRequest request,
            [FromServices] AuthContext authContext,
            [FromServices] DataContext db) 
        {
          // Validate request body
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Scope) || 
                string.IsNullOrWhiteSpace(request.LoginHint))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing scope or login_hint in request body."
                });
            }

            // Validate access_type if provided
            if (!string.IsNullOrWhiteSpace(request.AccessType) && 
                request.AccessType != "online" && request.AccessType != "offline")
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_ACCESS_TYPE",
                    Message = "access_type must be 'online' or 'offline'."
                });
            }

            // Look up user by LoginHint (assuming LoginHint maps to Tel)
            var user = await db.Users.FirstOrDefaultAsync(u => u.Tel == request.LoginHint);
            if (user == null)
            {
                return NotFound(new ErrorReason
                {
                    Code = "PAYEE_NOT_FOUND",
                    Message = "User not found."
                });
            }

            // Create auth token
            var interval = 5;
            var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeMilliseconds();
            var token = new AuthToken
            {
                UserId = user.Id,
                Interval = interval,
                Expires_in = expiry
            };

            // Save token to database
            db.AuthTokens.Add(token);
            await db.SaveChangesAsync();

            // Prepare response
            var response = new
            {
                auth_req_id = token.Id.ToString(),
                interval = interval,
                expires_in = expiry
            };

            return Ok(response);
        }
                  

        // 2. Create Oauth2 Token
        [HttpPost("oauth2/token")]
        public async Task<IActionResult> CreateOauth2Token(
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromForm] Oauth2TokenRequest request,
            [FromServices] AuthContext authContext)
        {
            // Check authentication
            var authCheck = await CheckAuthentication(authContext);
            if (authCheck != null)
            {
                return Unauthorized(new ErrorReason
                {
                    Code = "UNAUTHORIZED",
                    Message = "Invalid or missing authorization header."
                });
            }
            // Validate request body
            if (request == null || string.IsNullOrWhiteSpace(request.GrantType))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing grant_type in request body."
                });
            }

            // Mock success response
            return Ok(new
            {
                access_token = Guid.NewGuid().ToString("N"),
                token_type = "Bearer",
                expires_in = 3600,
                scope = "collection",
                refresh_token = Guid.NewGuid().ToString("N"),
                refresh_token_expired_in = 86400
            });
        }
    
        
        // 3. Create Access Token
        [HttpPost("token")]
        public async Task<IActionResult> CreateAccessToken(
            [FromHeader(Name = "Authorization")] string authorization,  
            [FromServices] DataContext db)
        {
            // Check if authorization header exists and has valid format
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized(new ErrorReason
                {
                    Code = "UNAUTHORIZED",
                    Message = "Invalid or missing authorization header."
                });
            }

            // Extract auth_req_id from authorization header
            // Assuming the auth_req_id is passed directly in the Authorization header
            if (!Guid.TryParse(authorization, out Guid authReqId))
            {
                return Unauthorized(new ErrorReason
                {
                    Code = "INVALID_AUTH_REQ_ID",
                    Message = "Invalid auth_req_id format."
                });
            }

            // Find the token in database
            var authToken = await db.AuthTokens.FirstOrDefaultAsync(t => t.Id == authReqId);
            if (authToken == null)
            {
                return Unauthorized(new ErrorReason
                {
                    Code = "INVALID_TOKEN",
                    Message = "Token not found."
                });
            }

            // Check if token has expired
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (currentTime > authToken.Expires_in)
            {
                return Unauthorized(new ErrorReason
                {
                    Code = "TOKEN_EXPIRED",
                    Message = "Session has expired."
                });
            }

            // Generate access token response
            return Ok(new
            {
                access_token = Guid.NewGuid().ToString("N"), // In production, use proper JWT token generation
                token_type = "Bearer",
                expires_in = 3600
            });
        }


        // 3. Deposit V_01
        [HttpPost("collection/v1_0/deposit")]
        // [Authorize]
        public IActionResult CreateDeposit(
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] DepositRequest request)
        {

            // Validate request body
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Amount) || 
                string.IsNullOrWhiteSpace(request.Currency) || 
                request.Payee == null || 
                string.IsNullOrWhiteSpace(request.Payee.PartyId) || 
                string.IsNullOrWhiteSpace(request.Payee.PartyIdType))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing required fields in request body."
                });
            }

            // Simulate error cases based on referenceId or currency for testing
            if (referenceId == "duplicate")
            {
                return StatusCode(409, new ErrorReason
                {
                    Code = "RESOURCE_ALREADY_EXIST",
                    Message = "Duplicated reference id. Creation of resource failed."
                });
            }

            if (request.Currency != "GHS") // Simulate invalid currency
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "INVALID_CURRENCY",
                    Message = "Currency not supported."
                });
            }

            // Fake response: 202 Accepted with empty body
            return StatusCode(202);
        }

        // 4. Deposit V_02
        [HttpPost("collection/v2_0/deposit")]
        // [Authorize]
        public IActionResult CreateDepositV2(
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] DepositV2Request request)
        {

            // Validate request body
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Amount) || 
                string.IsNullOrWhiteSpace(request.Currency) || 
                request.Payee == null || 
                string.IsNullOrWhiteSpace(request.Payee.PartyId) || 
                string.IsNullOrWhiteSpace(request.Payee.PartyIdType))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing required fields in request body."
                });
            }

            // Simulate error cases based on referenceId or currency for testing
            if (referenceId == "duplicate")
            {
                return StatusCode(409, new ErrorReason
                {
                    Code = "RESOURCE_ALREADY_EXIST",
                    Message = "Duplicated reference id. Creation of resource failed."
                });
            }

            if (request.Currency != "GHS") // Simulate invalid currency
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "INVALID_CURRENCY",
                    Message = "Currency not supported."
                });
            }

            // Fake response: 202 Accepted with empty body
            return StatusCode(202);
        }


        // 5. Transfer
        [HttpPost("v1_0/transfer")]
        // [Authorize]
        public IActionResult CreateTransfer(
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] TransferRequest request)
        {

            // Validate request body
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Amount) || 
                string.IsNullOrWhiteSpace(request.Currency) || 
                request.Payee == null || 
                string.IsNullOrWhiteSpace(request.Payee.PartyId) || 
                string.IsNullOrWhiteSpace(request.Payee.PartyIdType))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing required fields in request body."
                });
            }

            // Simulate error cases based on referenceId or currency for testing
            if (referenceId == "duplicate")
            {
                return StatusCode(409, new ErrorReason
                {
                    Code = "RESOURCE_ALREADY_EXIST",
                    Message = "Duplicated reference id. Creation of resource failed."
                });
            }

            if (referenceId == "error")
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "INTERNAL_PROCESSING_ERROR",
                    Message = "An internal error occurred while processing."
                });
            }

            if (request.Currency != "GHS") // Simulate invalid currency as client error
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_CURRENCY",
                    Message = "Currency not supported."
                });
            }

            // Fake response: 202 Accepted with empty body
            return StatusCode(202);
        }


        // 6. Get Transfer Status
        [HttpGet("v1_0/transfer/{referenceId}")]
        // [Authorize]
        public IActionResult GetTransferStatus(
            [FromRoute] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment)
        {

            // Validate referenceId
            if (string.IsNullOrWhiteSpace(referenceId))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REFERENCE_ID",
                    Message = "referenceId is required and must not be empty."
                });
            }

            // Simulate error cases based on referenceId
            if (referenceId == "error")
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "INTERNAL_PROCESSING_ERROR",
                    Message = "An internal error occurred while processing."
                });
            }

            if (referenceId == "notfound")
            {
                return NotFound(new ErrorReason
                {
                    Code = "RESOURCE_NOT_FOUND",
                    Message = "Requested resource was not found."
                });
            }

            // Simulate transfer status responses
            var baseResponse = new DisbursmentTransferResult
            {
                Amount = "100",
                Currency = "GHS",
                ExternalId = "83453",
                Payee = new Party
                {
                    PartyIdType = "MSISDN",
                    PartyId = "4609274685"
                }
            };

            if (referenceId == "success")
            {
                baseResponse.FinancialTransactionId = "363440463";
                baseResponse.Status = "SUCCESSFUL";
                return Ok(baseResponse);
            }

            if (referenceId == "limitbreached")
            {
                baseResponse.Status = "FAILED";
                baseResponse.Reason = new ErrorReason
                {
                    Code = "PAYER_LIMIT_REACHED",
                    Message = "The payer's limit has been breached."
                };
                return Ok(baseResponse);
            }

            if (referenceId == "insufficient")
            {
                baseResponse.Status = "FAILED";
                baseResponse.Reason = new ErrorReason
                {
                    Code = "NOT_ENOUGH_FUNDS",
                    Message = "The payer does not have enough funds."
                };
                return Ok(baseResponse);
            }

            // Default case: Simulate a pending transfer
            baseResponse.Status = "PENDING";
            baseResponse.PayerMessage = "Pending transfer";
            baseResponse.PayeeNote = "Awaiting processing";
            baseResponse.Reason = new ErrorReason
            {
                Code = "PAYEE_NOT_FOUND",
                Message = "Payee not found, transfer pending."
            };
            return Ok(baseResponse);
        }


        // 7. Get Account Holder Status
        [HttpGet("v1_0/accountholder/{accountHolderIdType}/{accountHolderId}/active")]
        // [Authorize]
        public IActionResult ValidateAccountHolderStatus(
            [FromRoute] string? accountHolderIdType,
            [FromRoute] string? accountHolderId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromServices] AuthContext authContext)
        {

            // Validate accountHolderIdType
            var validIdTypes = new[] { "MSISDN", "email", "alias", "id" };
            if (string.IsNullOrWhiteSpace(accountHolderIdType) || 
                !validIdTypes.Contains(accountHolderIdType, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_ACCOUNT_HOLDER_ID_TYPE",
                    Message = "accountHolderIdType must be one of 'MSISDN', 'email', 'alias', or 'id'."
                });
            }

            // Validate accountHolderId
            if (string.IsNullOrWhiteSpace(accountHolderId))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_ACCOUNT_HOLDER_ID",
                    Message = "accountHolderId is required and must not be empty."
                });
            }

            // Validate accountHolderId format based on accountHolderIdType
            if (accountHolderIdType.Equals("MSISDN", StringComparison.OrdinalIgnoreCase))
            {
                // Basic MSISDN validation (ITU-T E.164: + followed by 1-15 digits)
                if (!Regex.IsMatch(accountHolderId, @"^\+\d{1,15}$"))
                {
                    return BadRequest(new ErrorReason
                    {
                        Code = "INVALID_MSISDN",
                        Message = "accountHolderId must be a valid MSISDN (e.g., +1234567890)."
                    });
                }
            }
            else if (accountHolderIdType.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                // Basic email validation
                if (!Regex.IsMatch(accountHolderId, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return BadRequest(new ErrorReason
                    {
                        Code = "INVALID_EMAIL",
                        Message = "accountHolderId must be a valid email address."
                    });
                }
            }
            else if (accountHolderIdType.Equals("alias", StringComparison.OrdinalIgnoreCase) || 
                     accountHolderIdType.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                // Basic validation for alias or id (non-empty string, no specific format assumed)
                if (accountHolderId.Length > 100) // Arbitrary max length to prevent abuse
                {
                    return BadRequest(new ErrorReason
                    {
                        Code = "INVALID_ACCOUNT_HOLDER_ID",
                        Message = "accountHolderId is too long for type 'alias' or 'id'."
                    });
                }
            }

            // Simulate error cases
            if (targetEnvironment == "invalid")
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "NOT_ALLOWED_TARGET_ENVIRONMENT",
                    Message = "Access to target environment is forbidden."
                });
            }

            if (accountHolderId == "error")
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "INTERNAL_PROCESSING_ERROR",
                    Message = "An internal error occurred while processing."
                });
            }

            // Simulate account holder status
            if (accountHolderId == "inactive" || accountHolderId == "notfound")
            {
                return Ok(false);
            }

            // Default case: Active account holder
            return Ok(true);
        }
    
    }
}
