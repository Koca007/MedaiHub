using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Application.Abstractions.Cloud;

public interface ICloudSyncService
{
    Task<SyncRunResult> SynchronizeAsync(SyncReason reason, CancellationToken cancellationToken);

    Task<SyncStatusDto> GetStatusAsync(CancellationToken cancellationToken);
}
