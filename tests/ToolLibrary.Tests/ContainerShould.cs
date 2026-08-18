using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ToolLibrary.Configuration;

namespace ToolLibrary.Tests;

/// <summary>
/// These tests guard the composition root. They're part of the kata's constraints, not
/// scaffolding to be deleted — if one of them is in your way, that's the lesson talking.
/// </summary>
[TestFixture]
public class ContainerShould
{
    [Test]
    public void Know_about_at_least_one_interaction()
    {
        var interactions = ApplicationServices.Discover();

        Assert.That(
            interactions,
            Is.Not.Empty,
            "No application services found. An application service is a public class in the " +
            "ToolLibrary.Application namespace with a public Execute method — one per interaction.");
    }

    [Test]
    public void Resolve_every_interaction_it_has_been_asked_to_build()
    {
        var interactions = ApplicationServices.Discover();

        var services = new ServiceCollection()
            .AddToolLibrary()
            .AddToolLibraryAdapters()
            .FakeAnythingUnregistered(interactions);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        using var scope = provider.CreateScope();

        Assert.Multiple(() =>
        {
            foreach (var interaction in interactions)
            {
                Assert.That(
                    () => scope.ServiceProvider.GetRequiredService(interaction),
                    Throws.Nothing,
                    $"{interaction.Name} could not be resolved. Register it in {nameof(ToolLibraryModule)}.");
            }
        });
    }

    [Test]
    public void Only_ever_be_touched_by_the_composition_root()
    {
        var typesReferencingTheContainer = ApplicationServices
            .Candidates()
            .Where(t => t.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Any(p => p.ParameterType == typeof(IServiceProvider)))
            .Select(t => t.Name)
            .ToArray();

        Assert.That(
            typesReferencingTheContainer,
            Is.Empty,
            "An application service is injecting IServiceProvider. That's a service locator: it hides "
            + "the dependency from the constructor and from this test. Ask for what you need instead.");
    }
}
