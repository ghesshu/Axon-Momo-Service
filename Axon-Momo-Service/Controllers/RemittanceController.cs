using Axon_Momo_Service.Data;
using Axon_Momo_Service.Features.Common;
using Axon_Momo_Service.Features.Remittance;
using Axon_Momo_Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Axon_Momo_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemittanceController : BaseController
    {
        // Headers for Remittance API
        private static readonly string[] BcAuthorizeHeaders = ["X-Target-Environment"];
        private static readonly string[] CreateOauth2TokenHeaders = ["X-Target-Environment"];
        private static readonly string[] CashTransferHeaders = ["X-Reference-Id", "X-Target-Environment"];
        private static readonly string[] CashTransferStatusHeaders = ["X-Target-Environment"];
        private static readonly string[] TransferStatusHeaders = ["X-Target-Environment"];
        private static readonly string[] AccountHolderStatusHeaders = ["X-Target-Environment"];
        private static readonly string[] TransferHeaders = ["X-Reference-Id", "X-Target-Environment"];

        // 1. BC Authorize
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

            // Validate required headers
            authContext.ValidateHeaders(CreateOauth2TokenHeaders);

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
            [FromServices] DataContext db,
            [FromServices] JwtService jwtService)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized(new ErrorReason
                {
                    Code = "UNAUTHORIZED",
                    Message = "Invalid or missing authorization header."
                });
            }

            // Extract auth_req_id from authorization header
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

            // Generate JWT token
            var jwtToken = jwtService.GenerateToken(authToken.UserId);

            return Ok(new
            {
                access_token = jwtToken,
                token_type = "Bearer",
                expires_in = 3600
            });
        }
        
        // 4. CashTransfer
        [HttpPost("v2_0/cashtransfer")]
        // [Authorize] 
        public IActionResult CreateCashTransfer(
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] CashTransferRequest request,
            [FromServices] AuthContext authContext)
        {

            // Validate request body
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Amount) || 
                string.IsNullOrWhiteSpace(request.Currency) || 
                request.Payee == null || 
                string.IsNullOrWhiteSpace(request.Payee.PartyId) || 
                string.IsNullOrWhiteSpace(request.Payee.PartyIdType))
            {
                return BadRequest(new
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid or missing required fields in request body."
                });
            }

            // Simulate error cases based on referenceId or currency for testing
            if (referenceId == "duplicate")
            {
                return StatusCode(409, new
                {
                    Code = "RESOURCE_ALREADY_EXIST",
                    Message = "Duplicated reference id. Creation of resource failed."
                });
            }

            if (request.Currency != "GHS") // Simulate invalid currency
            {
                return StatusCode(500, new
                {
                    Code = "INVALID_CURRENCY",
                    Message = "Currency not supported, set currency to GHS"
                });
            }

            // Fake response: 202 Accepted with empty body
            return StatusCode(202);
        }

        // 5. GET CashTransferStatus
        [HttpGet("v2_0/cashtransfer/{referenceId}")]
        public IActionResult GetCashTransferStatus(
            [FromRoute] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment)
        {
        

            // Validate route parameter
            if (string.IsNullOrWhiteSpace(referenceId))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REFERENCE_ID",
                    Message = "ReferenceId is required."
                });
            }

            // Simulate different responses based on referenceId for testing purposes
            if (referenceId == "not-found")
            {
                return NotFound(new ErrorReason
                {
                    Code = "RESOURCE_NOT_FOUND",
                    Message = "Requested resource was not found."
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

            if (referenceId == "limit-breached")
            {
                return Ok(new CashTransferResult
                {
                    Amount = "100",
                    Currency = "GHS",
                    ExternalId = "83453",
                    Payee = new Party
                    {
                        PartyIdType = "MSISDN",
                        PartyId = "4609274685"
                    },
                    Status = "FAILED",
                    Reason = new ErrorReason
                    {
                        Code = "PAYER_LIMIT_REACHED",
                        Message = "The payer's limit has been breached."
                    }
                });
            }

            if (referenceId == "insufficient-balance")
            {
                return Ok(new CashTransferResult
                {
                    Amount = "100",
                    Currency = "GHS",
                    ExternalId = "83453",
                    Payee = new Party
                    {
                        PartyIdType = "MSISDN",
                        PartyId = "4609274685"
                    },
                    Status = "FAILED",
                    Reason = new ErrorReason
                    {
                        Code = "NOT_ENOUGH_FUNDS",
                        Message = "The payer does not have enough funds."
                    }
                });
            }

            // Mock success response
            return Ok(new CashTransferResult
            {
                Amount = "100",
                Currency = "GHS",
                FinancialTransactionId = "363440463",
                ExternalId = "83453",
                Payee = new Party
                {
                    PartyIdType = "MSISDN",
                    PartyId = "4609274685"
                },
                Status = "SUCCESSFUL"
            });
        }
    
        // 6. Transfer
        [HttpPost("v1_0/transfer")]
        public IActionResult CreateTransfer(
            [FromHeader(Name = "X-Callback-Url")] string? callbackUrl,
            [FromHeader(Name = "X-Reference-Id")] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment,
            [FromBody] RemittanceTransferRequest request)
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

        // 7. Get TransferStatus
        [HttpGet("v1_0/transfer/{referenceId}")]
        public IActionResult GetTransferStatus(
            [FromRoute] string? referenceId,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment)
        {

            // Validate route parameter
            if (string.IsNullOrWhiteSpace(referenceId))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_REFERENCE_ID",
                    Message = "ReferenceId is required."
                });
            }

            // Simulate different responses based on referenceId for testing purposes
            if (referenceId == "not-found")
            {
                return NotFound(new ErrorReason
                {
                    Code = "RESOURCE_NOT_FOUND",
                    Message = "Requested resource was not found."
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

            if (referenceId == "limit-breached")
            {
                return Ok(new TransferResult
                {
                    Amount = "100",
                    Currency = "GHS",
                    ExternalId = "83453",
                    Payee = new Party
                    {
                        PartyIdType = "MSISDN",
                        PartyId = "4609274685"
                    },
                    Status = "FAILED",
                    Reason = new ErrorReason
                    {
                        Code = "PAYER_LIMIT_REACHED",
                        Message = "The payer's limit has been breached."
                    }
                });
            }

            if (referenceId == "insufficient-balance")
            {
                return Ok(new TransferResult
                {
                    Amount = "100",
                    Currency = "GHS",
                    ExternalId = "83453",
                    Payee = new Party
                    {
                        PartyIdType = "MSISDN",
                        PartyId = "4609274685"
                    },
                    Status = "FAILED",
                    Reason = new ErrorReason
                    {
                        Code = "NOT_ENOUGH_FUNDS",
                        Message = "The payer does not have enough funds."
                    }
                });
            }

            // Mock success response
            return Ok(new TransferResult
            {
                Amount = "100",
                Currency = "GHS",
                FinancialTransactionId = "363440463",
                ExternalId = "83453",
                Payee = new Party
                {
                    PartyIdType = "MSISDN",
                    PartyId = "4609274685"
                },
                Status = "SUCCESSFUL"
            });
        }

        // 8. GET ValidateAccountHolderStatus
        [HttpGet("v1_0/accountholder/{accountHolderIdType}/{accountHolderId}/active")]
        public IActionResult ValidateAccountHolderStatus(
            [FromRoute] string? accountHolderIdType,
            [FromRoute] string? accountHolderId,
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromHeader(Name = "X-Target-Environment")] string? targetEnvironment)
        {

            // Validate route parameters
            if (string.IsNullOrWhiteSpace(accountHolderIdType))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_ACCOUNT_HOLDER_ID_TYPE",
                    Message = "accountHolderIdType is required."
                });
            }

            if (string.IsNullOrWhiteSpace(accountHolderId))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_ACCOUNT_HOLDER_ID",
                    Message = "accountHolderId is required."
                });
            }

            // Validate accountHolderIdType
            var validIdTypes = new[] { "msisdn", "email", "party_code" };
            if (!validIdTypes.Contains(accountHolderIdType.ToLower()))
            {
                return BadRequest(new ErrorReason
                {
                    Code = "INVALID_ACCOUNT_HOLDER_ID_TYPE",
                    Message = "accountHolderIdType must be one of: msisdn, email, party_code."
                });
            }

            // Simulate error case for incorrect target environment
            if (targetEnvironment == "invalid")
            {
                return StatusCode(500, new ErrorReason
                {
                    Code = "NOT_ALLOWED_TARGET_ENVIRONMENT",
                    Message = "Access to target environment is forbidden."
                });
            }

            // Simulate account holder not found or inactive
            if (accountHolderId == "inactive")
            {
                return Ok(false);
            }

            // Mock success response: account holder is active
            return Ok(true);
        }
    }

}

