using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Helpdesk.Tickets.Application.Abstractions;

public static class HandlerRegistration
{
    public static IServiceCollection AddCommandHandler<TCommand, TResponse, THandler>(
        this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(sp =>
            new LoggingCommandDecorator<TCommand, TResponse>(
                new ValidationCommandDecorator<TCommand, TResponse>(
                    sp.GetRequiredService<THandler>(),
                    sp.GetServices<IValidator<TCommand>>()),
                sp.GetRequiredService<ILogger<LoggingCommandDecorator<TCommand, TResponse>>>()));
        return services;
    }

    public static IServiceCollection AddQueryHandler<TQuery, TResponse, THandler>(
        this IServiceCollection services)
        where TQuery : IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResponse>>(sp =>
            new LoggingQueryDecorator<TQuery, TResponse>(
                sp.GetRequiredService<THandler>(),
                sp.GetRequiredService<ILogger<LoggingQueryDecorator<TQuery, TResponse>>>()));
        return services;
    }
}
