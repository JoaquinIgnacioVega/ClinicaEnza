using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turnos");
        builder.HasKey(e => e.IdTurno);

        builder.Property(e => e.HoraInicio).IsRequired();
        builder.Property(e => e.HoraFin).IsRequired();
        builder.Property(e => e.Estado).IsRequired();

        builder.HasIndex(e => new { e.IdMedico, e.HoraInicio });
        builder.HasIndex(e => new { e.IdConsultorio, e.HoraInicio });

        builder.HasOne(e => e.Paciente)
               .WithMany(p => p.Turnos)
               .HasForeignKey(e => e.IdPaciente)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Medico)
               .WithMany(m => m.Turnos)
               .HasForeignKey(e => e.IdMedico)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Consultorio)
               .WithMany(c => c.Turnos)
               .HasForeignKey(e => e.IdConsultorio)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

