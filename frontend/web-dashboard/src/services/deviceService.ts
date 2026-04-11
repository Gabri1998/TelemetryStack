// services/deviceService.ts
export async function getDevices(): Promise<string[]> {
  const res = await fetch("http://localhost:5000/api/devices");
  const json = await res.json();

  return json.map((d: any) => d.id);
}

export async function getDeviceStatus(deviceId: string) {
  const res = await fetch(
  `http://localhost:5000/api/devices/${deviceId}/status`
);

if (!res.ok) {
  return { online: false };
}

return await res.json();
}