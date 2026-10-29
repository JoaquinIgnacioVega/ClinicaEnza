using ClinicaEnza.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaEnza.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(e => e.Legajo).IsRequired().HasMaxLength(50);
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.Rol).IsRequired();
        builder.Property(e => e.FechaInicio).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.FechaBaja).IsRequired(false);

        builder.HasIndex(e => e.Legajo).IsUnique();

        builder.HasOne(e => e.Medico)
               .WithOne()
               .HasForeignKey<Usuario>(e => e.IdMedico)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

