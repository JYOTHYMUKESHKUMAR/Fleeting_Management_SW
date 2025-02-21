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
    public class ConsumptionDetailsService : IConsumptionDetailsService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public ConsumptionDetailsService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(ConsumptionDetailsService));
        }

        public async Task<IEnumerable<ConsumptionDetailsViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<ConsumptionDetailsViewModel>>(await _unitOfWork.ConsumptionDetailsRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.Id))));
        }

        public async Task<ConsumptionDetailsViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<ConsumptionDetailsViewModel>(await _unitOfWork.ConsumptionDetailsRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(ConsumptionDetailsViewModel consumptiondetailsModel, string userId)
        {
            _logger.Info($"fuelconsumption data create request by user: {userId} : {JsonConvert.SerializeObject(consumptiondetailsModel)}");
            var consumptiondetails = _mapper.Map<ConsumptionDetails>(consumptiondetailsModel);
            consumptiondetails.CreatedBy = userId;
            consumptiondetails.CreatedOn = DateTime.Now;
            await _unitOfWork.ConsumptionDetailsRepository.InsertAsync(consumptiondetails);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created fuellog");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"Consumption Details delete request by user: {userId} : consumptiondetails Id: {id}");
            var consumptiondetails = await _unitOfWork.ConsumptionDetailsRepository.GetByIdAsync(id);
            consumptiondetails.IsDeleted = true;
            consumptiondetails.DeletedBy = userId;
            consumptiondetails.DeletedOn = DateTime.Now;

            _unitOfWork.ConsumptionDetailsRepository.Update(consumptiondetails);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(ConsumptionDetailsViewModel consumptiondetailsModel, string userId)
        {
            _logger.Info($"fuel consumption detail update request by user: {userId} : {JsonConvert.SerializeObject(consumptiondetailsModel)}");
            var consumptiondetails = await _unitOfWork.ConsumptionDetailsRepository.GetByIdAsync(consumptiondetailsModel.Id);

            consumptiondetails.Id = consumptiondetailsModel.Id;
            consumptiondetails.UpdatedBy = userId;
            consumptiondetails.UpdatedOn = DateTime.Now;
            _unitOfWork.ConsumptionDetailsRepository.Update(consumptiondetails);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created fuellog");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(ConsumptionDetailsViewModel consumptiondetailsModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.ConsumptionDetailsRepository.GetAsync(filter: (c => c.Id != consumptiondetailsModel.Id && c.IsDeleted == false && (c.Id == consumptiondetailsModel.Id)))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("fuellog", "fuellog with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
