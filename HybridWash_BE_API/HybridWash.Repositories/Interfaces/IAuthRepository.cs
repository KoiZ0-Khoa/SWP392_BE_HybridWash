using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<Staff?> GetStaffByPhoneNumberAsync(string phoneNumber);
    Task<Customer?> GetCustomerByPhoneNumberAsync(string phoneNumber);
    Task<bool> CustomerPhoneNumberExistsAsync(string phoneNumber);
    Task AddCustomerAsync(Customer customer);
}
