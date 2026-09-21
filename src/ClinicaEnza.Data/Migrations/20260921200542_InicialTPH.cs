using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaEnza.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialTPH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consultorios",
                columns: table => new
                {
                    IdConsultorio = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Piso = table.Column<int>(type: "INTEGER", nullable: false),
                    Sector = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultorios", x => x.IdConsultorio);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    IdPersona = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Dni = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Mail = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    TipoPersona = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Especialidad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Matricula = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ObraSocial = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Peso = table.Column<decimal>(type: "TEXT", nullable: true),
                    Altura = table.Column<decimal>(type: "TEXT", nullable: true),
                    Legajo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Usuario_FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Rol = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.IdPersona);
                });

            migrationBuilder.CreateTable(
                name: "HistoriasClinicas",
                columns: table => new
                {
                    IdHistoriaClinica = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdPaciente = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriasClinicas", x => x.IdHistoriaClinica);
                    table.ForeignKey(
                        name: "FK_HistoriasClinicas_Personas_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Personas",
                        principalColumn: "IdPersona",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    IdTurno = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Horario = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    IdPaciente = table.Column<int>(type: "INTEGER", nullable: false),
                    IdMedico = table.Column<int>(type: "INTEGER", nullable: false),
                    IdConsultorio = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.IdTurno);
                    table.ForeignKey(
                        name: "FK_Turnos_Consultorios_IdConsultorio",
                        column: x => x.IdConsultorio,
                        principalTable: "Consultorios",
                        principalColumn: "IdConsultorio",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turnos_Personas_IdMedico",
                        column: x => x.IdMedico,
                        principalTable: "Personas",
                        principalColumn: "IdPersona",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turnos_Personas_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Personas",
                        principalColumn: "IdPersona",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrescripcionesMedicas",
                columns: table => new
                {
                    IdPrescripcion = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Medicamento = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IdHistoriaClinica = table.Column<int>(type: "INTEGER", nullable: false),
                    IdMedico = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescripcionesMedicas", x => x.IdPrescripcion);
                    table.ForeignKey(
                        name: "FK_PrescripcionesMedicas_HistoriasClinicas_IdHistoriaClinica",
                        column: x => x.IdHistoriaClinica,
                        principalTable: "HistoriasClinicas",
                        principalColumn: "IdHistoriaClinica",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrescripcionesMedicas_Personas_IdMedico",
                        column: x => x.IdMedico,
                        principalTable: "Personas",
                        principalColumn: "IdPersona",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoriasClinicas_IdPaciente",
                table: "HistoriasClinicas",
                column: "IdPaciente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Dni",
                table: "Personas",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Matricula",
                table: "Personas",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrescripcionesMedicas_IdHistoriaClinica",
                table: "PrescripcionesMedicas",
                column: "IdHistoriaClinica");

            migrationBuilder.CreateIndex(
                name: "IX_PrescripcionesMedicas_IdMedico",
                table: "PrescripcionesMedicas",
                column: "IdMedico");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_IdConsultorio_Horario",
                table: "Turnos",
                columns: new[] { "IdConsultorio", "Horario" });

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_IdMedico_Horario",
                table: "Turnos",
                columns: new[] { "IdMedico", "Horario" });

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_IdPaciente",
                table: "Turnos",
                column: "IdPaciente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrescripcionesMedicas");

            migrationBuilder.DropTable(
                name: "Turnos");

            migrationBuilder.DropTable(
                name: "HistoriasClinicas");

            migrationBuilder.DropTable(
                name: "Consultorios");

            migrationBuilder.DropTable(
                name: "Personas");
        }
    }
}
