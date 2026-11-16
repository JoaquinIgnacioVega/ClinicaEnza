using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaEnza.Infrastructure.Persistence.Migrations;

public partial class AgregarPasswordPaciente : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PasswordHash",
            table: "Personas",
            type: "TEXT",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "DebeCambiarPassword",
            table: "Personas",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PasswordHash", table: "Personas");
        migrationBuilder.DropColumn(name: "DebeCambiarPassword", table: "Personas");
    }
}
