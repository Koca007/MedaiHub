export type ErrorCode =
  | "method_not_allowed"
  | "invalid_request"
  | "payload_too_large"
  | "not_found"
  | "backend_failure";

export function correlationId(request: Request): string {
  const incoming = request.headers.get("x-correlation-id");
  return incoming && /^[a-zA-Z0-9_-]{8,80}$/.test(incoming) ? incoming : crypto.randomUUID();
}

export function jsonResponse(
  body: unknown,
  status: number,
  requestCorrelationId: string,
): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: {
      "content-type": "application/json; charset=utf-8",
      "cache-control": "no-store",
      "x-correlation-id": requestCorrelationId,
      "x-content-type-options": "nosniff",
    },
  });
}

export function errorResponse(
  code: ErrorCode,
  status: number,
  requestCorrelationId: string,
): Response {
  return jsonResponse(
    { error: { code, correlationId: requestCorrelationId } },
    status,
    requestCorrelationId,
  );
}

export async function readJsonBody(request: Request, maximumBytes = 4096): Promise<unknown> {
  const declaredLength = Number(request.headers.get("content-length") ?? "0");
  if (declaredLength > maximumBytes) {
    throw new RangeError("payload_too_large");
  }

  const text = await request.text();
  if (new TextEncoder().encode(text).byteLength > maximumBytes) {
    throw new RangeError("payload_too_large");
  }

  return JSON.parse(text) as unknown;
}

export function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

export function isSafeIdentifier(value: unknown, maximumLength = 200): value is string {
  return typeof value === "string" &&
    value.length > 0 &&
    value.length <= maximumLength &&
    /^[a-zA-Z0-9._:-]+$/.test(value);
}
