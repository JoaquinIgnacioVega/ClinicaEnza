using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class PrescripcionMedicaConfiguration : IEntityTypeConfiguration<PrescripcionMedica>
{
    public void Configure(EntityTypeBuilder<PrescripcionMedica> builder)
    {
        builder.ToTable("PrescripcionesMedicas");
        builder.HasKey(e => e.IdPrescripcion);

        builder.Property(e => e.Medicamento).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(e => e.HistoriaClinica)
               .WithMany(h => h.Prescripciones)
               .HasForeignKey(e => e.IdHistoriaClinica)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Medico)
               .WithMany(m => m.Prescripciones)
               .HasForeignKey(e => e.IdMedico)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

