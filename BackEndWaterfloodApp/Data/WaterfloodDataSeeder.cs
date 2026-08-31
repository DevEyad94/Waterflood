using BackEndWaterFloodApp.Models.zsk;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ZSK.Services.ReferenceData.Entities;

namespace BackEndWaterFloodApp.Data.Seeder;

public static class WaterfloodDataSeeder
{
    private static readonly DateTime SeedCreatedAt = new(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<zRole>().HasData(SeedData<zRole>(@"Data/Seeder/zsk/zRole.json"));

        modelBuilder
            .Entity<ZskRefWellType>()
            .HasData(SeedData<ZskRefWellType>(@"Data/Seeder/zsk/zskRefWellType.json"));
        modelBuilder
            .Entity<ZskRefWellStatus>()
            .HasData(SeedData<ZskRefWellStatus>(@"Data/Seeder/zsk/zskRefWellStatus.json"));
        modelBuilder
            .Entity<ZskRefRelationshipStatus>()
            .HasData(
                SeedData<ZskRefRelationshipStatus>(@"Data/Seeder/zsk/zskRefRelationshipStatus.json")
            );
        modelBuilder
            .Entity<ZskRefMonitoringRule>()
            .HasData(
                SeedData<ZskRefMonitoringRule>(@"Data/Seeder/zsk/zskRefMonitoringRules.json")
            );

        var records = SeedData<WaterfloodSeedRecord>(@"Data/Seeder/waterfloodRecords.json");
        modelBuilder
            .Entity<Domain.Entities.WaterfloodRecord>()
            .HasData(
                records.Select(r => new Domain.Entities.WaterfloodRecord
                {
                    Id = Guid.Parse(r.Id),
                    WellName = r.WellName,
                    WellTypeCode = r.WellTypeCode,
                    FieldName = r.FieldName,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    InjectionRate = r.InjectionRate,
                    OilProductionRate = r.OilProductionRate,
                    WaterProductionRate = r.WaterProductionRate,
                    WaterCut = r.WaterCut,
                    InjectionPressure = r.InjectionPressure,
                    WellStatusCode = r.WellStatusCode,
                    MeasurementDate = r.MeasurementDate,
                    CreatedAt = SeedCreatedAt,
                    CreatedBy = "System",
                })
            );

        var relationships = SeedData<RelationshipSeedRecord>(
            @"Data/Seeder/injectorProducerRelationships.json"
        );
        modelBuilder
            .Entity<Domain.Entities.InjectorProducerRelationship>()
            .HasData(
                relationships.Select(r => new Domain.Entities.InjectorProducerRelationship
                {
                    Id = Guid.Parse(r.Id),
                    InjectorWellId = Guid.Parse(r.InjectorWellId),
                    ProducerWellId = Guid.Parse(r.ProducerWellId),
                    Distance = r.Distance,
                    RelationshipStatusCode = r.RelationshipStatusCode,
                    CreatedAt = SeedCreatedAt,
                    CreatedBy = "System",
                })
            );

        modelBuilder
            .Entity<Domain.Entities.AlertThreshold>()
            .HasData(
                new Domain.Entities.AlertThreshold
                {
                    Id = 1,
                    MaxWaterCutPercent = 80m,
                    MinOilProductionRate = 500m,
                    MinInjectionRate = 1000m,
                    MaxInjectionPressure = 2500m,
                    ProductionDeclinePercent = 20m,
                    CreatedAt = SeedCreatedAt,
                    CreatedBy = "System",
                }
            );

        SeedMeasurementHistory(modelBuilder, records);
    }

    private static void SeedMeasurementHistory(
        ModelBuilder modelBuilder,
        List<WaterfloodSeedRecord> records
    )
    {
        var history = new List<Domain.Entities.WaterfloodMeasurementHistory>();
        var months = new[]
        {
            new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        var factors = new[] { 1.30m, 1.18m, 1.08m };
        var extraOilFactors = new[] { 1.55m, 1.40m, 1.28m };
        var extraDeclineWells = new HashSet<string> { "PROD-202", "PROD-204", "PROD-206" };
        var waterCutFactors = new[] { 0.72m, 0.82m, 0.92m };
        var pressureFactors = new[] { 0.90m, 0.94m, 0.97m };

        for (var monthIndex = 0; monthIndex < months.Length; monthIndex++)
        {
            foreach (var record in records)
            {
                var wellId = Guid.Parse(record.Id);
                var suffix = record.Id[(record.Id.LastIndexOf('-') + 1)..];
                var historyId = Guid.Parse($"c{monthIndex + 1}000001-0000-0000-0000-{suffix}");

                var oilFactor = extraDeclineWells.Contains(record.WellName)
                    ? extraOilFactors[monthIndex]
                    : factors[monthIndex];

                history.Add(
                    new Domain.Entities.WaterfloodMeasurementHistory
                    {
                        Id = historyId,
                        WellRecordId = wellId,
                        WellName = record.WellName,
                        WellTypeCode = record.WellTypeCode,
                        FieldName = record.FieldName,
                        InjectionRate = Scale(record.InjectionRate, factors[monthIndex]),
                        OilProductionRate = Scale(record.OilProductionRate, oilFactor),
                        WaterProductionRate = Scale(
                            record.WaterProductionRate,
                            waterCutFactors[monthIndex]
                        ),
                        WaterCut = ScalePercent(record.WaterCut, waterCutFactors[monthIndex]),
                        InjectionPressure = Scale(record.InjectionPressure, pressureFactors[monthIndex]),
                        WellStatusCode = record.WellStatusCode,
                        MeasurementDate = months[monthIndex],
                        CreatedAt = months[monthIndex],
                        CreatedBy = "System",
                    }
                );
            }
        }

        modelBuilder.Entity<Domain.Entities.WaterfloodMeasurementHistory>().HasData(history);
    }

    private static decimal? Scale(decimal? value, decimal factor) =>
        value.HasValue ? Math.Round(value.Value * factor, 2) : null;

    private static decimal? ScalePercent(decimal? value, decimal factor) =>
        value.HasValue ? Math.Min(99.9m, Math.Round(value.Value * factor, 2)) : null;

    private static List<T> SeedData<T>(string path)
    {
        using var reader = new StreamReader(path);
        var json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
    }

    private class WaterfloodSeedRecord
    {
        public string Id { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public string WellTypeCode { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? InjectionRate { get; set; }
        public decimal? OilProductionRate { get; set; }
        public decimal? WaterProductionRate { get; set; }
        public decimal? WaterCut { get; set; }
        public decimal? InjectionPressure { get; set; }
        public string WellStatusCode { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; }
    }

    private class RelationshipSeedRecord
    {
        public string Id { get; set; } = string.Empty;
        public string InjectorWellId { get; set; } = string.Empty;
        public string ProducerWellId { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string RelationshipStatusCode { get; set; } = string.Empty;
    }
}
