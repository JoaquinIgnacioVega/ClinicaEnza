namespace ClinicaEnza.Application.Common.Mediator;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"No se encontró un handler registrado para la solicitud de tipo '{requestType.Name}'.");
        }

        var method = handlerType.GetMethod("Handle", new[] { requestType, typeof(CancellationToken) });
        if (method == null)
        {
            throw new InvalidOperationException($"El método 'Handle' no fue encontrado en '{handlerType.Name}'.");
        }

        var result = method.Invoke(handler, new object[] { request, cancellationToken });
        if (result is Task<TResponse> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"El resultado de invocar '{handlerType.Name}.Handle' no devolvió un Task<{typeof(TResponse).Name}>.");
    }
}

