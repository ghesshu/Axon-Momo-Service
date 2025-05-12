using System;

namespace Axon_Momo_Service.Features.Auth;

public class AuthRequest
{
    public string Tel { get; set; } = string.Empty;
}


public class AuthResponse
{
    public string Auth_req_id { get; set; } = string.Empty;
    public int Interval { get; set; }
    public long Expires_in { get; set; }
}