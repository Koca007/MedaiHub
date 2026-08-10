namespace MediaHub.Shared.Contracts.Cloud;

public enum CloudEnvironment
{
    Local = 1,
    Development = 2,
    Staging = 3,
    Production = 4,
}

public enum CloudIdentityStatus
{
    Disabled = 1,
    Provisioning = 2,
    Authenticated = 3,
    Expired = 4,
    Unavailable = 5,
}

public sealed record CloudIdentityState(
    CloudIdentityStatus Status,
    Guid? UserId,
    bool IsSynthetic,
    DateTimeOffset? ExpiresAtUtc);

public enum SyncReason
{
    Startup = 1,
    Periodic = 2,
    LocalChange = 3,
    RealtimeHint = 4,
    UserRequested = 5,
}

public sealed record SyncRunResult(
    bool WasEnabled,
    int PushedOperations,
    int PulledChanges,
    string? ErrorCode);

public sealed record SyncStatusDto(
    bool IsEnabled,
    int PendingOperations,
    int FailedOperations,
    long LastRemoteSequence,
    DateTimeOffset? LastSuccessfulSyncAtUtc);

public sealed record RemoteConfigurationSnapshot(
    int ProtocolVersion,
    IReadOnlyDictionary<string, bool> FeatureFlags,
    DateTimeOffset FetchedAtUtc);

public sealed record ReleaseCatalogItem(
    string Version,
    string Channel,
    DateTimeOffset PublishedAtUtc,
    int MinimumProtocol,
    int? MaximumProtocol);
