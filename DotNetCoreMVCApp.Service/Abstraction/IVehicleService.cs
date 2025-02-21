using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleViewModel>> GetAllAsync();
        Task<VehicleViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(VehicleViewModel Vehicle);
        Task<bool> CreateAsync(VehicleViewModel Vehicle, string userId);
        Task<bool> UpdateAsync(VehicleViewModel Vehicle, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
