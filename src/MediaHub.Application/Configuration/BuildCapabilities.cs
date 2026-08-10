using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Application.Configuration;

public sealed record BuildCapabilities(
    CloudEnvironment Environment,
    bool IsInternalBuild,
    bool PersonalSynchronizationEnabled)
{
    public static BuildCapabilities PublicVersionOne { get; } = new(
        CloudEnvironment.Production,
        IsInternalBuild: false,
        PersonalSynchronizationEnabled: false);

    public void Validate()
    {
        if (!IsInternalBuild && PersonalSynchronizationEnabled)
        {
            throw new InvalidOperationException(
                "Personal synchronization cannot be enabled in a public version 1.0 build.");
        }

        if (!IsInternalBuild && Environment is CloudEnvironment.Local or CloudEnvironment.Development or CloudEnvironment.Staging)
        {
            throw new InvalidOperationException(
                "A public build cannot target a non-production backend environment.");
        }
    }
}
