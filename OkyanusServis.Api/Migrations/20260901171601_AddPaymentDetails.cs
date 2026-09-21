using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkyanusServis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Registrations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Installments",
                table: "Registrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Registrations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "Installments",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Registrations");
        }
    }
}
