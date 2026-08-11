using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenGenerator _tokenGenerator;

        public AuthService(IAuthRepository authRepository, ITokenGenerator tokenGenerator)
        {
            _authRepository = authRepository;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            if (request.IsStaff)
            {
                var staff = await _authRepository.GetStaffByPhoneNumberAsync(request.PhoneNumber);
                if (staff == null || !BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
                {
                    throw new Exception("Invalid phone number or password.");
                }

                if (staff.IsActive != true)
                {
                    throw new Exception("Account is inactive.");
                }

                return new AuthResponseDTO
                {
                    Token = _tokenGenerator.Generate(staff.StaffId.ToString(), staff.Role ?? "Washer", staff.FullName),
                    FullName = staff.FullName,
                    Role = staff.Role ?? "Washer"
                };
            }
            else
            {
                var customer = await _authRepository.GetCustomerByPhoneNumberAsync(request.PhoneNumber);
                if (customer == null || !BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
                {
                    throw new Exception("Invalid phone number or password.");
                }

                return new AuthResponseDTO
                {
                    Token = _tokenGenerator.Generate(customer.CustomerId.ToString(), "Customer", customer.FullName),
                    FullName = customer.FullName,
                    Role = "Customer"
                };
            }
        }

        public async Task<AuthResponseDTO> RegisterCustomerAsync(RegisterRequestDTO request)
        {
            var existingCustomer = await _authRepository.CustomerPhoneNumberExistsAsync(request.PhoneNumber);
            if (existingCustomer)
            {
                throw new Exception("Phone number is already registered.");
            }

            var customer = new Customer
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CurrentTier = "Member",
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.AddCustomerAsync(customer);

            return new AuthResponseDTO
            {
                Token = _tokenGenerator.Generate(customer.CustomerId.ToString(), "Customer", customer.FullName),
                FullName = customer.FullName,
                Role = "Customer"
            };
        }

    }
}
