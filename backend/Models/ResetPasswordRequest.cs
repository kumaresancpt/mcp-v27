using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class ResetPasswordRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}