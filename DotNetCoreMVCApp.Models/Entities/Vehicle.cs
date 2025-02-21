using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DotNetCoreMVCApp.Models.Repository
{
    public static class VehicleTypeHelper
    {
        public const string ConcreteMixer = "Concrete Mixer";
        public const string ConcretePump = "Concrete Pump";
        public const string TruckMixer = "Truck Mixer";
        public const string Canter = "CANTER";
        public const string DumpTruck = "Dump Truck";
        public const string Head = "Head";
        public const string PickUp = "PICKUP";         // Changed from "Pickup"
        public const string SeawageWT = "Seawage-WT";
        public const string SweetWT = "Sweet -WT";
        public const string Corolla = "COROLLA";
        public const string Lancer = "LANCER";
        public const string LandCruiser = "LAND CRUISER";

        public static List<string> GetAllTypes()
        {
            return new List<string>
        {
            ConcreteMixer, ConcretePump, TruckMixer,
            Canter, DumpTruck, Head, PickUp,         // Changed from "Pickup"
            SeawageWT, SweetWT, Corolla, Lancer, LandCruiser
        };
        }
    }
    [Table(nameof(Vehicle))]
    public class Vehicle
    {
        [Key]
        [Display(Name = "Vehicle ID")]
        [Required(ErrorMessage = "Vehicle ID is required")]
        [MaxLength(20, ErrorMessage = "Vehicle ID cannot exceed 20 characters")]
        public string VehicleId { get; set; }

        [Required(ErrorMessage = "License Plate is required")]
        [MaxLength(20, ErrorMessage = "License Plate cannot exceed 20 characters")]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }

        [Required(ErrorMessage = "Woqod License Plate is required")]
        [MaxLength(20, ErrorMessage = "Woqod License Plate cannot exceed 20 characters")]
        [Display(Name = "Woqod License Plate")]
        public string WoqodLicensePlate { get; set; }

        [Required(ErrorMessage = "GPS Code is required")]
        [MaxLength(50, ErrorMessage = "GPS Code cannot exceed 50 characters")]
        [Display(Name = "GPS Code")]
        public string GPSCode { get; set; }

        [Required(ErrorMessage = "Vehicle Type is required")]
        [MaxLength(20)]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; }

        [Required(ErrorMessage = "Vehicle Brand is required")]
        [MaxLength(50, ErrorMessage = "Vehicle Brand cannot exceed 50 characters")]
        [Display(Name = "Vehicle Brand")]
        public string VehicleBrand { get; set; }

        [Required(ErrorMessage = "Vehicle Model is required")]
        [MaxLength(50, ErrorMessage = "Vehicle Model cannot exceed 50 characters")]
        [Display(Name = "Vehicle Model")]
        public string VehicleModel { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(1900, 2100, ErrorMessage = "Please enter a valid year")]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Fuel Tank Capacity is required")]
        [Range(0, 2000, ErrorMessage = "Fuel Tank Capacity must be between 0 and 2000 liters")]
        [Display(Name = "Fuel Tank Capacity (L)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelTankCapacity { get; set; }

        [Required(ErrorMessage = "Fuel Limit is required")]
        [Range(0, 2000, ErrorMessage = "Fuel Limit must be between 0 and 2000 liters")]
        [Display(Name = "Fuel Limit (L)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelLimit { get; set; }

        [Required(ErrorMessage = "Registration Expiry Date is required")]
        [Display(Name = "Registration Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime RegistrationExpiryDate { get; set; }

        [Required(ErrorMessage = "Insurance Policy Number is required")]
        [MaxLength(50, ErrorMessage = "Insurance Policy Number cannot exceed 50 characters")]
        [Display(Name = "Insurance Policy No")]
        public string InsurancePolicyNo { get; set; }

        [Required(ErrorMessage = "Insurance Policy Expiry Date is required")]
        [Display(Name = "Insurance Policy Expiry")]
        [DataType(DataType.Date)]
        public DateTime InsurancePolicyExpiry { get; set; }

        // Audit Fields
        [MaxLength(100)]
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        [MaxLength(100)]
        public string? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }

        // Only one VehicleTypeList property
        [NotMapped]
        public List<SelectListItem> VehicleTypeList
        {
            get
            {
                return VehicleTypeHelper.GetAllTypes().Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                }).ToList();
            }
        }
    }
}