using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class MedicoConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.Property(e => e.Matricula).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Especialidad).IsRequired();

        builder.HasIndex(e => e.Matricula).IsUnique();
    }
}

