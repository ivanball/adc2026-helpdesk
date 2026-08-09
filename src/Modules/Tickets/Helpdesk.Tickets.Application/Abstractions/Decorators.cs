using System.Diagnostics;
using Helpdesk.SharedKernel;
using Microsoft.Extensions.Logging;

namespace Helpdesk.Tickets.Application.Abstractions;

internal sealed class ValidationCommandDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
    {
        var errors = validators.SelectMany(v => v.Validate(command)).ToList();
        return errors.Count > 0
            ? Task.FromResult(Result.Failure<TResponse>(errors[0]))
            : inner.Handle(command, ct);
    }
}

internal sealed class LoggingCommandDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    ILogger<LoggingCommandDecorator<TCommand, TResponse>> logger)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
    {
        var name = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();
        var result = await inner.Handle(command, ct);
        if (result.IsSuccess)
            logger.LogInformation("{Command} handled in {Ms}ms", name, stopwatch.ElapsedMilliseconds);
        else
            logger.LogWarning("{Command} failed: {Error} ({Ms}ms)",
                name, result.Error.Code, stopwatch.ElapsedMilliseconds);
        return result;
    }
}

internal sealed class LoggingQueryDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> inner,
    ILogger<LoggingQueryDecorator<TQuery, TResponse>> logger)
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await inner.Handle(query, ct);
        logger.LogInformation("{Query} handled in {Ms}ms",
            typeof(TQuery).Name, stopwatch.ElapsedMilliseconds);
        return result;
    }
}
