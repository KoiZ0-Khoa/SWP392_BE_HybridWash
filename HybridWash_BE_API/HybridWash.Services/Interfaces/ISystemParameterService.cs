using System.Threading.Tasks;
using HybridWash.Services.DTOs;

namespace HybridWash.Services.Interfaces;

public interface ISystemParameterService
{
    Task<SystemParameterDto> GetSystemParameterAsync();
    Task<SystemParameterDto> UpdateSystemParameterAsync(SystemParameterUpdateDto updateDto);
}
