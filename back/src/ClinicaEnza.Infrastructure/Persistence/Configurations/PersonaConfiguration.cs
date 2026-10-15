using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.ToTable("Personas");
        builder.HasKey(e => e.IdPersona);

        builder.HasDiscriminator<string>("TipoPersona")
               .HasValue<Paciente>("Paciente")
               .HasValue<Medico>("Medico")
               .HasValue<Usuario>("Usuario");

        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Dni).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Mail).HasMaxLength(150);

        builder.HasIndex(e => e.Dni).IsUnique();
    }
}

