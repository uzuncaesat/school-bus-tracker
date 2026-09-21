using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkyanusServis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddApproachNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArventoNode",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnApproach",
                table: "Registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApproachAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Leg = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistanceMeters = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproachAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApproachAlerts_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArventoUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArventoPin1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArventoPin2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArventoBaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseMock = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    ApproachThresholdMeters = table.Column<int>(type: "int", nullable: false),
                    PollIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    MorningStart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MorningEnd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EveningStart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EveningEnd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApproachAlerts_RegistrationId",
                table: "ApproachAlerts",
                column: "RegistrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApproachAlerts");

            migrationBuilder.DropTable(
                name: "ServiceConfigs");

            migrationBuilder.DropColumn(
                name: "ArventoNode",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NotifyOnApproach",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");
        }
    }
}
