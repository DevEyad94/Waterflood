using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace project.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoryDeclineAndUserFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProductionDeclinePercent",
                table: "AlertThresholds",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "WaterfloodMeasurementHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WellRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    WellName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WellTypeCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InjectionRate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    OilProductionRate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    WaterProductionRate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    WaterCut = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    InjectionPressure = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    WellStatusCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterfloodMeasurementHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterfloodMeasurementHistories_WaterfloodRecords_WellRecord~",
                        column: x => x.WellRecordId,
                        principalTable: "WaterfloodRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AlertThresholds",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProductionDeclinePercent",
                value: 20m);

            migrationBuilder.InsertData(
                table: "WaterfloodMeasurementHistories",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "FieldName", "InjectionPressure", "InjectionRate", "MeasurementDate", "OilProductionRate", "UpdatedAt", "UpdatedBy", "WaterCut", "WaterProductionRate", "WellName", "WellRecordId", "WellStatusCode", "WellTypeCode" },
                values: new object[,]
                {
                    { new Guid("c1000001-0000-0000-0000-000000000001"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", 1890.00m, 5850.00m, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-101", new Guid("a1000001-0000-0000-0000-000000000001"), "ACT", "INJ" },
                    { new Guid("c1000001-0000-0000-0000-000000000002"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", null, null, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1560.00m, null, null, 28.80m, 576.00m, "PROD-201", new Guid("a1000001-0000-0000-0000-000000000002"), "ACT", "PROD" },
                    { new Guid("c1000001-0000-0000-0000-000000000003"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", null, null, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1007.50m, null, null, 53.28m, 1332.00m, "PROD-202", new Guid("a1000001-0000-0000-0000-000000000003"), "ACT", "PROD" },
                    { new Guid("c1000001-0000-0000-0000-000000000004"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", 1755.00m, 4940.00m, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-102", new Guid("a1000001-0000-0000-0000-000000000004"), "ACT", "INJ" },
                    { new Guid("c1000001-0000-0000-0000-000000000005"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", null, null, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1170.00m, null, null, 39.60m, 792.00m, "PROD-203", new Guid("a1000001-0000-0000-0000-000000000005"), "ACT", "PROD" },
                    { new Guid("c1000001-0000-0000-0000-000000000006"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", null, null, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 620.00m, null, null, 60.48m, 1512.00m, "PROD-204", new Guid("a1000001-0000-0000-0000-000000000006"), "ACT", "PROD" },
                    { new Guid("c1000001-0000-0000-0000-000000000007"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "South Field", 2160.00m, 3250.00m, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-103", new Guid("a1000001-0000-0000-0000-000000000007"), "MNT", "INJ" },
                    { new Guid("c1000001-0000-0000-0000-000000000008"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "South Field", null, null, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1950.00m, null, null, 18.00m, 360.00m, "PROD-205", new Guid("a1000001-0000-0000-0000-000000000008"), "ACT", "PROD" },
                    { new Guid("c1000001-0000-0000-0000-000000000009"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "West Field", 2385.00m, 2340.00m, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-104", new Guid("a1000001-0000-0000-0000-000000000009"), "ACT", "INJ" },
                    { new Guid("c1000001-0000-0000-0000-00000000000a"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "West Field", null, null, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 465.00m, null, null, 64.80m, 1944.00m, "PROD-206", new Guid("a1000001-0000-0000-0000-00000000000a"), "SHT", "PROD" },
                    { new Guid("c2000001-0000-0000-0000-000000000001"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", 1974.00m, 5310.00m, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-101", new Guid("a1000001-0000-0000-0000-000000000001"), "ACT", "INJ" },
                    { new Guid("c2000001-0000-0000-0000-000000000002"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", null, null, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1416.00m, null, null, 32.80m, 656.00m, "PROD-201", new Guid("a1000001-0000-0000-0000-000000000002"), "ACT", "PROD" },
                    { new Guid("c2000001-0000-0000-0000-000000000003"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", null, null, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 910.00m, null, null, 60.68m, 1517.00m, "PROD-202", new Guid("a1000001-0000-0000-0000-000000000003"), "ACT", "PROD" },
                    { new Guid("c2000001-0000-0000-0000-000000000004"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", 1833.00m, 4484.00m, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-102", new Guid("a1000001-0000-0000-0000-000000000004"), "ACT", "INJ" },
                    { new Guid("c2000001-0000-0000-0000-000000000005"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", null, null, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1062.00m, null, null, 45.10m, 902.00m, "PROD-203", new Guid("a1000001-0000-0000-0000-000000000005"), "ACT", "PROD" },
                    { new Guid("c2000001-0000-0000-0000-000000000006"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", null, null, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 560.00m, null, null, 68.88m, 1722.00m, "PROD-204", new Guid("a1000001-0000-0000-0000-000000000006"), "ACT", "PROD" },
                    { new Guid("c2000001-0000-0000-0000-000000000007"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "South Field", 2256.00m, 2950.00m, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-103", new Guid("a1000001-0000-0000-0000-000000000007"), "MNT", "INJ" },
                    { new Guid("c2000001-0000-0000-0000-000000000008"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "South Field", null, null, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1770.00m, null, null, 20.50m, 410.00m, "PROD-205", new Guid("a1000001-0000-0000-0000-000000000008"), "ACT", "PROD" },
                    { new Guid("c2000001-0000-0000-0000-000000000009"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "West Field", 2491.00m, 2124.00m, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-104", new Guid("a1000001-0000-0000-0000-000000000009"), "ACT", "INJ" },
                    { new Guid("c2000001-0000-0000-0000-00000000000a"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "West Field", null, null, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 420.00m, null, null, 73.80m, 2214.00m, "PROD-206", new Guid("a1000001-0000-0000-0000-00000000000a"), "SHT", "PROD" },
                    { new Guid("c3000001-0000-0000-0000-000000000001"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", 2037.00m, 4860.00m, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-101", new Guid("a1000001-0000-0000-0000-000000000001"), "ACT", "INJ" },
                    { new Guid("c3000001-0000-0000-0000-000000000002"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", null, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1296.00m, null, null, 36.80m, 736.00m, "PROD-201", new Guid("a1000001-0000-0000-0000-000000000002"), "ACT", "PROD" },
                    { new Guid("c3000001-0000-0000-0000-000000000003"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", null, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), 832.00m, null, null, 68.08m, 1702.00m, "PROD-202", new Guid("a1000001-0000-0000-0000-000000000003"), "ACT", "PROD" },
                    { new Guid("c3000001-0000-0000-0000-000000000004"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", 1891.50m, 4104.00m, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-102", new Guid("a1000001-0000-0000-0000-000000000004"), "ACT", "INJ" },
                    { new Guid("c3000001-0000-0000-0000-000000000005"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", null, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), 972.00m, null, null, 50.60m, 1012.00m, "PROD-203", new Guid("a1000001-0000-0000-0000-000000000005"), "ACT", "PROD" },
                    { new Guid("c3000001-0000-0000-0000-000000000006"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", null, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), 512.00m, null, null, 77.28m, 1932.00m, "PROD-204", new Guid("a1000001-0000-0000-0000-000000000006"), "ACT", "PROD" },
                    { new Guid("c3000001-0000-0000-0000-000000000007"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "South Field", 2328.00m, 2700.00m, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-103", new Guid("a1000001-0000-0000-0000-000000000007"), "MNT", "INJ" },
                    { new Guid("c3000001-0000-0000-0000-000000000008"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "South Field", null, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1620.00m, null, null, 23.00m, 460.00m, "PROD-205", new Guid("a1000001-0000-0000-0000-000000000008"), "ACT", "PROD" },
                    { new Guid("c3000001-0000-0000-0000-000000000009"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "West Field", 2570.50m, 1944.00m, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-104", new Guid("a1000001-0000-0000-0000-000000000009"), "ACT", "INJ" },
                    { new Guid("c3000001-0000-0000-0000-00000000000a"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "West Field", null, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), 384.00m, null, null, 82.80m, 2484.00m, "PROD-206", new Guid("a1000001-0000-0000-0000-00000000000a"), "SHT", "PROD" }
                });

            migrationBuilder.InsertData(
                table: "ZSK_Ref_MonitoringRules",
                columns: new[] { "RuleCode", "DefaultThresholdValue", "Description", "Name", "Severity", "TargetWellType" },
                values: new object[] { "RULE_PRODUCTION_DECLINE", 20.0m, "Significant decrease in oil production compared with previous measurements", "Production Decline", "Warning", "PROD" });

            migrationBuilder.CreateIndex(
                name: "IX_WaterfloodMeasurementHistories_MeasurementDate",
                table: "WaterfloodMeasurementHistories",
                column: "MeasurementDate");

            migrationBuilder.CreateIndex(
                name: "IX_WaterfloodMeasurementHistories_WellRecordId",
                table: "WaterfloodMeasurementHistories",
                column: "WellRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WaterfloodMeasurementHistories");

            migrationBuilder.DeleteData(
                table: "ZSK_Ref_MonitoringRules",
                keyColumn: "RuleCode",
                keyValue: "RULE_PRODUCTION_DECLINE");

            migrationBuilder.DropColumn(
                name: "ProductionDeclinePercent",
                table: "AlertThresholds");
        }
    }
}
