import {
  correlationId,
  errorResponse,
  isRecord,
  isSafeIdentifier,
  jsonResponse,
  readJsonBody,
} from "../_shared/http.ts";
import { createServerClient } from "../_shared/supabase.ts";

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
      !isSafeIdentifier(input.seriesExternalId, 200) ||
      !Number.isInteger(input.seasonNumber) ||
      Number(input.seasonNumber) < 0 ||
      !Number.isInteger(input.episodeNumber) ||
      Number(input.episodeNumber) < 1 ||
      !isSafeIdentifier(input.regionCode, 10)
    ) {
      return errorResponse("invalid_request", 400, requestCorrelationId);
    }

    const { data, error } = await createServerClient()
      .from("episode_release_catalog")
      .select("official_air_date,provider_available,provider_available_at,updated_at")
      .eq("provider_key", input.providerKey)
      .eq("series_external_id", input.seriesExternalId)
      .eq("season_number", input.seasonNumber)
      .eq("episode_number", input.episodeNumber)
      .eq("region_code", String(input.regionCode).toUpperCase())
      .maybeSingle();

    if (error) throw error;
    if (!data) return errorResponse("not_found", 404, requestCorrelationId);

    const today = new Date().toISOString().slice(0, 10);
    const airDateHasArrived = data.official_air_date !== null && data.official_air_date <= today;

    return jsonResponse(
      {
        officialAirDate: data.official_air_date,
        providerAvailable: data.provider_available,
        providerAvailableAt: data.provider_available_at,
        remotelyAvailable: airDateHasArrived && data.provider_available,
        updatedAt: data.updated_at,
      },
      200,
      requestCorrelationId,
    );
  } catch (error) {
    if (error instanceof RangeError) {
      return errorResponse("payload_too_large", 413, requestCorrelationId);
    }
    console.error(
      JSON.stringify({ code: "episode_backend_failure", correlationId: requestCorrelationId }),
    );
    return errorResponse("backend_failure", 503, requestCorrelationId);
  }
});
