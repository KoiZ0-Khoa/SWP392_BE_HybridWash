using System.Threading.Tasks;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations;

public class SystemParameterService : ISystemParameterService
{
    private readonly ISystemParameterRepository _repository;

    public SystemParameterService(ISystemParameterRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemParameterDto> GetSystemParameterAsync()
    {
        var parameter = await _repository.GetSystemParameterAsync();
        
        if (parameter == null)
        {
            return new SystemParameterDto(); // fallback
        }

        return new SystemParameterDto
        {
            BikeDepositAmount = parameter.BikeDepositAmount,
            CarDepositPercentage = parameter.CarDepositPercentage,
            ContactPhone = parameter.ContactPhone,
            CancellationRefundDays = parameter.CancellationRefundDays
        };
    }

    public async Task<SystemParameterDto> UpdateSystemParameterAsync(SystemParameterUpdateDto updateDto)
    {
        var parameter = await _repository.GetSystemParameterAsync();
        
        if (parameter == null)
        {
            parameter = new Entities.Models.SystemParameter { Id = 1 };
            await _repository.AddSystemParameterAsync(parameter);
        }

        parameter.BikeDepositAmount = updateDto.BikeDepositAmount;
        parameter.CarDepositPercentage = updateDto.CarDepositPercentage;
        parameter.ContactPhone = updateDto.ContactPhone;
        parameter.CancellationRefundDays = updateDto.CancellationRefundDays;

        await _repository.SaveChangesAsync();

        return new SystemParameterDto
        {
            BikeDepositAmount = parameter.BikeDepositAmount,
            CarDepositPercentage = parameter.CarDepositPercentage,
            ContactPhone = parameter.ContactPhone,
            CancellationRefundDays = parameter.CancellationRefundDays
        };
    }
}
