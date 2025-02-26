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
    public class DriverViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Qatar ID")]
        public string QatarId { get; set; }

        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Display(Name = "QID Expiry Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime QIDExpiryDate { get; set; }

        public string Nationality { get; set; }

        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DOB { get; set; }

        [Display(Name = "Is Deactivated")]
        public bool IsDeactivated { get; set; }

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
    }
}