
using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<Staff?> GetStaffByPhoneNumberAsync(string phoneNumber);
    Task<Customer?> GetCustomerByPhoneNumberAsync(string phoneNumber);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<bool> CustomerPhoneNumberExistsAsync(string phoneNumber);
    Task<bool> CustomerEmailExistsAsync(string email);
    Task<bool> LicensePlateExistsAsync(string licensePlate);
    Task AddCustomerAsync(Customer customer);
    
    Task<bool> StaffPhoneNumberExistsAsync(string phoneNumber);
    Task AddStaffAsync(Staff staff);
    
    Task UpdateCustomerAsync(Customer customer);
    Task UpdateStaffAsync(Staff staff);
}
