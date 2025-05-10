using System;
using System.ComponentModel.DataAnnotations;

namespace Axon_Momo_Service.Features.Auth;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Tel must be a 10-digit number.")]
    public string Tel { get; set; } = string.Empty;
}


public class AuthToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public int Interval { get; set; }

    public long Expires_in { get; set; }

    // Navigation property
    public User? User { get; set; }
}