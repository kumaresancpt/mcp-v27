using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Services;

public interface IAuthService
{
    Task<AuthResult> AuthenticateAsync(LoginRequest request);
    Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<ServiceOperationResult> ResetPasswordAsync(ResetPasswordRequest request);
}