
using DotNetCoreMVCApp.Models.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Web
{
    public class ConsumptionDetailsViewModel
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; }
        public string WoqodLicensePlate { get; set; }
        public List<SelectListItem> LicensePlateList { get; set; }
        public List<SelectListItem> WoqodLicensePlateList { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public string ProductName { get; set; }
        public decimal OfflineLimit { get; set; }
        public decimal FuelQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime SaleTime { get; set; }
        public string InvoiceMonth { get; set; }
        public string StationName { get; set; }
        public string GroupName { get; set; }
        public string FleetName { get; set; }
        public string CostCenter { get; set; }
        public decimal? OdometerReading { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public virtual Driver Driver { get; set; }
    

    // Lists for dropdowns (used in create/edit forms)
        public IEnumerable<SelectListItem> DriverList { get; set; }
        public IEnumerable<SelectListItem> ProductNameList { get; set; }
    }
}