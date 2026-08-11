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
            // 1. Thử tìm trong bảng Staff trước (bao gồm Admin, Manager, Washer...)
            var staff = await _authRepository.GetStaffByPhoneNumberAsync(request.PhoneNumber);
            if (staff != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
                {
                    throw new Exception("Mật khẩu không chính xác.");
                }

                if (staff.IsActive != true)
                {
                    throw new Exception("Tài khoản đã bị khóa.");
                }

                return new AuthResponseDTO
                {
                    Token = _tokenGenerator.Generate(staff.StaffId.ToString(), staff.Role ?? "Washer", staff.FullName),
                    FullName = staff.FullName,
                    Role = staff.Role ?? "Washer"
                };
            }

            // 2. Nếu không có trong Staff, tìm trong bảng Customers
            var customer = await _authRepository.GetCustomerByPhoneNumberAsync(request.PhoneNumber);
            if (customer != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
                {
                    throw new Exception("Mật khẩu không chính xác.");
                }

                return new AuthResponseDTO
                {
                    Token = _tokenGenerator.Generate(customer.CustomerId.ToString(), "Customer", customer.FullName),
                    FullName = customer.FullName,
                    Role = "Customer"
                };
            }

            // 3. Nếu không tìm thấy ở cả 2 bảng
            throw new Exception("Số điện thoại hoặc mật khẩu không chính xác.");
        }

        public async Task<AuthResponseDTO> RegisterCustomerAsync(RegisterRequestDTO request)
        {
            var existingCustomer = await _authRepository.CustomerPhoneNumberExistsAsync(request.PhoneNumber);
            if (existingCustomer)
            {
                throw new Exception("Phone number is already registered.");
            }

            var existingLicensePlate = await _authRepository.LicensePlateExistsAsync(request.LicensePlate);
            if (existingLicensePlate)
            {
                throw new Exception("License plate is already registered.");
            }

            var customer = new Customer
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CurrentTier = "Member",
                CreatedAt = DateTime.UtcNow,
                Vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        LicensePlate = request.LicensePlate,
                        VehicleType = request.VehicleType,
                        QrCode = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            await _authRepository.AddCustomerAsync(customer);

            return new AuthResponseDTO
            {
                Token = _tokenGenerator.Generate(customer.CustomerId.ToString(), "Customer", customer.FullName),
                FullName = customer.FullName,
                Role = "Customer"
            };
        }

        public async Task<AuthResponseDTO> CreateStaffAsync(CreateStaffRequestDTO request)
        {
            if (await _authRepository.StaffPhoneNumberExistsAsync(request.PhoneNumber))
            {
                throw new Exception("Số điện thoại nhân viên đã tồn tại.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var staff = new Staff
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = passwordHash,
                Role = string.IsNullOrEmpty(request.Role) ? "Staff" : request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.AddStaffAsync(staff);

            return new AuthResponseDTO
            {
                Token = "", // Không tự động đăng nhập khi Admin tạo
                FullName = staff.FullName,
                Role = staff.Role
            };
        }
    }
}
