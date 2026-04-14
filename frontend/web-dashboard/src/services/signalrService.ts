import * as signalR from "@microsoft/signalr";
import type { TelemetryDto } from "../types/Telemetry";

const HUB_URL = "http://localhost:5000/telemetryHub";

export let connection: signalR.HubConnection | null = null;

export async function startConnection(
  onMessage: (data: TelemetryDto) => void
) {
  if (connection?.state === "Connected" || connection?.state === "Connecting") return;

  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveTelemetry", (data: TelemetryDto) => {
      console.log("WS RECEIVED:", data);
      onMessage(data);
    });
  }

  try {
    await connection.start();
    console.log("SignalR connected");
  } catch (err) {
    console.error("SignalR failed:", err);
  }
}

export async function stopConnection() {
  if (connection) {
    await connection.stop();
    connection = null;
  }
}