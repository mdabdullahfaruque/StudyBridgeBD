using Microsoft.Extensions.DependencyInjection;

namespace StudyBridge.Shared.CQRS;

public interface IDispatcher
{
    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
    Task CommandAsync(ICommand command, CancellationToken cancellationToken = default);
    Task<TResponse> CommandAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}

public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResponse>, TResponse>.HandleAsync));
        var result = method!.Invoke(handler, new object[] { query, cancellationToken });
        
        return await (Task<TResponse>)result!;
    }

    public async Task CommandAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.HandleAsync));
        var result = method!.Invoke(handler, new object[] { command, cancellationToken });
        
        await (Task)result!;
    }

    public async Task<TResponse> CommandAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResponse>, TResponse>.HandleAsync));
        var result = method!.Invoke(handler, new object[] { command, cancellationToken });
        
        return await (Task<TResponse>)result!;
    }
}
