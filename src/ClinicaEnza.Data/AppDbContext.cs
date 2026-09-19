using Microsoft.EntityFrameworkCore;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Persona> Personas { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Medico> Medicos { get; set; } = null!;
    public DbSet<Paciente> Pacientes { get; set; } = null!;
    public DbSet<HistoriaClinica> HistoriasClinicas { get; set; } = null!;
    public DbSet<PrescripcionMedica> PrescripcionesMedicas { get; set; } = null!;
    public DbSet<Consultorio> Consultorios { get; set; } = null!;
    public DbSet<Turno> Turnos { get; set; } = null!;

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

        // Persona

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("Personas");
            entity.HasKey(e => e.IdPersona);

            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Dni).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Mail).HasMaxLength(150);

            entity.HasIndex(e => e.Dni).IsUnique();
        });

        // Usuario

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");

            entity.Property(e => e.Legajo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Rol).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Medico

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.ToTable("Medicos");

            entity.Property(e => e.Matricula).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Especialidad).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => e.Matricula).IsUnique();
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.ToTable("Pacientes");

            entity.Property(e => e.ObraSocial).HasMaxLength(100);
        });

        // Historia Clinica

        modelBuilder.Entity<HistoriaClinica>(entity =>
        {
            entity.ToTable("HistoriasClinicas");
            entity.HasKey(e => e.IdHistoriaClinica);

            entity.HasOne(e => e.Paciente)
                  .WithOne(p => p.HistoriaClinica)
                  .HasForeignKey<HistoriaClinica>(e => e.IdPaciente)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Prescripcion Medica

        modelBuilder.Entity<PrescripcionMedica>(entity =>
        {
            entity.ToTable("PrescripcionesMedicas");
            entity.HasKey(e => e.IdPrescripcion);

            entity.Property(e => e.Medicamento).IsRequired().HasMaxLength(255);

            entity.HasOne(e => e.HistoriaClinica)
                  .WithMany(h => h.Prescripciones)
                  .HasForeignKey(e => e.IdHistoriaClinica)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Medico)
                  .WithMany(m => m.Prescripciones)
                  .HasForeignKey(e => e.IdMedico)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Consultorio

        modelBuilder.Entity<Consultorio>(entity =>
        {
            entity.ToTable("Consultorios");
            entity.HasKey(e => e.IdConsultorio);

            entity.Property(e => e.Numero).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Sector).HasMaxLength(50);
        });

        // Turnos

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.ToTable("Turnos");
            entity.HasKey(e => e.IdTurno);

            entity.Property(e => e.Horario).IsRequired();

            // Índices para optimizar la búsqueda de solapamientos
            entity.HasIndex(e => new { e.IdMedico, e.Horario });
            entity.HasIndex(e => new { e.IdConsultorio, e.Horario });

            entity.HasOne(e => e.Paciente)
                  .WithMany(p => p.Turnos)
                  .HasForeignKey(e => e.IdPaciente)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Medico)
                  .WithMany(m => m.Turnos)
                  .HasForeignKey(e => e.IdMedico)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Consultorio)
                  .WithMany(c => c.Turnos)
                  .HasForeignKey(e => e.IdConsultorio)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
