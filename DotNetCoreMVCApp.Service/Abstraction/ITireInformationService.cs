using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface ITireInformationService
    {
        Task<IEnumerable<TireInformationViewModel>> GetAllAsync();
        Task<TireInformationViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(TireInformationViewModel tireinformation);
        Task<bool> CreateAsync(TireInformationViewModel tireinformation, string userId);
        Task<bool> UpdateAsync(TireInformationViewModel tireinformation, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
