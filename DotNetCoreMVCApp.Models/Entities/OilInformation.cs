using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DotNetCoreMVCApp.Models.Entities
{
    public static class OilTypeHelper
    {
        public const string Synthetic = "Synthetic";
        public const string SemiSynthetic = "Semi-Synthetic";
        public const string Mineral = "Mineral";
        public const string HighMileage = "High Mileage";
        public const string Conventional = "Conventional";

        public static List<string> GetAllTypes()
        {
            return new List<string> { Synthetic, SemiSynthetic, Mineral, HighMileage, Conventional };
        }
    }

    [Table(nameof(OilInformation))]
    public class OilInformation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Car Information")]
        public string LicensePlate { get; set; }

       
        [NotMapped]
        public List<SelectListItem> LicensePlateList { get; set; } = new List<SelectListItem>();

        [Required]
        [MaxLength(50)]
        [Display(Name = "Oil Type")]
        public string OilType { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Oil Brand")]
        public string OilBrand { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Oil Price")]
        public decimal OilPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Current Odometer")]
        public decimal CurrentOdometer { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Next Oil Change Odometer")]
        public decimal NextOilChangeOdometer { get; set; }

        [Required]
        [Display(Name = "Oil Change Date")]
        [DataType(DataType.DateTime)]
        public DateTime OilChangeDate { get; set; }

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