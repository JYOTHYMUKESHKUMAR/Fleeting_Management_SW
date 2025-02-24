using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Web
{
    public class TireInformationViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Serial Number")]
        public string TireSerialNumber { get; set; }

        [Display(Name = "Size")]
        public string TireSize { get; set; }

        [Display(Name = "Brand")]
        public string TireBrand { get; set; }

        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TirePrice { get; set; }

        public string Supplier { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Vehicle Information")]

        public string LicensePlate { get; set; }


        [NotMapped]
        public List<SelectListItem> LicensePlateList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Installation Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime InstallationDate { get; set; }

        [Display(Name = "Installation Odometer")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal InstallationOdometer { get; set; }

        [Display(Name = "Removal Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? RemovalDate { get; set; }

        [Display(Name = "Removal Odometer")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? RemovalOdometer { get; set; }

        [Display(Name = "Is Deactivated")]
        public bool IsDeactivated { get; set; }

        public string? Notes { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? CreatedOn { get; set; }

        [Display(Name = "Last Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Last Updated On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedOn { get; set; }

        [Display(Name = "Is Deleted")]
        public bool IsDeleted { get; set; }

        [Display(Name = "Deleted By")]
        public string? DeletedBy { get; set; }

        [Display(Name = "Deleted On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? DeletedOn { get; set; }
    }
}