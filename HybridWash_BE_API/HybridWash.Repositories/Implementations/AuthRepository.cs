
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

    public Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return _context.Customers.FirstOrDefaultAsync(customer => customer.Email == email);
    }

    public Task<bool> CustomerEmailExistsAsync(string email)
    {
        return _context.Customers.AnyAsync(customer => customer.Email == email);
    }

    public Task<bool> LicensePlateExistsAsync(string licensePlate)
    {
        return _context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate);
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> StaffPhoneNumberExistsAsync(string phoneNumber)
    {
        return await _context.Staff.AnyAsync(s => s.PhoneNumber == phoneNumber);
    }

    public async Task AddStaffAsync(Staff staff)
    {
        await _context.Staff.AddAsync(staff);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStaffAsync(Staff staff)
    {
        _context.Staff.Update(staff);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<List<Staff>> GetAllStaffsAsync()
    {
        return await _context.Staff.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }
}
