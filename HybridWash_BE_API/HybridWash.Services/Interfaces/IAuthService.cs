
using HybridWash.Services.DTOs;

namespace HybridWash.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<AuthResponseDTO> RegisterCustomerAsync(RegisterRequestDTO request);
    }
}
