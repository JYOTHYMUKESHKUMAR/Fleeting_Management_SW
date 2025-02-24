using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DotNetCoreMVCApp.Models.Entities
{
    [Table(nameof(TireInformation))]
    public class TireInformation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Tire Serial Number")]
        public string TireSerialNumber { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Tire Size")]
        public string TireSize { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Tire Brand")]
        public string TireBrand { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Tire Price")]
        public decimal TirePrice { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Supplier")]
        public string Supplier { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Vehicle Information")]
        
        public string LicensePlate { get; set; }


        [NotMapped]
        public List<SelectListItem> LicensePlateList { get; set; } = new List<SelectListItem>();

        [Required]
        [Display(Name = "Installation Date")]
        [DataType(DataType.DateTime)]
        public DateTime InstallationDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Installation Odometer")]
        public decimal InstallationOdometer { get; set; }

        [Display(Name = "Removal Date")]
        [DataType(DataType.DateTime)]
        public DateTime? RemovalDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Removal Odometer")]
        public decimal? RemovalOdometer { get; set; }

        [Display(Name = "Is Deactivated")]
        public bool IsDeactivated { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        [MaxLength(100)]
        public string? DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}