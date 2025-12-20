using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHR.Migrations
{
    /// <inheritdoc />
    public partial class AddJoursCongesTotalToEmploye : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JoursCongesTotal",
                table: "Employes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Calendriers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoursCongesTotal",
                table: "Employes");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Calendriers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
