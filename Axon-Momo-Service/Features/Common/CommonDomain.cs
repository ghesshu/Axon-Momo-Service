using System;

namespace Axon_Momo_Service.Features.Common;

public class BcAuthorizeRequest
{
    public string Scope { get; set; }
    public string LoginHint { get; set; }
    public string AccessType { get; set; }
    public int? ConsentValidIn { get; set; }
    public string ClientNotificationToken { get; set; }
    public string ScopeInstruction { get; set; }
}

public class Oauth2TokenRequest
{
    public string GrantType { get; set; }
    public string AuthReqId { get; set; }
    public string RefreshToken { get; set; }
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
    public string Code { get; set; }
    public string Message { get; set; }
}