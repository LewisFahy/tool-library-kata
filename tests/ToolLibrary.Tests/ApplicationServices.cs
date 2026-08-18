using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ToolLibrary.Configuration;

namespace ToolLibrary.Tests;

/// <summary>
/// Test-side plumbing for the container tests. You shouldn't need to change this.
/// </summary>
internal static class ApplicationServices
{
    private static readonly Assembly ToolLibraryAssembly = typeof(ToolLibraryModule).Assembly;

    private const string ApplicationNamespace = "ToolLibrary.Application";

    /// <summary>
    /// An application service is a public, concrete class in the Application namespace
    /// that has a public Execute method. Commands and results live there too, but they
    /// don't execute anything, so they're ignored.
    /// </summary>
    public static IReadOnlyCollection<Type> Discover() =>
        Candidates().Where(HasExecuteMethod).ToArray();

    public static IEnumerable<Type> Candidates() =>
        ToolLibraryAssembly
            .GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Namespace is not null && t.Namespace.StartsWith(ApplicationNamespace, StringComparison.Ordinal));

    public static bool HasExecuteMethod(Type type) =>
        PublicInstanceMethods(type).Any(m => m.Name == "Execute");

    public static IEnumerable<MethodInfo> PublicInstanceMethods(Type type) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName);
}
