using MediaHub.Application.Configuration;
using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Application.Tests.Configuration;

public sealed class BuildCapabilitiesTests
{
    [Fact]
    public void PublicVersionOneConfigurationIsValid()
    {
        BuildCapabilities.PublicVersionOne.Validate();
    }

    [Fact]
    public void PublicBuildCannotEnablePersonalSynchronization()
    {
        var capabilities = new BuildCapabilities(
            CloudEnvironment.Production,
            IsInternalBuild: false,
            PersonalSynchronizationEnabled: true);

        Assert.Throws<InvalidOperationException>(capabilities.Validate);
    }

    [Theory]
    [InlineData(CloudEnvironment.Local)]
    [InlineData(CloudEnvironment.Development)]
    [InlineData(CloudEnvironment.Staging)]
    public void PublicBuildCannotTargetInternalEnvironment(CloudEnvironment environment)
    {
        var capabilities = new BuildCapabilities(
            environment,
            IsInternalBuild: false,
            PersonalSynchronizationEnabled: false);

        Assert.Throws<InvalidOperationException>(capabilities.Validate);
    }
}
