import { apiFetch } from "./api";
import type { TelemetryDto } from "../types/Telemetry";

export async function getTelemetry(
  deviceId: string,
  limit = 10
): Promise<TelemetryDto[]> {
  return apiFetch(`/devices/${deviceId}/telemetry?limit=${limit}`);
}