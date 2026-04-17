import { apiFetch } from "./api";
import type { DeviceDto } from "../types/Device";
import type { DeviceStatusDto } from "../types/Telemetry";

export async function getDevices(): Promise<string[]> {
  const data = await apiFetch<DeviceDto[]>("/devices");
  return data.map((d) => d.id);
}

export async function getDeviceStatus(
  deviceId: string
): Promise<DeviceStatusDto> {
  return apiFetch(`/devices/${deviceId}/status`);
}