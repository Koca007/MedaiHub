using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Infrastructure.Supabase;

public sealed record SupabaseConnectionOptions(
    CloudEnvironment Environment,
    Uri Url,
    string PublishableKey,
    int ProtocolVersion)
{
    public void Validate()
    {
        if (!Url.IsAbsoluteUri)
        {
            throw new InvalidOperationException("The Supabase URL must be absolute.");
        }

        if (Url.Scheme != Uri.UriSchemeHttps && !Url.IsLoopback)
        {
            throw new InvalidOperationException("A non-local Supabase endpoint must use HTTPS.");
        }

        if (string.IsNullOrWhiteSpace(PublishableKey))
        {
            throw new InvalidOperationException("A Supabase publishable key is required.");
        }

        if (PublishableKey.Contains("service_role", StringComparison.OrdinalIgnoreCase)
            || PublishableKey.StartsWith("sb_secret_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Privileged Supabase credentials cannot be used by the desktop client.");
        }

        if (ProtocolVersion is < RemoteProtocol.MinimumSupportedVersion or > RemoteProtocol.MaximumSupportedVersion)
        {
            throw new InvalidOperationException("The configured remote protocol is not supported by this client.");
        }
    }
}
