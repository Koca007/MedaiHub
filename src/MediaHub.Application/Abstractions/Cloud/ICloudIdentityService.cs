using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Application.Abstractions.Cloud;

public interface ICloudIdentityService
{
    Task<CloudIdentityState> GetStateAsync(CancellationToken cancellationToken);
}
