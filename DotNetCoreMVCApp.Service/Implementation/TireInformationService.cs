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
using DotNetCoreMVCApp.Models.Entities;

namespace DotNetCoreMVCApp.Service.Implementation
{
    public class TireInformationService : ITireInformationService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public TireInformationService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(TireInformationService));
        }

        public async Task<IEnumerable<TireInformationViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<TireInformationViewModel>>(await _unitOfWork.TireInformationRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.Id))));
        }

        public async Task<TireInformationViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<TireInformationViewModel>(await _unitOfWork.TireInformationRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(TireInformationViewModel tireinformationModel, string userId)
        {
            _logger.Info($"tireinformation create request by user: {userId} : {JsonConvert.SerializeObject(tireinformationModel)}");
            var tireinformation = _mapper.Map<TireInformation>(tireinformationModel);
            tireinformation.CreatedBy = userId;
            tireinformation.CreatedOn = DateTime.Now;
            await _unitOfWork.TireInformationRepository.InsertAsync(tireinformation);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created TireInformation");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"TireInformation delete request by user: {userId} : tireinformation Id: {id}");
            var tireinformation= await _unitOfWork.TireInformationRepository.GetByIdAsync(id);
            tireinformation.IsDeleted = true;
            tireinformation.DeletedBy = userId;
            tireinformation.DeletedOn = DateTime.Now;

            _unitOfWork.TireInformationRepository.Update(tireinformation);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(TireInformationViewModel tireinformationModel, string userId)
        {
            _logger.Info($"customer update request by user: {userId} : {JsonConvert.SerializeObject(tireinformationModel)}");
            var tireinformation = await _unitOfWork.TireInformationRepository.GetByIdAsync(tireinformationModel.Id);
         
            tireinformation.Id = tireinformationModel.Id;
            tireinformation.UpdatedBy = userId;
            tireinformation.UpdatedOn = DateTime.Now;
            _unitOfWork.TireInformationRepository.Update(tireinformation);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created tireinformation");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(TireInformationViewModel tireinformationModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.TireInformationRepository.GetAsync(filter: (c => c.Id != tireinformationModel.Id && c.IsDeleted == false && (c.Id == tireinformationModel.Id )))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("tireinformation", "tireinformation with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
