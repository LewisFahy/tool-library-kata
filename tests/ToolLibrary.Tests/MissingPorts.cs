using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace ToolLibrary.Tests;

/// <summary>
/// Test-side plumbing for the container tests. You shouldn't need to change this.
///
/// The kata has no database and no email server, so the ports your services depend on have
/// no real implementations. Rather than make you register throwaway stubs, this fills in a
/// fake for any interface the container doesn't know about.
///
/// The point: your own registrations still have to be right. Only the outside world is
/// faked for you.
/// </summary>
internal static class MissingPorts
{
    public static IServiceCollection FakeAnythingUnregistered(this IServiceCollection services, IEnumerable<Type> roots)
    {
        var pending = new Queue<Type>(roots);
        var visited = new HashSet<Type>();

        while (pending.Count > 0)
        {
            var type = pending.Dequeue();

            if (!visited.Add(type))
            {
                continue;
            }

            foreach (var dependency in ConstructorDependenciesOf(type))
            {
                if (services.Any(d => d.ServiceType == dependency))
                {
                    var registered = services.First(d => d.ServiceType == dependency).ImplementationType;

                    if (registered is not null)
                    {
                        pending.Enqueue(registered);
                    }

                    continue;
                }

                if (dependency.IsInterface || dependency.IsAbstract)
                {
                    services.AddSingleton(dependency, _ => Create.Fake(dependency));
                }
                else
                {
                    pending.Enqueue(dependency);
                }
            }
        }

        return services;
    }

    private static IEnumerable<Type> ConstructorDependenciesOf(Type type) =>
        type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault()
            ?.GetParameters()
            .Select(p => p.ParameterType)
            .Where(t => t != typeof(string) && !t.IsPrimitive && !t.IsValueType)
            ?? [];
}
