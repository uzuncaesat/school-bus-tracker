using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkyanusServis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSplitPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DownPayment",
                table: "Registrations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DownPaymentBank",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DownPaymentMethod",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InstallmentAmount",
                table: "Registrations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstallmentBank",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownPayment",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DownPaymentBank",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DownPaymentMethod",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "InstallmentAmount",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "InstallmentBank",
                table: "Registrations");
        }
    }
}
