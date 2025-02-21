using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Entities
{
    public class ImportResultModel
    {
        public string VehicleId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public ImportResultModel(string vehicleId, bool success, string message)
        {
            VehicleId = vehicleId;
            Success = success;
            Message = message;
        }
    }
}
