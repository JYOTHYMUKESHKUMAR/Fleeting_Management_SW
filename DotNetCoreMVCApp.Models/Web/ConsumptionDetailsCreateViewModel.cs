using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Web
{
    public class ConsumptionDetailsCreateViewModel
    {

        public int Id { get; set; }

        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }

        public string WoqodLicensePlate { get; set; }

        [NotMapped]
        public List<SelectListItem> LicensePlateList { get; set; } = new List<SelectListItem>();

        [NotMapped]
        public List<SelectListItem> WoqodLicensePlateList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Driver")]
        public int DriverId { get; set; }

        [MaxLength(100, ErrorMessage = "Driver Name cannot exceed 100 characters")]
        [Display(Name = "Driver Name")]
        public string DriverName { get; set; }

        [Required(ErrorMessage = "Fuel Type is required")]
        [MaxLength(20)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Fuel limit is required")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Offline Limit")]
        public decimal OfflineLimit { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Liters(Lt)")]
        public decimal FuelQuantity { get; set; }

        [Required(ErrorMessage = "unit price is required")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalCost { get; set; }

        [Required(ErrorMessage = "Fuel Date is required")]
        [Display(Name = "Sale Time")]
        [DataType(DataType.DateTime)]
        public DateTime SaleTime { get; set; }


        // In both ConsumptionDetails.cs and ConsumptionDetailsCreateViewModel.cs
        [Required(ErrorMessage = "Invoice Month is required")]
        [StringLength(8)]
        public string InvoiceMonth { get; set; }

        [MaxLength(50)]
        public string StationName { get; set; }

        [MaxLength(50)]
        public string GroupName { get; set; }

        [MaxLength(50)]
        public string FleetName { get; set; }

        [MaxLength(500)]
        public string CostCenter { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OdometerReading { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public IEnumerable<SelectListItem> DriverList { get; set; }
        public IEnumerable<SelectListItem> ProductNameList { get; set; }
    }
}