using ClinicaEnza.Domain;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace ClinicaEnza.Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    private static bool _sqliteSchemaChecked;
    private static readonly object _schemaLock = new();

    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        EnsureSqliteSchemaCompatibility();
    }

    private void EnsureSqliteSchemaCompatibility()
    {
        if (_sqliteSchemaChecked || !Database.IsSqlite()) return;

        lock (_schemaLock)
        {
            if (_sqliteSchemaChecked) return;

            try
            {
                var connection = Database.GetDbConnection();
                var closeConnection = connection.State != ConnectionState.Open;

                if (closeConnection)
                {
                    connection.Open();
                }

                EnsureColumn(connection, "Consultorios", "Activo", "ALTER TABLE Consultorios ADD COLUMN Activo INTEGER NOT NULL DEFAULT 1;");
                EnsureColumn(connection, "Personas", "PasswordHash", "ALTER TABLE Personas ADD COLUMN PasswordHash TEXT NULL;");
                EnsureColumn(connection, "Personas", "DebeCambiarPassword", "ALTER TABLE Personas ADD COLUMN DebeCambiarPassword INTEGER NOT NULL DEFAULT 1;");

                if (closeConnection)
                {
                    connection.Close();
                }
            }
            catch
            {
                // La tabla puede no existir todavía; las migraciones la crearán.
            }
            finally
            {
                _sqliteSchemaChecked = true;
            }
        }
    }

    private static void EnsureColumn(DbConnection connection, string tableName, string columnName, string alterSql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName}');";
        var exists = false;
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
        {
            command.CommandText = alterSql;
            command.ExecuteNonQuery();
        }
    }

    public DbSet<Persona> Personas { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Medico> Medicos { get; set; } = null!;
    public DbSet<Paciente> Pacientes { get; set; } = null!;
    public DbSet<HistoriaClinica> HistoriasClinicas { get; set; } = null!;
    public DbSet<PrescripcionMedica> PrescripcionesMedicas { get; set; } = null!;
    public DbSet<Consultorio> Consultorios { get; set; } = null!;
    public DbSet<Turno> Turnos { get; set; } = null!;
    public DbSet<HorarioAtencion> HorariosAtencion { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=clinica_enza.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

