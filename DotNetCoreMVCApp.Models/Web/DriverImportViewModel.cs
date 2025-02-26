using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Web
{
    public class DriverImportViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Driver Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Qatar ID is required")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Qatar ID must be exactly 11 digits")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Qatar ID must contain only numbers and be exactly 11 digits")]
        [Display(Name = "Qatar ID")]
        [UniqueQatarId]
        public string QatarId { get; set; }

        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Display(Name = "QID Expiry Date")]
        public DateTime QIDExpiryDate { get; set; }

        [Display(Name = "Nationality")]
        public string Nationality { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        [Display(Name = "Is Deactivated")]
        public bool IsDeactivated { get; set; }
    }
}