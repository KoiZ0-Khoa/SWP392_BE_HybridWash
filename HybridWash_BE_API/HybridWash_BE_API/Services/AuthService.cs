using HybridWash_BE_API.DTOs;
using HybridWash_BE_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HybridWash_BE_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AutowashContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AutowashContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            if (request.IsStaff)
            {
                var staff = await _context.Staff.FirstOrDefaultAsync(s => s.PhoneNumber == request.PhoneNumber);
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
                    Token = GenerateJwtToken(staff.StaffId.ToString(), staff.Role ?? "Washer", staff.FullName),
                    FullName = staff.FullName,
                    Role = staff.Role ?? "Washer"
                };
            }
            else
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == request.PhoneNumber);
                if (customer == null || !BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
                {
                    throw new Exception("Invalid phone number or password.");
                }

                return new AuthResponseDTO
                {
                    Token = GenerateJwtToken(customer.CustomerId.ToString(), "Customer", customer.FullName),
                    FullName = customer.FullName,
                    Role = "Customer"
                };
            }
        }

        public async Task<AuthResponseDTO> RegisterCustomerAsync(RegisterRequestDTO request)
        {
            var existingCustomer = await _context.Customers.AnyAsync(c => c.PhoneNumber == request.PhoneNumber);
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

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return new AuthResponseDTO
            {
                Token = GenerateJwtToken(customer.CustomerId.ToString(), "Customer", customer.FullName),
                FullName = customer.FullName,
                Role = "Customer"
            };
        }

        private string GenerateJwtToken(string id, string role, string fullName)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new Exception("JWT Key is missing"));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, id),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
