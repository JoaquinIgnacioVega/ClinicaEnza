using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaEnza.Infrastructure.Persistence.Migrations;

public partial class AgregarActivoConsultorio : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "Activo",
            table: "Consultorios",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Activo", table: "Consultorios");
    }
}
