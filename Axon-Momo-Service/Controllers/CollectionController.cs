using System.Text.RegularExpressions;
using Axon_Momo_Service.Data;
using Axon_Momo_Service.Features.Collection;
using Axon_Momo_Service.Features.Common;
using Axon_Momo_Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Axon_Momo_Service.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public partial class CollectionController : BaseController
    {
        // Common headers


        // BC Authorize
        [HttpPost("v1_0/bc-authorize")]
        public async Task<IActionResult> BcAuthorize(
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromForm] BcAuthorizeRequest request,
            [FromServices] AuthContext authContext)
        {
            // Check authentication
            await CheckAuthentication(authContext);
            

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

            // Mock success response
            return Ok(new
            {
                auth_req_id = Guid.NewGuid().ToString("N"),
                interval = 5,
                expires_in = 600
            });
        }
         

        // 1. Create Access Token
        [HttpPost("token")]
        public async Task<IActionResult> CreateAccessToken(
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromServices] AuthContext authContext,
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

        // 2. Create Oauth2 Token
        [HttpPost("oauth2/token")]
        public async Task<IActionResult> CreateOauth2Token(
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromForm] Oauth2TokenRequest request,
            [FromServices] AuthContext authContext)
        {
            // Check authentication
            await CheckAuthentication(authContext);

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


        // 3. Create Payments
        [HttpPost("/v2_0/payment")]
        // [Authorize]
        public IActionResult CreatePayments(
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] CreatePaymentsRequest request)
        {

            // Validate request body
            if (request == null || 
                request.Money == null || 
                string.IsNullOrWhiteSpace(request.Money.Amount) || 
                string.IsNullOrWhiteSpace(request.Money.Currency))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing required fields in request body (money.amount, money.currency)."
                });
            }

            // Simulate error cases based on referenceId
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

            // Simulate invalid customerReference (client-side error)
            if (request.CustomerReference == "invalid")
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_CUSTOMER_REFERENCE",
                    Message = "Invalid customer reference provided."
                });
            }

            // Fake response: 202 Accepted with empty body
            return StatusCode(202);
        }

        // 4. Get Account Balance
        [HttpGet("/v1_0/account/balance")]
        // [Authorize]
        public IActionResult GetAccountBalance(
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment)
        {
          

            // Simulate error cases
            if (targetEnvironment == "invalid")
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "NOT_ALLOWED_TARGET_ENVIRONMENT",
                    Message = "Access to target environment is forbidden."
                });
            }

            if (targetEnvironment == "error")
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "INTERNAL_PROCESSING_ERROR",
                    Message = "An internal error occurred while processing."
                });
            }

            // Simulate invalid request (e.g., malformed target environment)
            if (string.IsNullOrWhiteSpace(targetEnvironment))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "X-Target-Environment is required and must not be empty."
                });
            }

            // Fake response: 200 OK with balance
            return Ok(new Balance
            {
                AvailableBalance = "1000.00",
                Currency = "GHS"
            });
        }

        // 5. Request To Pay
        [HttpPost("/v1_0/requesttopay")]
        // [Authorize]
        public IActionResult RequestToPay(
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] RequestToPayRequest request)
        {
          

            // Validate request body
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Amount) || 
                string.IsNullOrWhiteSpace(request.Currency) || 
                request.Payer == null || 
                string.IsNullOrWhiteSpace(request.Payer.PartyId) || 
                string.IsNullOrWhiteSpace(request.Payer.PartyIdType))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing required fields in request body (amount, currency, payer)."
                });
            }

            // Simulate error cases based on referenceId
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

            // Simulate invalid payer (client-side error)
            if (request.Payer.PartyId == "invalid")
            {
                return BadRequest(new ErrorReason
                {
                    Code = "PAYEE_NOT_FOUND",
                    Message = "Payer not found."
                });
            }

            // Fake response: 202 Accepted with empty body
            return StatusCode(202);
        }

        // 6. RequestToWithdraw-V1
        [HttpPost("/v1_0/requesttowithdraw")]
        // [Authorize]
        public IActionResult RequestToWithdraw(
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] RequestToPayRequest request)
        {

            // Validate request body
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Amount) || 
                string.IsNullOrWhiteSpace(request.Currency) || 
                request.Payer == null || 
                string.IsNullOrWhiteSpace(request.Payer.PartyId) || 
                string.IsNullOrWhiteSpace(request.Payer.PartyIdType))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing required fields in request body (amount, currency, payer)."
                });
            }

            // Simulate error cases based on referenceId
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

            // Simulate invalid payer (client-side error)
            if (request.Payer.PartyId == "invalid")
            {
                return BadRequest(new ErrorReason
                {
                    Code = "PAYEE_NOT_FOUND",
                    Message = "Payer not found."
                });
            }

            // Fake response: 202 Accepted with empty body
            return StatusCode(202);
        }
        
        // 7. RequestToWithdraw-V2
        [HttpPost("/v2_0/requesttowithdraw")]
        // [Authorize]
        public IActionResult RequestToWithdrawV2(
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] RequestToPayRequest request )
        {

            // Validate request body
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Amount) || 
                string.IsNullOrWhiteSpace(request.Currency) || 
                request.Payer == null || 
                string.IsNullOrWhiteSpace(request.Payer.PartyId) || 
                string.IsNullOrWhiteSpace(request.Payer.PartyIdType))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing required fields in request body (amount, currency, payer)."
                });
            }

            // Simulate error cases based on referenceId
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

            // Simulate invalid payer (client-side error)
            if (request.Payer.PartyId == "invalid")
            {
                return BadRequest(new ErrorReason
                {
                    Code = "PAYEE_NOT_FOUND",
                    Message = "Payer not found."
                });
            }

            // Fake response: 202 Accepted with empty body
            return StatusCode(202);
        }

        // 8. Request To Pay Transaction Status
         [HttpGet("/collection/v1_0/requesttopay/{referenceId}")]
         // [Authorize]
        public IActionResult RequesttoPayTransactionStatus(
            [FromRoute] string? referenceId,
            [FromHeader(Name = "Authorization")] string? authorization,
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

            // Simulate request-to-pay status responses
            var baseResponse = new RequestToPayResult
            {
                Amount = "100",
                Currency = "GHS",
                ExternalId = "947354",
                Payer = new Party
                {
                    PartyIdType = "MSISDN",
                    PartyId = "4656473839"
                }
            };

            if (referenceId == "success")
            {
                baseResponse.FinancialTransactionId = "23503452";
                baseResponse.Status = "SUCCESSFUL";
                return Ok(baseResponse);
            }

            if (referenceId == "payernotfound")
            {
                baseResponse.Status = "FAILED";
                baseResponse.Reason = new ErrorReason
                {
                    Code = "PAYER_NOT_FOUND",
                    Message = "Payee does not exist"
                };
                return Ok(baseResponse);
            }

            // Default case: Simulate a pending request-to-pay
            baseResponse.Status = "PENDING";
            baseResponse.PayerMessage = "Pending authorization";
            baseResponse.PayeeNote = "Awaiting payer approval";
            baseResponse.Reason = new ErrorReason
            {
                Code = "PAYEE_NOT_FOUND",
                Message = "Payee not found, transaction pending."
            };
            return Ok(baseResponse);
        }

        // 9. Request To Withdraw Transaction Status
        [HttpGet("/v1_0/requesttowithdraw/{referenceId}")]
        // [Authorize]
        public IActionResult RequestToWithdrawTransactionStatus(
            [FromRoute] string? referenceId,
            [FromHeader(Name = "Authorization")] string? authorization,
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

            // Simulate request-to-withdraw status responses
            var baseResponse = new RequestToPayResult
            {
                Amount = "100",
                Currency = "GHS",
                ExternalId = "947354",
                Payer = new Party
                {
                    PartyIdType = "MSISDN",
                    PartyId = "4656473839"
                }
            };

            if (referenceId == "success")
            {
                baseResponse.FinancialTransactionId = "23503452";
                baseResponse.Status = "SUCCESSFUL";
                return Ok(baseResponse);
            }

            if (referenceId == "payernotfound")
            {
                baseResponse.Status = "FAILED";
                baseResponse.Reason = new ErrorReason
                {
                    Code = "PAYER_NOT_FOUND",
                    Message = "Payee does not exist"
                };
                return Ok(baseResponse);
            }

            // Default case: Simulate a pending request-to-withdraw
            baseResponse.Status = "PENDING";
            baseResponse.PayerMessage = "Pending authorization";
            baseResponse.PayeeNote = "Awaiting payer approval";
            baseResponse.Reason = new ErrorReason
            {
                Code = "PAYEE_NOT_FOUND",
                Message = "Payee not found, transaction pending."
            };
            return Ok(baseResponse);
        }

        // 10. ValidateAccountHolderStatus
         [HttpGet("v1_0/accountholder/{accountHolderIdType}/{accountHolderId}/active")]
         // [Authorize]
        public IActionResult ValidateAccountHolderStatus(
            [FromRoute] string? accountHolderIdType,
            [FromRoute] string? accountHolderId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment)
        {
            

            // Validate accountHolderIdType (case-sensitive, must be lowercase 'msisdn' or 'email')
            if (string.IsNullOrWhiteSpace(accountHolderIdType) || 
                (accountHolderIdType != "msisdn" && accountHolderIdType != "email"))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_ACCOUNT_HOLDER_ID_TYPE",
                    Message = "accountHolderIdType must be 'msisdn' or 'email' (lowercase)."
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
            if (accountHolderIdType == "msisdn")
            {
                // MSISDN validation (ITU-T E.164: + followed by 1-15 digits)
                if (!MyRegex().IsMatch(accountHolderId))
                {
                    return BadRequest(new ErrorReason
                    {
                        Code = "INVALID_MSISDN",
                        Message = "accountHolderId must be a valid MSISDN (e.g., +1234567890)."
                    });
                }
            }
            else if (accountHolderIdType == "email")
            {
                // Email validation
                if (!MyRegex1().IsMatch(accountHolderId))
                {
                    return BadRequest(new ErrorReason
                    {
                        Code = "INVALID_EMAIL",
                        Message = "accountHolderId must be a valid email address."
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

        [GeneratedRegex(@"^\+\d{1,15}$")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex MyRegex1();
    }
}
