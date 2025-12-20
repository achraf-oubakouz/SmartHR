using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHR.Migrations
{
    /// <inheritdoc />
    public partial class yes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rapports_Managers_ManagerId",
                table: "Rapports");

            migrationBuilder.DropColumn(
                name: "FichierPath",
                table: "Rapports");

            migrationBuilder.AddForeignKey(
                name: "FK_Rapports_Managers_ManagerId",
                table: "Rapports",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rapports_Managers_ManagerId",
                table: "Rapports");

            migrationBuilder.AddColumn<string>(
                name: "FichierPath",
                table: "Rapports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Rapports_Managers_ManagerId",
                table: "Rapports",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
