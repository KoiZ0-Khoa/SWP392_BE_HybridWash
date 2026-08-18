using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace HybridWash.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;

        public AuthService(IAuthRepository authRepository, ITokenGenerator tokenGenerator, IMemoryCache memoryCache, IEmailService emailService)
        {
            _authRepository = authRepository;
            _tokenGenerator = tokenGenerator;
            _memoryCache = memoryCache;
            _emailService = emailService;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            // 1. Thử tìm trong bảng Staff trước (Admin hoặc Staff)
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
                    Token = _tokenGenerator.Generate(staff.StaffId.ToString(), staff.Role ?? "Staff", staff.FullName),
                    FullName = staff.FullName,
                    Role = staff.Role ?? "Staff"
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

            var existingEmail = await _authRepository.CustomerEmailExistsAsync(request.Email);
            if (existingEmail)
            {
                throw new Exception("Email is already registered.");
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
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CurrentTier = "Member",
                CreatedAt = DateTime.UtcNow,
                Vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        LicensePlate = request.LicensePlate,
                        VehicleType = request.VehicleType,
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

        public async Task<string> ForgotPasswordAsync(ForgotPasswordRequestDTO request)
        {
            var customer = await _authRepository.GetCustomerByEmailAsync(request.Email);

            if (customer == null)
            {
                throw new Exception("Email không tồn tại trong hệ thống.");
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Save to memory cache with 5 minutes expiration
            _memoryCache.Set($"OTP_{request.Email}", otp, TimeSpan.FromMinutes(5));

            // Send Email
            string subject = "Mã xác nhận khôi phục mật khẩu";
            string body = $"Mã OTP của bạn là: <strong>{otp}</strong>. Mã này sẽ hết hạn trong 5 phút.";
            await _emailService.SendEmailAsync(request.Email, subject, body);

            return "Mã OTP đã được gửi đến email của bạn.";
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            if (!_memoryCache.TryGetValue($"OTP_{request.Email}", out string? cachedOtp))
            {
                throw new Exception("Mã OTP đã hết hạn hoặc không tồn tại.");
            }

            if (cachedOtp != request.OTP)
            {
                throw new Exception("Mã OTP không chính xác.");
            }

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            var customer = await _authRepository.GetCustomerByEmailAsync(request.Email);
            if (customer != null)
            {
                customer.PasswordHash = newPasswordHash;
                await _authRepository.UpdateCustomerAsync(customer);
            }
            else
            {
                throw new Exception("Email không tồn tại trong hệ thống.");
            }

            _memoryCache.Remove($"OTP_{request.Email}");
            return true;
        }

        public async Task<UserListDto> GetAllUsersAsync()
        {
            var customers = await _authRepository.GetAllCustomersAsync();
            var staffs = await _authRepository.GetAllStaffsAsync();

            var result = new UserListDto();

            foreach (var c in customers)
            {
                result.Customers.Add(new UserDto
                {
                    Id = c.CustomerId,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Role = "Customer",
                    Tier = c.CurrentTier,
                    CreatedAt = c.CreatedAt
                });
            }

            foreach (var s in staffs)
            {
                var dto = new UserDto
                {
                    Id = s.StaffId,
                    FullName = s.FullName,
                    PhoneNumber = s.PhoneNumber,
                    Role = s.Role,
                    CreatedAt = s.CreatedAt
                };

                if (s.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    result.Admins.Add(dto);
                }
                else
                {
                    result.Staffs.Add(dto);
                }
            }

            return result;
        }
    }
}
