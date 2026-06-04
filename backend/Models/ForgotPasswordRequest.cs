using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class ForgotPasswordRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}