namespace MitMediator;

/// <summary>
/// Request type with void response.
/// </summary>
public interface IRequest : IRequest<Unit>;

/// <summary>
/// Request type.
/// </summary>
/// <typeparam name="TResponse">Type of request response.</typeparam>
public interface IRequest<TResponse>;
