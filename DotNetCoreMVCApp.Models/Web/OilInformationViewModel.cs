using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Web
{
    public class OilInformationViewModel
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(100)]
        [Display(Name = "Car Information")]
        public string LicensePlate { get; set; }


        [NotMapped]
        public List<SelectListItem> LicensePlateList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Oil Type")]
        public string OilType { get; set; }

        [Display(Name = "Oil Brand")]
        public string OilBrand { get; set; }

        [Display(Name = "Oil Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal OilPrice { get; set; }

        [Display(Name = "Current Odometer")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal CurrentOdometer { get; set; }

        [Display(Name = "Next Oil Change Odometer")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal NextOilChangeOdometer { get; set; }

        [Display(Name = "Oil Change Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime OilChangeDate { get; set; }

        [Display(Name = "Next Oil Change Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? NextOilChangeDate { get; set; }

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
        public IEnumerable<SelectListItem> OilTypeList { get; set; }
    }
}