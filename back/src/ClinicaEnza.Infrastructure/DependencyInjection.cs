using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Infrastructure.Persistence;
using ClinicaEnza.Infrastructure.Persistence.Context;
using ClinicaEnza.Infrastructure.Persistence.Repositories;
using ClinicaEnza.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaEnza.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Data Source=clinica_enza.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // Registrar UnitOfWork y Repositorios
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITurnoRepository, TurnoRepository>();
        services.AddScoped<IHorarioAtencionRepository, HorarioAtencionRepository>();
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Servicios de infraestructura
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}

