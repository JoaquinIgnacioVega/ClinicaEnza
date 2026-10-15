using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class HorarioAtencionConfiguration : IEntityTypeConfiguration<HorarioAtencion>
{
    public void Configure(EntityTypeBuilder<HorarioAtencion> builder)
    {
        builder.ToTable("HorariosAtencion");
        builder.HasKey(e => e.IdHorarioAtencion);

        builder.Property(e => e.DiaSemana).IsRequired();
        builder.Property(e => e.HoraDesde).IsRequired();
        builder.Property(e => e.HoraHasta).IsRequired();

        builder.HasOne(e => e.Medico)
               .WithMany()
               .HasForeignKey(e => e.IdMedico)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Consultorio)
               .WithMany()
               .HasForeignKey(e => e.IdConsultorio)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

