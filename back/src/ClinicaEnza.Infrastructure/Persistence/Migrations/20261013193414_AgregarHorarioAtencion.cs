using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaEnza.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHorarioAtencion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HorariosAtencion",
                columns: table => new
                {
                    IdHorarioAtencion = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiaSemana = table.Column<int>(type: "INTEGER", nullable: false),
                    HoraDesde = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    HoraHasta = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    IdMedico = table.Column<int>(type: "INTEGER", nullable: false),
                    IdConsultorio = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosAtencion", x => x.IdHorarioAtencion);
                    table.ForeignKey(
                        name: "FK_HorariosAtencion_Consultorios_IdConsultorio",
                        column: x => x.IdConsultorio,
                        principalTable: "Consultorios",
                        principalColumn: "IdConsultorio",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HorariosAtencion_Personas_IdMedico",
                        column: x => x.IdMedico,
                        principalTable: "Personas",
                        principalColumn: "IdPersona",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosAtencion_IdConsultorio",
                table: "HorariosAtencion",
                column: "IdConsultorio");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosAtencion_IdMedico",
                table: "HorariosAtencion",
                column: "IdMedico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorariosAtencion");
        }
    }
}
