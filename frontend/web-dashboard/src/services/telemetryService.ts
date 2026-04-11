import type { Telemetry } from "../types/Telemetry";

const API_BASE = "http://localhost:5000/api";

export async function getTelemetry(
  deviceId: string,
  limit = 10
): Promise<Telemetry[]> {
  const res = await fetch(
    `${API_BASE}/devices/${deviceId}/telemetry?limit=${limit}`
  );

  const json = await res.json();

  if (!json) return [];

  if (Array.isArray(json)) return json;

  return Object.values(json);
}