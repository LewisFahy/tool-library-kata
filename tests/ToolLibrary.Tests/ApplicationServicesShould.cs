using System.Linq;
using NUnit.Framework;

namespace ToolLibrary.Tests;

[TestFixture]
public class ApplicationServicesShould
{
    [Test]
    public void Expose_one_public_method_each_named_Execute()
    {
        var offenders = ApplicationServices
            .Discover()
            .Select(t => new
            {
                Service = t.Name,
                PublicMethods = ApplicationServices.PublicInstanceMethods(t).Select(m => m.Name).ToArray()
            })
            .Where(x => x.PublicMethods.Length != 1)
            .Select(x => $"{x.Service} exposes: {string.Join(", ", x.PublicMethods)}")
            .ToArray();

        Assert.That(
            offenders,
            Is.Empty,
            "An application service models a single interaction, so it has a single public method: Execute. "
            + "If you need a second one, you've found a second interaction — give it its own class.");
    }
}
