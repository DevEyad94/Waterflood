using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace project.Migrations
{
    /// <inheritdoc />
    public partial class WaterfloodInitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertThresholds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaxWaterCutPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MinOilProductionRate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MinInjectionRate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MaxInjectionPressure = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertThresholds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "zRoles",
                columns: table => new
                {
                    zRoleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zRoles", x => x.zRoleID);
                });

            migrationBuilder.CreateTable(
                name: "ZSK_Ref_MonitoringRules",
                columns: table => new
                {
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetWellType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DefaultThresholdValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZSK_Ref_MonitoringRules", x => x.RuleCode);
                });

            migrationBuilder.CreateTable(
                name: "ZSK_Ref_RelationshipStatus",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZSK_Ref_RelationshipStatus", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ZSK_Ref_WellStatus",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZSK_Ref_WellStatus", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ZSK_Ref_WellType",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZSK_Ref_WellType", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    zRoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleID);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_zRoles_zRoleId",
                        column: x => x.zRoleId,
                        principalTable: "zRoles",
                        principalColumn: "zRoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WaterfloodRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WellName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WellTypeCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_WaterfloodRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterfloodRecords_ZSK_Ref_WellStatus_WellStatusCode",
                        column: x => x.WellStatusCode,
                        principalTable: "ZSK_Ref_WellStatus",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaterfloodRecords_ZSK_Ref_WellType_WellTypeCode",
                        column: x => x.WellTypeCode,
                        principalTable: "ZSK_Ref_WellType",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InjectorProducerRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InjectorWellId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerWellId = table.Column<Guid>(type: "uuid", nullable: false),
                    Distance = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    RelationshipStatusCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InjectorProducerRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InjectorProducerRelationships_WaterfloodRecords_InjectorWel~",
                        column: x => x.InjectorWellId,
                        principalTable: "WaterfloodRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InjectorProducerRelationships_WaterfloodRecords_ProducerWel~",
                        column: x => x.ProducerWellId,
                        principalTable: "WaterfloodRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InjectorProducerRelationships_ZSK_Ref_RelationshipStatus_Re~",
                        column: x => x.RelationshipStatusCode,
                        principalTable: "ZSK_Ref_RelationshipStatus",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AlertThresholds",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "MaxInjectionPressure", "MaxWaterCutPercent", "MinInjectionRate", "MinOilProductionRate", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", 2500m, 80m, 1000m, 500m, null, null });

            migrationBuilder.InsertData(
                table: "ZSK_Ref_MonitoringRules",
                columns: new[] { "RuleCode", "DefaultThresholdValue", "Description", "Name", "Severity", "TargetWellType" },
                values: new object[,]
                {
                    { "RULE_COMBINED_DEFICIT", 0.0m, "High water cut combined with low oil production", "Combined Production Deficit", "Critical", "PROD" },
                    { "RULE_HIGH_PRESSURE", 2500.0m, "Injection pressure exceeds maximum allowed", "High Injection Pressure", "Critical", "INJ" },
                    { "RULE_HIGH_WATER_CUT", 80.0m, "Producer water cut exceeds defined threshold", "High Water Cut", "Warning", "PROD" },
                    { "RULE_INACTIVE_WELL", 0.0m, "Well is shut-in or under maintenance", "Inactive Well", "Notice", "ANY" },
                    { "RULE_LOW_INJECTION", 1000.0m, "Injector falls below target injection rate", "Low Injection Rate", "Warning", "INJ" },
                    { "RULE_LOW_OIL_PROD", 500.0m, "Oil production falls below defined threshold", "Low Oil Production", "Warning", "PROD" }
                });

            migrationBuilder.InsertData(
                table: "ZSK_Ref_RelationshipStatus",
                columns: new[] { "Code", "Description", "Name" },
                values: new object[,]
                {
                    { "ACT", "Injector-producer relationship is active", "Active" },
                    { "INA", "Injector-producer relationship is inactive", "Inactive" }
                });

            migrationBuilder.InsertData(
                table: "ZSK_Ref_WellStatus",
                columns: new[] { "Code", "ColorCode", "Description", "Name" },
                values: new object[,]
                {
                    { "ACT", "#28a745", "Well is actively operating", "Active" },
                    { "MNT", "#ffc107", "Well is under maintenance", "Maintenance" },
                    { "SHT", "#dc3545", "Well is temporarily shut in", "Shut-in" }
                });

            migrationBuilder.InsertData(
                table: "ZSK_Ref_WellType",
                columns: new[] { "Code", "Description", "Name" },
                values: new object[,]
                {
                    { "INJ", "Water injection well", "Injector" },
                    { "PROD", "Fluid production well", "Producer" }
                });

            migrationBuilder.InsertData(
                table: "zRoles",
                columns: new[] { "zRoleID", "Name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Operator" },
                    { 3, "PetroleumEngineer" }
                });

            migrationBuilder.InsertData(
                table: "WaterfloodRecords",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "FieldName", "InjectionPressure", "InjectionRate", "Latitude", "Longitude", "MeasurementDate", "OilProductionRate", "UpdatedAt", "UpdatedBy", "WaterCut", "WaterProductionRate", "WellName", "WellStatusCode", "WellTypeCode" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-0000-0000-000000000001"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", 2100m, 4500m, 22.12346m, 56.123456m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-101", "ACT", "INJ" },
                    { new Guid("a1000001-0000-0000-0000-000000000002"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", null, null, 22.12510m, 56.126200m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1200m, null, null, 40m, 800m, "PROD-201", "ACT", "PROD" },
                    { new Guid("a1000001-0000-0000-0000-000000000003"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "North Field", null, null, 22.12820m, 56.129300m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), 650m, null, null, 74m, 1850m, "PROD-202", "ACT", "PROD" },
                    { new Guid("a1000001-0000-0000-0000-000000000004"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", 1950m, 3800m, 21.98765m, 55.987654m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-102", "ACT", "INJ" },
                    { new Guid("a1000001-0000-0000-0000-000000000005"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", null, null, 21.99010m, 55.990200m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), 900m, null, null, 55m, 1100m, "PROD-203", "ACT", "PROD" },
                    { new Guid("a1000001-0000-0000-0000-000000000006"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Central Field", null, null, 21.99320m, 55.994100m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), 400m, null, null, 84m, 2100m, "PROD-204", "ACT", "PROD" },
                    { new Guid("a1000001-0000-0000-0000-000000000007"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "South Field", 2400m, 2500m, 20.65432m, 54.654321m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-103", "MNT", "INJ" },
                    { new Guid("a1000001-0000-0000-0000-000000000008"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "South Field", null, null, 20.65710m, 54.658200m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1500m, null, null, 25m, 500m, "PROD-205", "ACT", "PROD" },
                    { new Guid("a1000001-0000-0000-0000-000000000009"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "West Field", 2650m, 1800m, 23.45679m, 57.123456m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "INJ-104", "ACT", "INJ" },
                    { new Guid("a1000001-0000-0000-0000-00000000000a"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "West Field", null, null, 23.45920m, 57.126100m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), 300m, null, null, 90m, 2700m, "PROD-206", "SHT", "PROD" }
                });

            migrationBuilder.InsertData(
                table: "InjectorProducerRelationships",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Distance", "InjectorWellId", "ProducerWellId", "RelationshipStatusCode", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("b2000001-0000-0000-0000-000000000001"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0.4m, new Guid("a1000001-0000-0000-0000-000000000001"), new Guid("a1000001-0000-0000-0000-000000000002"), "ACT", null, null },
                    { new Guid("b2000001-0000-0000-0000-000000000002"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0.8m, new Guid("a1000001-0000-0000-0000-000000000001"), new Guid("a1000001-0000-0000-0000-000000000003"), "ACT", null, null },
                    { new Guid("b2000001-0000-0000-0000-000000000003"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0.5m, new Guid("a1000001-0000-0000-0000-000000000004"), new Guid("a1000001-0000-0000-0000-000000000005"), "ACT", null, null },
                    { new Guid("b2000001-0000-0000-0000-000000000004"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0.9m, new Guid("a1000001-0000-0000-0000-000000000004"), new Guid("a1000001-0000-0000-0000-000000000006"), "ACT", null, null },
                    { new Guid("b2000001-0000-0000-0000-000000000005"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0.6m, new Guid("a1000001-0000-0000-0000-000000000007"), new Guid("a1000001-0000-0000-0000-000000000008"), "ACT", null, null },
                    { new Guid("b2000001-0000-0000-0000-000000000006"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0.7m, new Guid("a1000001-0000-0000-0000-000000000009"), new Guid("a1000001-0000-0000-0000-00000000000a"), "ACT", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InjectorProducerRelationships_InjectorWellId",
                table: "InjectorProducerRelationships",
                column: "InjectorWellId");

            migrationBuilder.CreateIndex(
                name: "IX_InjectorProducerRelationships_ProducerWellId",
                table: "InjectorProducerRelationships",
                column: "ProducerWellId");

            migrationBuilder.CreateIndex(
                name: "IX_InjectorProducerRelationships_RelationshipStatusCode",
                table: "InjectorProducerRelationships",
                column: "RelationshipStatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserID",
                table: "UserRoles",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_zRoleId",
                table: "UserRoles",
                column: "zRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WaterfloodRecords_FieldName",
                table: "WaterfloodRecords",
                column: "FieldName");

            migrationBuilder.CreateIndex(
                name: "IX_WaterfloodRecords_MeasurementDate",
                table: "WaterfloodRecords",
                column: "MeasurementDate");

            migrationBuilder.CreateIndex(
                name: "IX_WaterfloodRecords_WellName",
                table: "WaterfloodRecords",
                column: "WellName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaterfloodRecords_WellStatusCode",
                table: "WaterfloodRecords",
                column: "WellStatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_WaterfloodRecords_WellTypeCode",
                table: "WaterfloodRecords",
                column: "WellTypeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertThresholds");

            migrationBuilder.DropTable(
                name: "InjectorProducerRelationships");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "ZSK_Ref_MonitoringRules");

            migrationBuilder.DropTable(
                name: "WaterfloodRecords");

            migrationBuilder.DropTable(
                name: "ZSK_Ref_RelationshipStatus");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "zRoles");

            migrationBuilder.DropTable(
                name: "ZSK_Ref_WellStatus");

            migrationBuilder.DropTable(
                name: "ZSK_Ref_WellType");
        }
    }
}
