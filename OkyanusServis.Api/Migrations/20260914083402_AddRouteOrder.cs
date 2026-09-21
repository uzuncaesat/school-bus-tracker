using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkyanusServis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RouteOrder",
                table: "Registrations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteOrder",
                table: "Registrations");
        }
    }
}
