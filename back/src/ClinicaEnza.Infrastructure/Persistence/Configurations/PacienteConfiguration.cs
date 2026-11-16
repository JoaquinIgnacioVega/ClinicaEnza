using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.Property(e => e.ObraSocial).HasMaxLength(100);
        builder.Property(e => e.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(500).IsRequired(false);
        builder.Property(e => e.DebeCambiarPassword).IsRequired().HasDefaultValue(true);
    }
}

