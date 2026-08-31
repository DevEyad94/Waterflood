using BackEndWaterFloodApp.Data.Seeder;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Models;
using BackEndWaterFloodApp.Models.zsk;
using Microsoft.EntityFrameworkCore;
using ZSK.Services.ReferenceData.Entities;

namespace BackEndWaterFloodApp.Data;

public class WaterfloodDbContext : DbContext
{
    public WaterfloodDbContext(DbContextOptions<WaterfloodDbContext> options)
        : base(options) { }

    public DbSet<zRole> zRoles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<ZskRefWellType> ZskRefWellTypes { get; set; }
    public DbSet<ZskRefWellStatus> ZskRefWellStatuses { get; set; }
    public DbSet<ZskRefRelationshipStatus> ZskRefRelationshipStatuses { get; set; }
    public DbSet<ZskRefMonitoringRule> ZskRefMonitoringRules { get; set; }

    public DbSet<WaterfloodRecord> WaterfloodRecords { get; set; }
    public DbSet<WaterfloodMeasurementHistory> WaterfloodMeasurementHistories { get; set; }
    public DbSet<InjectorProducerRelationship> InjectorProducerRelationships { get; set; }
    public DbSet<AlertThreshold> AlertThresholds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WaterfloodRecord>(entity =>
        {
            entity.HasIndex(e => e.WellName).IsUnique();
            entity.HasIndex(e => e.FieldName);
            entity.HasIndex(e => e.MeasurementDate);

            entity.Property(e => e.InjectionRate).HasPrecision(12, 2);
            entity.Property(e => e.OilProductionRate).HasPrecision(12, 2);
            entity.Property(e => e.WaterProductionRate).HasPrecision(12, 2);
            entity.Property(e => e.WaterCut).HasPrecision(5, 2);
            entity.Property(e => e.InjectionPressure).HasPrecision(10, 2);
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);

            entity.HasOne(e => e.WellType)
                .WithMany()
                .HasForeignKey(e => e.WellTypeCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WellStatus)
                .WithMany()
                .HasForeignKey(e => e.WellStatusCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InjectorProducerRelationship>(entity =>
        {
            entity.Property(e => e.Distance).HasPrecision(8, 2);

            entity.HasOne(r => r.InjectorWell)
                .WithMany()
                .HasForeignKey(r => r.InjectorWellId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ProducerWell)
                .WithMany()
                .HasForeignKey(r => r.ProducerWellId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.RelationshipStatus)
                .WithMany()
                .HasForeignKey(r => r.RelationshipStatusCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WaterfloodMeasurementHistory>(entity =>
        {
            entity.HasIndex(e => e.WellRecordId);
            entity.HasIndex(e => e.MeasurementDate);
            entity.Property(e => e.InjectionRate).HasPrecision(12, 2);
            entity.Property(e => e.OilProductionRate).HasPrecision(12, 2);
            entity.Property(e => e.WaterProductionRate).HasPrecision(12, 2);
            entity.Property(e => e.WaterCut).HasPrecision(5, 2);
            entity.Property(e => e.InjectionPressure).HasPrecision(10, 2);

            entity
                .HasOne(e => e.WellRecord)
                .WithMany()
                .HasForeignKey(e => e.WellRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AlertThreshold>(entity =>
        {
            entity.Property(e => e.MaxWaterCutPercent).HasPrecision(5, 2);
            entity.Property(e => e.MinOilProductionRate).HasPrecision(12, 2);
            entity.Property(e => e.MinInjectionRate).HasPrecision(12, 2);
            entity.Property(e => e.MaxInjectionPressure).HasPrecision(10, 2);
            entity.Property(e => e.ProductionDeclinePercent).HasPrecision(5, 2);
        });

        WaterfloodDataSeeder.Seed(modelBuilder);
    }
}
