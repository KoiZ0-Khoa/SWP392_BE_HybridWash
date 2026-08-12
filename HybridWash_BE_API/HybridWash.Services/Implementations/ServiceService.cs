
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.Service;
using HybridWash.Services.Interfaces;
using HybridWash.Entities.Models;

namespace HybridWash.Services.Implementations
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        public ServiceService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }


        public async Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync()
        {
            return (await _serviceRepository.GetAllServicesAsync()).Select(Map).ToList();
        }

        public async Task<IReadOnlyList<ServiceDto>> GetActiveServicesAsync()
        {
            return (await _serviceRepository.GetActiveServicesAsync()).Select(Map).ToList();
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(int serviceId)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(serviceId);
            return service == null ? null : Map(service);
        }

        public async Task<ServiceDto> CreateServiceAsync(UpsertServiceDto request)
        {
            Validate(request);

            var service = new Service
            {
                ServiceName = request.ServiceName.Trim(),
                Description = request.Description?.Trim(),
                Price = request.Price,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _serviceRepository.AddServiceAsync(service);
            return Map(service);
        }

        public async Task<bool> UpdateServiceAsync(int serviceId, UpsertServiceDto request)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(serviceId);
            if (service == null)
            {
                return false;
            }

            Validate(request);
            service.ServiceName = request.ServiceName.Trim();
            service.Description = request.Description?.Trim();
            service.Price = request.Price;
            service.IsActive = request.IsActive;

            await _serviceRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateServiceAsync(int serviceId)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(serviceId);
            if (service == null)
            {
                return false;
            }

            service.IsActive = false;
            await _serviceRepository.SaveChangesAsync();
            return true;
        }

        private static void Validate(UpsertServiceDto request)
        {
            if (string.IsNullOrWhiteSpace(request.ServiceName))
            {
                throw new ArgumentException("Service name is required.");
            }

            if (request.Price <= 0)
            {
                throw new ArgumentException("Service price must be greater than zero.");
            }
        }

        private static ServiceDto Map(Service service)
        {
            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                IsActive = service.IsActive
            };
        }
    }
}
