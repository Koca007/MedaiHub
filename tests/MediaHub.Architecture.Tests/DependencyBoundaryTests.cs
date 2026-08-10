using System.Reflection;
using MediaHub.Application.Configuration;
using MediaHub.Domain.Media;
using MediaHub.Scanner;
using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Architecture.Tests;

public sealed class DependencyBoundaryTests
{
    [Fact]
    public void DomainHasNoInfrastructureOrPresentationDependency()
    {
        var references = ReferencedAssemblyNames(typeof(MediaTitle).Assembly);

        Assert.DoesNotContain(references, name => name.StartsWith("MediaHub.Infrastructure", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name == "MediaHub.App");
        Assert.DoesNotContain(references, name => name.StartsWith("PresentationFramework", StringComparison.Ordinal));
    }

    [Fact]
    public void ApplicationHasNoConcreteInfrastructureDependency()
    {
        var references = ReferencedAssemblyNames(typeof(BuildCapabilities).Assembly);

        Assert.DoesNotContain(references, name => name.StartsWith("MediaHub.Infrastructure", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name == "MediaHub.App");
    }

    [Fact]
    public void SharedContractsStayFrameworkNeutral()
    {
        var references = ReferencedAssemblyNames(typeof(RemoteProtocol).Assembly);

        Assert.DoesNotContain(references, name => name.StartsWith("PresentationFramework", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.Contains("Supabase", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ScannerDoesNotReferencePlayerImplementation()
    {
        var references = ReferencedAssemblyNames(typeof(ScannerAssemblyMarker).Assembly);

        Assert.DoesNotContain("MediaHub.Player", references);
    }

    private static string[] ReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(name => name.Name ?? string.Empty)
            .ToArray();
}
