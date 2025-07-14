using System.Collections.Concurrent;
using System.Reflection;

namespace MitMediator.Tasks;

/// <summary>
/// Static info about send method compatible with MediatR.
/// </summary>
internal static class MediatRSendMethodInfo
{
    public static readonly MethodInfo SendGenericMethod = typeof(Mediator)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .First(m => m.Name == "Send"
                    && m.IsGenericMethodDefinition
                    && m.GetGenericArguments().Length == 2
                    && m.GetParameters().Length == 2);
    
    public static ConcurrentDictionary<Type, MethodInfo> SendMethod { get; } = new();
}