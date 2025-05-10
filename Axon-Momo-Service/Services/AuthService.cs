using System;
using System.Threading.Tasks;
using Axon_Momo_Service.Data;
using Axon_Momo_Service.Features.Auth;
using Axon_Momo_Service.Features.Collection;
using Axon_Momo_Service.Features.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Axon_Momo_Service.Services;

public class AuthContext(IHttpContextAccessor httpContextAccessor, DataContext db)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly DataContext _db = db;

    private async Task<AuthToken?> GetTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var authId = httpContext?.Request.Headers.Authorization.ToString();
        // var authId = httpContext?.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authId))
            return null;

        if (!Guid.TryParse(authId, out var authIdGuid))
            return null;

        var token = await _db.AuthTokens.FirstOrDefaultAsync(t => t.Id == authIdGuid);

        if (token == null)
            return null;

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return token.Expires_in > now ? token : null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return token != null;
    }

    public async Task<string?> GetUserTelAsync()
    {
        var token = await GetTokenAsync();
        if (token == null) return null;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == token.UserId);
        return user?.Tel;
    }

    public async Task<bool> EnsureAuthenticatedAsync()
    {
        return await IsAuthenticatedAsync();
    }

    public IActionResult? ValidateHeaders(string[] headers)
    {
        // var httpContext = _httpContextAccessor.HttpContext;
        // if (httpContext == null)
        //     return new BadRequestObjectResult(new ErrorReason
        //     {
        //         Code = "HTTP_CONTEXT_ERROR",
        //         Message = "HTTP context is not available"
        //     });

        // var missingHeaders = headers
        //     .Where(header => string.IsNullOrEmpty(httpContext.Request.Headers[header].ToString()))
        //     .ToList();

        // if (missingHeaders.Count != 0)
        // {
        //     var message = missingHeaders.Count == 1
        //         ? $"{missingHeaders[0]} header is required"
        //         : $"The following headers are required: {string.Join(", ", missingHeaders)}";
            
        //     return new BadRequestObjectResult(new ErrorReason
        //     {
        //         Code = "MISSING_HEADER",
        //         Message = message
        //     });
        // }

        return null;
    }
}