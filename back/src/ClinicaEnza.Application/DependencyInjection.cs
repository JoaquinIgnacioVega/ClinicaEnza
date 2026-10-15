using System.Reflection;
using ClinicaEnza.Application.Common.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaEnza.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registrar Mediator artesanal
        services.AddScoped<IMediator, Mediator>();

        // Escanear y registrar automáticamente todos los Handlers del ensamblado
        var assembly = Assembly.GetExecutingAssembly();
        var handlerInterfaceType = typeof(IRequestHandler<,>);

        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == handlerInterfaceType);

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        return services;
    }
}

