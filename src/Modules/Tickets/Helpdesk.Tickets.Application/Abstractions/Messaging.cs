using Helpdesk.SharedKernel;

namespace Helpdesk.Tickets.Application.Abstractions;

public interface ICommand<TResponse> { }
public interface IQuery<TResponse> { }

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct);
}

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct);
}

public interface IValidator<in T>
{
    IReadOnlyList<Error> Validate(T instance);
}
