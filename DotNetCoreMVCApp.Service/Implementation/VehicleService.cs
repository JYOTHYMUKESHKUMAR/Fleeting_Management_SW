using AutoMapper;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Repository.Implementation;
using DotNetCoreMVCApp.Service.Abstraction;

namespace DotNetCoreMVCApp.Service.Implementation
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleViewModel>> GetAllAsync();
        Task<VehicleViewModel> GetByIdAsync(string id);
        Task<bool> CreateAsync(VehicleCreateViewModel vehicleModel, string userId);
        Task<bool> DeleteAsync(string id, string userId);
        Task<bool> UpdateAsync(VehicleViewModel vehicleModel, string userId);
        Task<ErrorStateModel> ValidateAsync(VehicleViewModel vehicleModel);
    }

    public class VehicleService : IVehicleService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public VehicleService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(VehicleService));
        }

        public async Task<IEnumerable<VehicleViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<VehicleViewModel>>(
                await _unitOfWork.VehicleRepository.GetAsync(
                    filter: c => c.IsDeleted == false,
                    orderBy: c => c.OrderBy(a => a.VehicleId)
                )
            );
        }

        public async Task<VehicleViewModel> GetByIdAsync(string id)
        {
            return _mapper.Map<VehicleViewModel>(
                await _unitOfWork.VehicleRepository.GetByIdAsync(id)
            );
        }

        public async Task<bool> CreateAsync(VehicleCreateViewModel vehicleModel, string userId)
        {
            _logger.Info($"Vehicle create request by user: {userId} : {JsonConvert.SerializeObject(vehicleModel)}");

            var vehicle = _mapper.Map<Vehicle>(vehicleModel);
            vehicle.CreatedBy = userId;
            vehicle.CreatedOn = DateTime.Now;

            await _unitOfWork.VehicleRepository.InsertAsync(vehicle);
            await _unitOfWork.SaveAsync();

            _logger.Info($"Created Vehicle");
            return true;
        }

        public async Task<bool> DeleteAsync(string id, string userId)
        {
            _logger.Info($"Vehicle delete request by user: {userId} : Vehicle Id: {id}");

            var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(id);
            vehicle.IsDeleted = true;
            vehicle.DeletedBy = userId;
            vehicle.DeletedOn = DateTime.Now;

            _unitOfWork.VehicleRepository.Update(vehicle);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(VehicleViewModel vehicleModel, string userId)
        {
            _logger.Info($"Vehicle update request by user: {userId} : {JsonConvert.SerializeObject(vehicleModel)}");

            var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(vehicleModel.VehicleId);

            vehicle.LicensePlate = vehicleModel.LicensePlate;
            vehicle.VehicleType = vehicleModel.VehicleType;
            vehicle.UpdatedBy = userId;
            vehicle.UpdatedOn = DateTime.Now;

            _unitOfWork.VehicleRepository.Update(vehicle);
            await _unitOfWork.SaveAsync();

            _logger.Info($"Updated Vehicle");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(VehicleViewModel vehicleModel)
        {
            ErrorStateModel errorStateModel = new();

            // Check if vehicle with same ID or license plate exists
            var existingVehicles = await _unitOfWork.VehicleRepository.GetAsync(
                filter: c => c.IsDeleted == false &&
                            c.VehicleId != vehicleModel.VehicleId &&
                            (c.VehicleId == vehicleModel.VehicleId ||
                             c.LicensePlate == vehicleModel.LicensePlate)
            );

            errorStateModel.IsValid = !existingVehicles.Any();

            if (!errorStateModel.IsValid)
            {
                if (existingVehicles.Any(v => v.VehicleId == vehicleModel.VehicleId))
                {
                    errorStateModel.Errors.Add("vehicleId", "Vehicle ID already exists.");
                }
                if (existingVehicles.Any(v => v.LicensePlate == vehicleModel.LicensePlate))
                {
                    errorStateModel.Errors.Add("licensePlate", "License Plate already exists.");
                }
            }

            return errorStateModel;
        }
    }
}