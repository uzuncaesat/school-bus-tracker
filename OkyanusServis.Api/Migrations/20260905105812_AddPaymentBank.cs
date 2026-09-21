using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkyanusServis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bank",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bank",
                table: "Registrations");
        }
    }
}
