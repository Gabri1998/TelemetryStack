// types/Telemetry.ts

export interface TelemetryDto {
  deviceId: string;
  temperature: number;
  speed: number;
  battery: number;
  timestamp: string;
}

export interface DeviceStatusDto {
  deviceId: string;
  online: boolean;
}