# Volume 32 — Plugin SDK Examples

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Minimal metadata provider

These examples are used by built-in-extension development and the internal compatibility test host. Public Windows 1.0 does not install the generated `.mhpkg` output. The approved 1.0 metadata implementation uses the same provider contract from the signed application bundle.

```csharp
namespace Example.Metadata;

public sealed class ExampleMetadataProvider : IMetadataProvider
{
    public ProviderDescriptor Descriptor { get; } = new(
        Id: "com.example.metadata",
        DisplayName: "Example Metadata",
        Version: new Version(1, 0, 0));

    /// <summary>
    /// Searches the provider using only the host-supplied, capability-scoped HTTP client.
    /// </summary>
    public async Task<IReadOnlyList<MetadataSearchResult>> SearchAsync(
        MetadataSearchRequest request,
        ProviderContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Array.Empty<MetadataSearchResult>();
        }

        var uri = ProviderUriBuilder.BuildSearchUri(request);
        using var response = await context.Http.GetAsync(uri, cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.ReadJsonAsync<SearchPayload>(
            cancellationToken);

        return payload.Items
            .Take(50)
            .Select(ProviderMapper.ToSearchResult)
            .ToArray();
    }

    public Task<MetadataEnvelope?> GetAsync(
        ExternalMediaId id,
        MetadataRequestOptions options,
        ProviderContext context,
        CancellationToken cancellationToken)
    {
        // Implementation follows the same validation, size, timeout,
        // and cancellation rules as SearchAsync.
        throw new NotImplementedException();
    }
}
```

## 2. Manifest

```json
{
  "schemaVersion": 1,
  "id": "com.example.metadata",
  "name": "Example Metadata Provider",
  "version": "1.0.0",
  "publisher": "Example",
  "entryAssembly": "Example.Metadata.dll",
  "minimumHostVersion": "1.0.0",
  "sdkVersion": "1.0",
  "capabilities": [
    "metadata.search",
    "metadata.read",
    "network.https"
  ],
  "networkHosts": [
    "api.example.invalid"
  ],
  "supportedLocales": ["en", "hu"]
}
```

## 3. Subtitle provider result

The subtitle provider contract is reserved for the post-1.0 online-subtitle milestone. Version 1.0 subtitle handling remains embedded, sidecar, and manual import.

```csharp
public sealed record SubtitleCandidate(
    SubtitleCandidateId Id,
    string Language,
    string? DisplayTitle,
    bool IsForced,
    bool IsHearingImpaired,
    SubtitleFormat Format,
    decimal? ProviderScore,
    SubtitleSyncEvidence SyncEvidence);
```

Providers do not choose the active subtitle.

## 4. Declarative settings schema

```json
{
  "schemaVersion": 1,
  "sections": [
    {
      "id": "account",
      "titleResource": "Settings.Account.Title",
      "fields": [
        {
          "key": "apiKey",
          "type": "secret",
          "labelResource": "Settings.ApiKey.Label",
          "required": true
        },
        {
          "key": "preferredLanguage",
          "type": "select",
          "labelResource": "Settings.Language.Label",
          "options": [
            {"value": "hu", "labelResource": "Language.Hungarian"},
            {"value": "en", "labelResource": "Language.English"}
          ]
        }
      ]
    }
  ]
}
```

The host stores the secret in protected storage and gives the plugin a scoped secret handle.

## 5. Provider HTTP

Plugins must use `IProviderHttpClient`. Forbidden:

- raw unrestricted `HttpClient`;
- disabling certificate validation;
- arbitrary local-network requests without capability;
- logging authorization headers;
- unlimited response buffering.

## 6. Cache example

```csharp
var cacheKey = new ProviderCacheKey(
    Namespace: "movie",
    Key: externalId.Value,
    Locale: context.UiLocale,
    SchemaVersion: 1);

var cached = await context.Cache.GetAsync<MetadataEnvelope>(
    cacheKey,
    cancellationToken);

if (cached is not null)
{
    return cached;
}
```

Cache payload size and lifetime are host-enforced.

## 7. Logging example

```csharp
context.Logger.LogInformation(
    PluginEventIds.SearchCompleted,
    "Metadata search completed with {ResultCount} results.",
    results.Count);
```

Do not log query titles if plugin privacy policy and host setting disallow content logging.

## 8. Cancellation

Every provider operation:

- passes token to HTTP/cache;
- stops parsing when cancelled;
- does not wrap cancellation as failure;
- cleans temporary files;
- does not retry after user cancellation.

## 9. Acquisition provider boundary

```csharp
public interface IAcquisitionProvider
{
    Task<IReadOnlyList<AcquisitionCandidate>> SearchAsync(
        AcquisitionSearchRequest request,
        ProviderContext context,
        CancellationToken cancellationToken);

    Task<AcquisitionResolution> ResolveAsync(
        AcquisitionCandidateId candidateId,
        ProviderContext context,
        CancellationToken cancellationToken);
}
```

Resolution returns one of:

- authorized HTTP download;
- external-client handoff;
- browser details page;
- unavailable.

It cannot invoke a process directly.

## 10. Declarative dashboard row

A restricted plugin may return:

```csharp
public sealed record DashboardRowContribution(
    string StableId,
    LocalizedText Heading,
    IReadOnlyList<DashboardCardModel> Cards,
    DashboardRowAction? SeeAll);
```

Cards refer to host-recognized entities/actions. Arbitrary markup is not accepted.

## 11. Plugin validation checklist

- manifest ID stable and reverse-domain style;
- version valid;
- no duplicate assembly;
- no native binaries without declaration;
- requested capabilities justified;
- HTTPS domains narrow;
- locales valid;
- package paths safe;
- archive limits;
- license present;
- SDK compatibility;
- no forbidden references, where static validation can detect.

## 12. Compatibility tests

Plugin authors test against:

- minimum host;
- current stable;
- next SDK preview;
- network timeout;
- invalid JSON;
- rate limit;
- cancellation;
- host cache unavailable;
- localization fallback;
- permission denied.

## 13. Developer packaging command

```text
mediahub-plugin pack \
  --manifest manifest.json \
  --output Example.Metadata.1.0.0.mhpkg
```

Validator prints package hash and capability summary.

## 14. Security review questions

- What data leaves the device?
- Which hosts receive it?
- Are media fingerprints sent?
- Are titles/searches logged?
- What credentials are required?
- Is downloaded content validated?
- Can the plugin function without broad file access?
- What happens when service is unavailable?
- How is account deletion handled?

## 15. Acceptance criteria

- Sample metadata plugin activates in the internal test host.
- Permissions match manifest.
- Unlisted network host is blocked.
- Secret remains protected.
- Timeout and cancellation work.
- Invalid package path is rejected.
- Host renders settings accessibly.
- Public Windows 1.0 rejects or ignores external `.mhpkg` installation attempts because no production install surface is exposed.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
