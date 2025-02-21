using AutoMapper;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Repository.Implementation;
using DotNetCoreMVCApp.Service.Abstraction;

namespace DotNetCoreMVCApp.Service.Implementation
{
    public class DriverService : IDriverService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public DriverService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(DriverService));
        }

        public async Task<IEnumerable<DriverViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<DriverViewModel>>(await _unitOfWork.DriverRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.Name))));
        }

        public async Task<DriverViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<DriverViewModel>(await _unitOfWork.DriverRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(DriverViewModel driverModel, string userId)
        {
            _logger.Info($"driver create request by user: {userId} : {JsonConvert.SerializeObject(driverModel)}");
            var driver = _mapper.Map<Driver>(driverModel);
            driver.CreatedBy = userId;
            driver.CreatedOn = DateTime.Now;
            await _unitOfWork.DriverRepository.InsertAsync(driver);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created Driver");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"Driver delete request by user: {userId} : driver Id: {id}");
            var driver= await _unitOfWork.DriverRepository.GetByIdAsync(id);
            driver.IsDeleted = true;
            driver.DeletedBy = userId;
            driver.DeletedOn = DateTime.Now;

            _unitOfWork.DriverRepository.Update(driver);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(DriverViewModel driverModel, string userId)
        {
            _logger.Info($"customer update request by user: {userId} : {JsonConvert.SerializeObject(driverModel)}");
            var driver = await _unitOfWork.DriverRepository.GetByIdAsync(driverModel.Id);
         
            driver.Name = driverModel.Name;
            driver.UpdatedBy = userId;
            driver.UpdatedOn = DateTime.Now;
            _unitOfWork.DriverRepository.Update(driver);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created driver");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(DriverViewModel driverModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.DriverRepository.GetAsync(filter: (c => c.Id != driverModel.Id && c.IsDeleted == false && (c.Name == driverModel.Name )))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("driver", "driver with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
