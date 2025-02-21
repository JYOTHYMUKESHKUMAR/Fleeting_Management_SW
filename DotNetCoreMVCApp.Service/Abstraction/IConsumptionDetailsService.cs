using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface IConsumptionDetailsService
    {
        Task<IEnumerable<ConsumptionDetailsViewModel>> GetAllAsync();
        Task<ConsumptionDetailsViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(ConsumptionDetailsViewModel consumptiondetails);
        Task<bool> CreateAsync(ConsumptionDetailsViewModel consumptiondetails, string userId);
        Task<bool> UpdateAsync(ConsumptionDetailsViewModel consumptiondetails, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
