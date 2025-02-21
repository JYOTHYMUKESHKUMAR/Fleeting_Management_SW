using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreMVCApp.Models.Migrations
{
    /// <inheritdoc />
    public partial class Import : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Driver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QatarId = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QIDExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Driver", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                columns: table => new
                {
                    VehicleId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WoqodLicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GPSCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleBrand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VehicleModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    FuelTankCapacity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FuelLimit = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RegistrationExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.VehicleId);
                });

            migrationBuilder.CreateTable(
                name: "vw_ComprehensiveConsumptionReport",
                columns: table => new
                {
                    ConsumptionDetailsId = table.Column<int>(type: "int", nullable: false),
                    SaleTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DriverQatarId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DriverContact = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OdometerReading = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FuelQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DistanceSinceLastRefill = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DaysSinceLastRefill = table.Column<int>(type: "int", nullable: false),
                    KmPerLiter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VehicleTotalRefills = table.Column<int>(type: "int", nullable: false),
                    VehicleTotalFuel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VehicleTotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VehicleTotalDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DriverTotalRefills = table.Column<int>(type: "int", nullable: false),
                    DriverTotalFuel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DriverTotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DriverVehicleCount = table.Column<int>(type: "int", nullable: false),
                    VehicleFuelLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FuelLimitStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MonthlyTotalFuel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyTotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsumptionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WoqodLicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OfflineLimit = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FuelQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SaleTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceMonth = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    StationName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FleetName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CostCenter = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OdometerReading = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumptionDetails_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FuelLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WoqodLicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FuelType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OfflineLimit = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FuelQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CostPerLiter = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FuelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceMonth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StationName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FleetName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CostCenter = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OdometerReading = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelLog_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionDetails_DriverId",
                table: "ConsumptionDetails",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_QatarId",
                table: "Driver",
                column: "QatarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelLog_DriverId",
                table: "FuelLog",
                column: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ConsumptionDetails");

            migrationBuilder.DropTable(
                name: "FuelLog");

            migrationBuilder.DropTable(
                name: "Vehicle");

            migrationBuilder.DropTable(
                name: "vw_ComprehensiveConsumptionReport");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Driver");
        }
    }
}
