import {
  correlationId,
  errorResponse,
  isRecord,
  isSafeIdentifier,
  jsonResponse,
  readJsonBody,
} from "../_shared/http.ts";
import { createServerClient } from "../_shared/supabase.ts";

const allowedMediaTypes = new Set(["movie", "series", "season", "episode", "person"]);
const allowedPayloadFields = new Set([
  "title",
  "originalTitle",
  "overview",
  "releaseDate",
  "year",
  "runtimeMinutes",
  "genres",
  "artwork",
  "episodes",
]);

function allowListedPayload(payload: unknown): Record<string, unknown> {
  if (!isRecord(payload)) return {};
  return Object.fromEntries(
    Object.entries(payload).filter(([key]) => allowedPayloadFields.has(key)),
  );
}

Deno.serve(async (request) => {
  const requestCorrelationId = correlationId(request);
  if (request.method !== "POST") {
    return errorResponse("method_not_allowed", 405, requestCorrelationId);
  }

  try {
    const input = await readJsonBody(request);
    if (
      !isRecord(input) ||
      !isSafeIdentifier(input.providerKey, 100) ||
      !isSafeIdentifier(input.externalId, 200) ||
      typeof input.mediaType !== "string" ||
      !allowedMediaTypes.has(input.mediaType) ||
      (input.originalLocale !== undefined && !isSafeIdentifier(input.originalLocale, 20))
    ) {
      return errorResponse("invalid_request", 400, requestCorrelationId);
    }

    const localePreference = ["hu-HU", input.originalLocale, "en"]
      .filter((value): value is string => typeof value === "string")
      .filter((value, index, values) => values.indexOf(value) === index);

    const { data, error } = await createServerClient()
      .from("metadata_cache")
      .select(
        "provider_key,media_type,external_id,locale,payload,attribution,fetched_at,expires_at",
      )
      .eq("provider_key", input.providerKey)
      .eq("media_type", input.mediaType)
      .eq("external_id", input.externalId)
      .in("locale", localePreference)
      .gt("expires_at", new Date().toISOString());

    if (error) throw error;
    const selected = localePreference
      .map((locale) => data?.find((row) => row.locale === locale))
      .find((row) => row !== undefined);

    if (!selected) return errorResponse("not_found", 404, requestCorrelationId);

    return jsonResponse(
      {
        providerKey: selected.provider_key,
        mediaType: selected.media_type,
        externalId: selected.external_id,
        locale: selected.locale,
        payload: allowListedPayload(selected.payload),
        attribution: selected.attribution,
        fetchedAt: selected.fetched_at,
        expiresAt: selected.expires_at,
      },
      200,
      requestCorrelationId,
    );
  } catch (error) {
    if (error instanceof RangeError) {
      return errorResponse("payload_too_large", 413, requestCorrelationId);
    }
    console.error(
      JSON.stringify({ code: "metadata_backend_failure", correlationId: requestCorrelationId }),
    );
    return errorResponse("backend_failure", 503, requestCorrelationId);
  }
});
