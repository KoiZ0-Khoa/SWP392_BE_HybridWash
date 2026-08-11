
using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Implementations;

public class AuthRepository : IAuthRepository
{
    private readonly AutowashContext _context;

    public AuthRepository(AutowashContext context)
    {
        _context = context;
    }

    public Task<Staff?> GetStaffByPhoneNumberAsync(string phoneNumber)
    {
        return _context.Staff.FirstOrDefaultAsync(staff => staff.PhoneNumber == phoneNumber);
    }

    public Task<Customer?> GetCustomerByPhoneNumberAsync(string phoneNumber)
    {
        return _context.Customers.FirstOrDefaultAsync(customer => customer.PhoneNumber == phoneNumber);
    }

    public Task<bool> CustomerPhoneNumberExistsAsync(string phoneNumber)
    {
        return _context.Customers.AnyAsync(customer => customer.PhoneNumber == phoneNumber);
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }
}
