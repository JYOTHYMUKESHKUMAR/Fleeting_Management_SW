using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface IDriverService
    {
        Task<IEnumerable<DriverViewModel>> GetAllAsync();
        Task<DriverViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(DriverViewModel driver);
        Task<bool> CreateAsync(DriverViewModel driver, string userId);
        Task<bool> UpdateAsync(DriverViewModel driver, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
