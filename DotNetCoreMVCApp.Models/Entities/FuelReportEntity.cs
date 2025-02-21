using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Repository
{
    [Table("vw_ComprehensiveConsumptionReport")]
    public class FuelReportEntity
    {
        [Key]
        public int ConsumptionDetailsId { get; set; }

        [Required]
        [Display(Name = "SaleTime")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime SaleTime { get; set; }

        [Required]
        [Display(Name = "License Plate")]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [Required]
        [Display(Name = "Vehicle ID")]
        [StringLength(50)]
        public string VehicleId { get; set; }

        [Required]
        [Display(Name = "Vehicle Type")]
        [StringLength(50)]
        public string VehicleType { get; set; }

        [Display(Name = "Driver ID")]
        public int DriverId { get; set; }

        [Required]
        [Display(Name = "Driver Name")]
        [StringLength(100)]
        public string DriverName { get; set; }

        [Display(Name = "Qatar ID")]
        [StringLength(20)]
        public string DriverQatarId { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(20)]
        public string DriverContact { get; set; }

        [Required]
        [Display(Name = "ProductName")]
        [StringLength(20)]
        public string ProductName { get; set; }

        
        [Display(Name = "Odometer Reading")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal OdometerReading { get; set; }

        [Required]
        [Display(Name = "Fuel Quantity (L)")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal FuelQuantity { get; set; }

        [Required]
        [Display(Name = "Unit Price")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalCost { get; set; }

        [Display(Name = "Fuel Station")]
        [StringLength(100)]
        public string StationName{ get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        [Display(Name = "Distance Since Last (km)")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal DistanceSinceLastRefill { get; set; }

        [Display(Name = "Days Since Last Refill")]
        public int DaysSinceLastRefill { get; set; }

        [Display(Name = "Km/L")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal KmPerLiter { get; set; }

        [Display(Name = "Vehicle Total Refills")]
        public int VehicleTotalRefills { get; set; }

        [Display(Name = "Vehicle Total Fuel (L)")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal VehicleTotalFuel { get; set; }

        [Display(Name = "Vehicle Total Cost")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal VehicleTotalCost { get; set; }

        [Display(Name = "Vehicle Total Distance (km)")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal VehicleTotalDistance { get; set; }

        [Display(Name = "Driver Total Refills")]
        public int DriverTotalRefills { get; set; }

        [Display(Name = "Driver Total Fuel (L)")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal DriverTotalFuel { get; set; }

        [Display(Name = "Driver Total Cost")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal DriverTotalCost { get; set; }

        [Display(Name = "Driver Vehicle Count")]
        public int DriverVehicleCount { get; set; }

        [Display(Name = "Vehicle Fuel Limit")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal VehicleFuelLimit { get; set; }

        [Display(Name = "Fuel Limit Status")]
        [StringLength(20)]
        public string FuelLimitStatus { get; set; }

        [Display(Name = "Monthly Total Fuel")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal MonthlyTotalFuel { get; set; }

        [Display(Name = "Monthly Total Cost")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal MonthlyTotalCost { get; set; }

        // Helper properties for UI
        [NotMapped]
        public string StatusClass => FuelLimitStatus == "Exceeded" ? "text-danger" : "text-success";

        [NotMapped]
        public string FormattedCost => $"QAR {TotalCost:N2}";
    }

    // Filter class for search
    public class FuelReportFilter
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Driver")]
        public int? DriverId { get; set; }

        [Display(Name = "Vehicle")]
        public string LicensePlate { get; set; }

        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; }

        [Display(Name = "Product name")]
        public string ProductName { get; set; }

        public void SetDefaultDateRange()
        {
            // Removed default date assignments
        }
    }

    // View Model for the report page
    public class FuelReportViewModel
    {
        public List<FuelReportEntity> Details { get; set; } = new();

        public FuelReportFilter Filter { get; set; } = new();

        [NotMapped]
        public List<SelectListItem> DriversList { get; set; } = new();

        [NotMapped]
        public List<SelectListItem> VehiclesList { get; set; } = new();

        [NotMapped]
        public List<SelectListItem> VehicleTypesList { get; set; } = new();

        [NotMapped]
        public List<SelectListItem> ProductNamesList { get; set; } = new();

        public string DateRangeDisplay =>
            Filter.StartDate.HasValue && Filter.EndDate.HasValue
                ? $"{Filter.StartDate.Value:dd/MM/yyyy} - {Filter.EndDate.Value:dd/MM/yyyy}"
                : "All Dates";

        // Summary properties
        public int TotalRefills => Details.Count;

        public decimal TotalFuelConsumed => Details.Sum(x => x.FuelQuantity);

        public decimal TotalDistance => Details.Sum(x => x.DistanceSinceLastRefill);

        public decimal TotalCost => Details.Sum(x => x.TotalCost);

        public decimal AverageKmPerLiter =>
            TotalFuelConsumed > 0 ? TotalDistance / TotalFuelConsumed : 0;

        public int UniqueVehicles => Details.Select(x => x.LicensePlate).Distinct().Count();

        public int UniqueDrivers => Details.Select(x => x.DriverId).Distinct().Count();

        public decimal AverageCostPerLiter =>
            TotalFuelConsumed > 0 ? TotalCost / TotalFuelConsumed : 0;

        public Dictionary<string, decimal> GetProductNameBreakdown() =>
            Details.GroupBy(x => x.ProductName)
                   .ToDictionary(g => g.Key, g => g.Sum(x => x.FuelQuantity));

        public Dictionary<string, decimal> GetVehicleTypeBreakdown() =>
            Details.GroupBy(x => x.VehicleType)
                   .ToDictionary(g => g.Key, g => g.Sum(x => x.FuelQuantity));
    }
}