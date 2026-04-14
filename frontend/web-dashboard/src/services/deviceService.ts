import type { DeviceDto } from "../types/Device";
import type { DeviceStatusDto } from "../types/Telemetry";

export async function getDevices(): Promise<string[]> {
  const res = await fetch("http://localhost:5000/api/devices");

  if (!res.ok) {
    throw new Error("Failed to fetch devices");
  }

  const json: DeviceDto[] = await res.json();

  return json.map((d) => d.id);
}

export async function getDeviceStatus(
  deviceId: string
): Promise<DeviceStatusDto> {
  const res = await fetch(
    `http://localhost:5000/api/devices/${deviceId}/status`
  );

  if (!res.ok) {
    return {
      deviceId,
      online: false,
    };
  }

  return await res.json();
}