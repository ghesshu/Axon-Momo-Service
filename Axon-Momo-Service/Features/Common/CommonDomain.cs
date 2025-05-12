using System;

namespace Axon_Momo_Service.Features.Common;

public class BcAuthorizeRequest
{
    public string Scope { get; set; } = string.Empty;
    public string LoginHint { get; set; }  = string.Empty;
    public string AccessType { get; set; } = string.Empty;
    public int? ConsentValidIn { get; set; } 
    public string ClientNotificationToken { get; set; } = string.Empty;
    public string ScopeInstruction { get; set; } = string.Empty;
}

public class Oauth2TokenRequest
{
    public string GrantType { get; set; } = string.Empty;
    public string AuthReqId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}


public class Party
{
    public string? PartyIdType { get; set; }
    public string? PartyId { get; set; }
}

public class Money
{
    public string? Amount { get; set; }

    public string? Currency { get; set; }
}

public class ErrorReason
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}