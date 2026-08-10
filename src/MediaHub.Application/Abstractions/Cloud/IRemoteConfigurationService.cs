using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Application.Abstractions.Cloud;

public interface IRemoteConfigurationService
{
    Task<RemoteConfigurationSnapshot> RefreshAsync(CancellationToken cancellationToken);
}
