using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkyanusServis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTcFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentTc",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentTc",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentTc",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "StudentTc",
                table: "Registrations");
        }
    }
}
