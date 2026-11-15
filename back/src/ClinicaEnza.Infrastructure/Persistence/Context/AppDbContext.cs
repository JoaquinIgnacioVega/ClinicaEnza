using ClinicaEnza.Domain;
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
        if (_sqliteSchemaChecked) return;

        lock (_schemaLock)
        {
            if (_sqliteSchemaChecked) return;
            try
            {
                if (Database.IsSqlite())
                {
                    Database.ExecuteSqlRaw("ALTER TABLE Consultorios ADD COLUMN Activo INTEGER NOT NULL DEFAULT 1;");
                }
            }
            catch
            {
                // La columna Activo ya existe o la tabla aún no fue inicializada
            }
            finally
            {
                _sqliteSchemaChecked = true;
            }
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

