using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class HistoriaClinicaConfiguration : IEntityTypeConfiguration<HistoriaClinica>
{
    public void Configure(EntityTypeBuilder<HistoriaClinica> builder)
    {
        builder.ToTable("HistoriasClinicas");
        builder.HasKey(e => e.IdHistoriaClinica);

        builder.HasOne(e => e.Paciente)
               .WithOne(p => p.HistoriaClinica)
               .HasForeignKey<HistoriaClinica>(e => e.IdPaciente)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

