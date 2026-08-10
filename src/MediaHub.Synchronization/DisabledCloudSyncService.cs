using MediaHub.Application.Abstractions.Cloud;
using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Synchronization;

public sealed class DisabledCloudSyncService : ICloudSyncService
{
    private static readonly SyncStatusDto DisabledStatus = new(
        IsEnabled: false,
        PendingOperations: 0,
        FailedOperations: 0,
        LastRemoteSequence: 0,
        LastSuccessfulSyncAtUtc: null);

    public Task<SyncRunResult> SynchronizeAsync(
        SyncReason reason,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new SyncRunResult(false, 0, 0, null));
    }

    public Task<SyncStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(DisabledStatus);
    }
}
