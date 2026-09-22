using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaEnza.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialActualizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Horario",
                table: "Turnos",
                newName: "HoraInicio");

            migrationBuilder.RenameIndex(
                name: "IX_Turnos_IdMedico_Horario",
                table: "Turnos",
                newName: "IX_Turnos_IdMedico_HoraInicio");

            migrationBuilder.RenameIndex(
                name: "IX_Turnos_IdConsultorio_Horario",
                table: "Turnos",
                newName: "IX_Turnos_IdConsultorio_HoraInicio");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Turnos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraFin",
                table: "Turnos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha",
                table: "PrescripcionesMedicas",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<int>(
                name: "Rol",
                table: "Personas",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Especialidad",
                table: "Personas",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdMedico",
                table: "Personas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_IdMedico",
                table: "Personas",
                column: "IdMedico",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_Personas_IdMedico",
                table: "Personas",
                column: "IdMedico",
                principalTable: "Personas",
                principalColumn: "IdPersona",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personas_Personas_IdMedico",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_Personas_IdMedico",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "HoraFin",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "PrescripcionesMedicas");

            migrationBuilder.DropColumn(
                name: "IdMedico",
                table: "Personas");

            migrationBuilder.RenameColumn(
                name: "HoraInicio",
                table: "Turnos",
                newName: "Horario");

            migrationBuilder.RenameIndex(
                name: "IX_Turnos_IdMedico_HoraInicio",
                table: "Turnos",
                newName: "IX_Turnos_IdMedico_Horario");

            migrationBuilder.RenameIndex(
                name: "IX_Turnos_IdConsultorio_HoraInicio",
                table: "Turnos",
                newName: "IX_Turnos_IdConsultorio_Horario");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Turnos",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Personas",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Especialidad",
                table: "Personas",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
