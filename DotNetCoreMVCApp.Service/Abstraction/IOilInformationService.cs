using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface IOilInformationService
    {
        Task<IEnumerable<OilInformationViewModel>> GetAllAsync();
        Task<OilInformationViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(OilInformationViewModel oilinformation);
        Task<bool> CreateAsync(OilInformationViewModel oilinformation, string userId);
        Task<bool> UpdateAsync(OilInformationViewModel oilinformation, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
