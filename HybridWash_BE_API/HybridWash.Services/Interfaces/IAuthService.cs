
using HybridWash.Services.DTOs;

namespace HybridWash.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<AuthResponseDTO> RegisterCustomerAsync(RegisterRequestDTO request);
        Task<AuthResponseDTO> CreateStaffAsync(CreateStaffRequestDTO request);
        Task<string> ForgotPasswordAsync(ForgotPasswordRequestDTO request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO request);
    }
}
