using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaEnza.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPasswordHashAUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Personas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Legajo",
                table: "Personas",
                column: "Legajo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Personas_Legajo",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Personas");
        }
    }
}
