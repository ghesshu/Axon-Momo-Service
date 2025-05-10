using Microsoft.AspNetCore.Mvc;
using Axon_Momo_Service.Services;
using Axon_Momo_Service.Features.Common;

namespace Axon_Momo_Service.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected async Task<IActionResult?> CheckAuthentication(AuthContext authContext)
        {
            if (!await authContext.EnsureAuthenticatedAsync())
            {
                return Unauthorized(new ErrorReason
                {
                    Code = "PAYEE_NOT_FOUND",
                    Message = "Invalid or expired token"
                });
            }
            return null;
        }
    }
}