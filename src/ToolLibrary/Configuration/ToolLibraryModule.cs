using System;
using Microsoft.Extensions.DependencyInjection;

namespace ToolLibrary.Configuration;

/// <summary>
/// The composition root for the tool library.
///
/// This is the ONLY place in the whole solution that is allowed to know how to build
/// anything. Nothing else calls <c>new</c> on an application service, and nothing else
/// touches the container. No service locator, no static access to the provider.
///
/// Round 2: as you build each interaction, register it here. The container tests will
/// fail until you do.
/// </summary>
public static class ToolLibraryModule
{
    /// <summary>
    /// Registers everything the tool library owns: its application services and its
    /// domain services.
    ///
    /// It deliberately does NOT register ports (repositories, clock, email, payments).
    /// Those are the host's problem — see <see cref="AddToolLibraryAdapters"/>.
    /// Ask yourself why that separation might matter before you collapse the two.
    /// </summary>
    public static IServiceCollection AddToolLibrary(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Application services — one per interaction.
        //   services.AddScoped<BorrowTool>();

        // Domain services — the things that hold rules but have no identity.
        //   services.AddScoped<ILendingPolicy, LendingPolicy>();

        return services;
    }

    /// <summary>
    /// Registers the real implementations of the ports the library depends on.
    ///
    /// In this kata there aren't any — there's no database and no email server. Leave it
    /// empty. It exists to make the seam visible: the domain declares what it needs, and
    /// somebody outside decides what actually satisfies it.
    /// </summary>
    public static IServiceCollection AddToolLibraryAdapters(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        //   services.AddSingleton<IClock, SystemClock>();
        //   services.AddScoped<IMemberRepository, SqlMemberRepository>();

        return services;
    }
}
