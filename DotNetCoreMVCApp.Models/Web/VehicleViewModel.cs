﻿using DotNetCoreMVCApp.Models.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DotNetCoreMVCApp.Models.Web
{
    public class VehicleViewModel
    {
        [Display(Name = "Vehicle ID")]
        [Required(ErrorMessage = "Vehicle ID is required")]
        [MaxLength(20, ErrorMessage = "Vehicle ID cannot exceed 20 characters")]
        public string VehicleId { get; set; }

        [Display(Name = "License Plate")]
        [Required(ErrorMessage = "License Plate is required")]
        [MaxLength(20, ErrorMessage = "License Plate cannot exceed 20 characters")]
        public string LicensePlate { get; set; }

        [Display(Name = "Woqod License Plate")]
        [Required(ErrorMessage = "Woqod License Plate is required")]
        [MaxLength(20, ErrorMessage = "Woqod License Plate cannot exceed 20 characters")]
        public string WoqodLicensePlate { get; set; }

        [Display(Name = "GPS Code")]
        [Required(ErrorMessage = "GPS Code is required")]
        [MaxLength(50, ErrorMessage = "GPS Code cannot exceed 50 characters")]
        public string GPSCode { get; set; }

        [Display(Name = "Vehicle Type")]
        [Required(ErrorMessage = "Vehicle Type is required")]
        public string VehicleType { get; set; }

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; }

        [Display(Name = "Vehicle Brand")]
        [Required(ErrorMessage = "Vehicle Brand is required")]
        [MaxLength(50, ErrorMessage = "Vehicle Brand cannot exceed 50 characters")]
        public string VehicleBrand { get; set; }

        [Display(Name = "Vehicle Model")]
        [Required(ErrorMessage = "Vehicle Model is required")]
        [MaxLength(50, ErrorMessage = "Vehicle Model cannot exceed 50 characters")]
        public string VehicleModel { get; set; }

        [Display(Name = "Year")]
        [Required(ErrorMessage = "Year is required")]
        [Range(1900, 2100, ErrorMessage = "Please enter a valid year")]
        public int Year { get; set; }

        [Display(Name = "Fuel Tank Capacity (L)")]
        [Required(ErrorMessage = "Fuel Tank Capacity is required")]
        [Range(0, 2000, ErrorMessage = "Fuel Tank Capacity must be between 0 and 2000 liters")]
        [DisplayFormat(DataFormatString = "{0:N2}")] // Format with 2 decimal places
        public decimal FuelTankCapacity { get; set; }

        [Display(Name = "Fuel Limit (L)")]
        [Required(ErrorMessage = "Fuel Limit is required")]
        [Range(0, 2000, ErrorMessage = "Fuel Limit must be between 0 and 2000 liters")]
        [DisplayFormat(DataFormatString = "{0:N2}")] // Format with 2 decimal places
        public decimal FuelLimit { get; set; }

        [Display(Name = "Registration Expiry Date")]
        [Required(ErrorMessage = "Registration Expiry Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime RegistrationExpiryDate { get; set; }

        [Display(Name = "Insurance Policy No")]
        [Required(ErrorMessage = "Insurance Policy Number is required")]
        [MaxLength(50, ErrorMessage = "Insurance Policy Number cannot exceed 50 characters")]
        public string InsurancePolicyNo { get; set; }

        [Display(Name = "Insurance Policy Expiry")]
        [Required(ErrorMessage = "Insurance Policy Expiry Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime InsurancePolicyExpiry { get; set; }

        [Display(Name = "Is Deactivated")]
        public bool IsDeactivated { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }

        [Display(Name = "Updated On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        [Display(Name = "Deleted By")]
        public string DeletedBy { get; set; }

        [Display(Name = "Deleted On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? DeletedOn { get; set; }

        // List for dropdown (used in create/edit forms)
        public IEnumerable<SelectListItem> VehicleTypeList { get; set; }

        public VehicleViewModel()
        {
            VehicleTypeList = VehicleTypeHelper.GetAllTypes().Select(x => new SelectListItem
            {
                Text = x,
                Value = x
            });
        }
    }
}