using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHR.Migrations
{
    /// <inheritdoc />
    public partial class RemovePointage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pointages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pointages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeId = table.Column<int>(type: "int", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstPresent = table.Column<bool>(type: "bit", nullable: false),
                    HeureArrivee = table.Column<TimeSpan>(type: "time", nullable: true),
                    HeureDepart = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pointages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pointages_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_EmployeId",
                table: "Pointages",
                column: "EmployeId");
        }
    }
}
