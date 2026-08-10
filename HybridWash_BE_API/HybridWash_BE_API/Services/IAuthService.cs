using HybridWash_BE_API.DTOs;

namespace HybridWash_BE_API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<AuthResponseDTO> RegisterCustomerAsync(RegisterRequestDTO request);
    }
}
