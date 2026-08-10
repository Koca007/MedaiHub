import {
  correlationId,
  errorResponse,
  isRecord,
  jsonResponse,
  readJsonBody,
} from "../_shared/http.ts";
import { createServerClient } from "../_shared/supabase.ts";

const channels = new Set(["stable", "beta", "developer"]);

Deno.serve(async (request) => {
  const requestCorrelationId = correlationId(request);
  if (request.method !== "POST") {
    return errorResponse("method_not_allowed", 405, requestCorrelationId);
  }

  try {
    const input = await readJsonBody(request);
    if (
      !isRecord(input) ||
      typeof input.channel !== "string" ||
      !channels.has(input.channel) ||
      !Number.isInteger(input.protocolVersion) ||
      Number(input.protocolVersion) < 1
    ) {
      return errorResponse("invalid_request", 400, requestCorrelationId);
    }

    const { data, error } = await createServerClient()
      .from("application_releases")
      .select(
        "version,channel,published_at,minimum_protocol,maximum_protocol,changelog,download_reference",
      )
      .eq("channel", input.channel)
      .eq("is_active", true)
      .lte("minimum_protocol", input.protocolVersion)
      .order("published_at", { ascending: false })
      .limit(20);

    if (error) throw error;
    const compatible = data?.find((release) =>
      release.maximum_protocol === null || release.maximum_protocol >= Number(input.protocolVersion)
    );

    return jsonResponse({ compatibleRelease: compatible ?? null }, 200, requestCorrelationId);
  } catch (error) {
    if (error instanceof RangeError) {
      return errorResponse("payload_too_large", 413, requestCorrelationId);
    }
    console.error(
      JSON.stringify({ code: "release_backend_failure", correlationId: requestCorrelationId }),
    );
    return errorResponse("backend_failure", 503, requestCorrelationId);
  }
});
