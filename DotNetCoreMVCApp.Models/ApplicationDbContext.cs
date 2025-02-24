using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace DotNetCoreMVCApp.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Driver> DriverSet { get; set; }
        public DbSet<Vehicle> VehicleSet { get; set; }
        
        public DbSet<FuelReportEntity> FuelReportEntitySet { get; set; }
        //public DbSet<ConsumptionReportEntity> ConsumptionReportEntitySet { get; set; }
        public DbSet<ConsumptionDetails> ConsumptionDetails { get; set; }
        public DbSet<TireInformation> TireInformationSet { get; set; }
        public DbSet<OilInformation> OilInformationSet { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FuelReportEntity>()
           .ToView("vw_ComprehensiveFuelReport")
           .HasNoKey();

            /*// Configure Driver entity
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.ToTable("Driver");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedBy)
                    .IsRequired();

                entity.Property(e => e.CreatedOn)
                    .IsRequired();

                
                
            });
*/
            // Configure Identity tables
            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.HasKey(r => new { r.UserId, r.RoleId });
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            });

            // Configure cascade delete behavior
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}