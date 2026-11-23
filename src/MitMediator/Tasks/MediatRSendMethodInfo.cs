using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MitMediator.Tasks;

/// <summary>
/// Static info about send method compatible with MediatR.
/// </summary>
[RequiresDynamicCode("The compatibility send overload creates generic methods at runtime.")]
[RequiresUnreferencedCode("The compatibility send overload uses reflection over the concrete request type.")]
internal static class MediatRSendMethodInfo
{
    public static readonly MethodInfo SendGenericMethod = typeof(Mediator)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .First(m => m is { Name: "Send", IsGenericMethodDefinition: true }
                    && m.GetGenericArguments().Length == 2
                    && m.GetParameters().Length == 2);

    public static ConcurrentDictionary<Type, MethodInfo> SendMethod { get; } = new();
}
