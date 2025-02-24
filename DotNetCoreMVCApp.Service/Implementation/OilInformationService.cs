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
    public class OilInformationService : IOilInformationService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public OilInformationService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(OilInformationService));
        }

        public async Task<IEnumerable<OilInformationViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<OilInformationViewModel>>(await _unitOfWork.OilInformationRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.Id))));
        }

        public async Task<OilInformationViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<OilInformationViewModel>(await _unitOfWork.OilInformationRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(OilInformationViewModel oilinformationModel, string userId)
        {
            _logger.Info($"oilinformation create request by user: {userId} : {JsonConvert.SerializeObject(oilinformationModel)}");
            var oilinformation = _mapper.Map<OilInformation>(oilinformationModel);
            oilinformation.CreatedBy = userId;
            oilinformation.CreatedOn = DateTime.Now;
            await _unitOfWork.OilInformationRepository.InsertAsync(oilinformation);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created OilInformation");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"Oil info delete request by user: {userId} : oilinformation Id: {id}");
            var oilinformation= await _unitOfWork.OilInformationRepository.GetByIdAsync(id);
            oilinformation.IsDeleted = true;
            oilinformation.DeletedBy = userId;
            oilinformation.DeletedOn = DateTime.Now;

            _unitOfWork.OilInformationRepository.Update(oilinformation);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(OilInformationViewModel oilinformationModel, string userId)
        {
            _logger.Info($"customer update request by user: {userId} : {JsonConvert.SerializeObject(oilinformationModel)}");
            var oilinformation = await _unitOfWork.OilInformationRepository.GetByIdAsync(oilinformationModel.Id);
         
            oilinformation.Id= oilinformationModel.Id;
            oilinformation.UpdatedBy = userId;
            oilinformation.UpdatedOn = DateTime.Now;
            _unitOfWork.OilInformationRepository.Update(oilinformation);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created oilinformation");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(OilInformationViewModel oilinformationModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.OilInformationRepository.GetAsync(filter: (c => c.Id != oilinformationModel.Id && c.IsDeleted == false && (c.Id == oilinformationModel.Id )))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("oilinformation", "oilinformation with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
