using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Web
{
    public class UniqueQatarIdAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dbContext = (ApplicationDbContext)validationContext
                .GetService(typeof(ApplicationDbContext));
            var qatarId = value as string;
            var driverViewModel = validationContext.ObjectInstance as DriverCreateViewModel;
            var exists = dbContext.DriverSet.Any(d =>
                d.QatarId == qatarId &&
                !d.IsDeleted &&
                d.Id != driverViewModel.Id);  // Exclude current driver when updating
            if (exists)
            {
                return new ValidationResult("This Qatar ID is already registered with another driver.");
            }
            return ValidationResult.Success;
        }
    }

    public class DriverCreateViewModel
    {
        [Required(ErrorMessage = "Driver ID is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Driver Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Qatar ID is required")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Qatar ID must be exactly 11 digits")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Qatar ID must contain only numbers and be exactly 11 digits")]
        [Display(Name = "Qatar ID")]
        [UniqueQatarId]
        public string QatarId { get; set; }

        [MaxLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "QID Expiry Date is required")]
        [Display(Name = "QID Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime QIDExpiryDate { get; set; }

        [Required(ErrorMessage = "Nationality is required")]
        [MaxLength(50, ErrorMessage = "Nationality cannot exceed 50 characters")]
        public string Nationality { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Display(Name = "Is Deactivated")]
        public bool IsDeactivated { get; set; }
    }
}