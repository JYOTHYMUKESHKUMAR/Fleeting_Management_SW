using DotNetCoreMVCApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreMVCApp.Models.Repository
{
    [Table(nameof(Driver))]
    [Index(nameof(QatarId), IsUnique = true)]
    public class Driver
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Qatar ID must be exactly 11 digits")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Qatar ID must contain only numbers and be exactly 11 digits")]
        [Display(Name = "Qatar ID")]
        public string QatarId { get; set; }

        [MaxLength(20)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Required]
        [Display(Name = "QID Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime QIDExpiryDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nationality { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}