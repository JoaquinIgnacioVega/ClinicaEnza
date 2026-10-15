using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class ConsultorioConfiguration : IEntityTypeConfiguration<Consultorio>
{
    public void Configure(EntityTypeBuilder<Consultorio> builder)
    {
        builder.ToTable("Consultorios");
        builder.HasKey(e => e.IdConsultorio);

        builder.Property(e => e.Numero).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Sector).HasMaxLength(50);
    }
}

