using System.Xml.Linq;

namespace MediaHub.Architecture.Tests;

public sealed class MainWindowBindingTests
{
    [Theory]
    [InlineData("DiscoveredFiles")]
    [InlineData("IndexedFiles")]
    [InlineData("SkippedFiles")]
    public void ReadOnlyScanCountersUseOneWayBindings(string propertyName)
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var xamlPath = Path.Combine(
            repositoryRoot,
            "src",
            "MediaHub.App",
            "MainWindow.xaml");
        var document = XDocument.Load(xamlPath);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        var binding = document
            .Descendants(presentation + "Run")
            .Select(run => run.Attribute("Text")?.Value)
            .Single(value => value?.Contains(propertyName, StringComparison.Ordinal) == true);

        Assert.Contains("Mode=OneWay", binding, StringComparison.Ordinal);
    }
}
