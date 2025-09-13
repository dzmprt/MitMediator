namespace MitMediator;

/// <summary>
/// Marker of requests type without a response type. (for reflection in extension projects)
/// </summary>
public interface IBaseRequest;

/// <summary>
/// Request type.
/// </summary>
/// <typeparam name="TResponse">Type of request response.</typeparam>
public interface IRequest<TResponse> : IBaseRequest;

/// <summary>
/// Request type with void response.
/// </summary>
public interface IRequest : IRequest<Unit>;
