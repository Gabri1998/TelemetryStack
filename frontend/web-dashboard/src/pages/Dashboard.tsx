import { useEffect, useState, useMemo, useRef } from "react";
import { getTelemetry } from "../services/telemetryService";
import { startConnection } from "../services/signalrService";
import type { TelemetryDto } from "../types/Telemetry";
import { connection } from "../services/signalrService";
import { getDeviceStatus, getDevices } from "../services/deviceService";
import { Thermometer, Battery, Gauge } from "lucide-react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
} from "recharts";

export default function Dashboard() {
  const [data, setData] = useState<TelemetryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [deviceId, setDeviceId] = useState<string>("");
  const [devices, setDevices] = useState<string[]>([]);
  const [status, setStatus] = useState<boolean>(false);

  const previousDeviceRef = useRef<string | null>(null);
  const deviceRef = useRef<string>("");
  const startedRef = useRef(false);

  const formatted = useMemo(() => {
    return data.map((t) => ({
      ...t,
      time: new Date(t.timestamp).toLocaleTimeString(),
    }));
  }, [data]);

  useEffect(() => {
    getDevices().then((res) => {
      setDevices(res);
      if (res.length > 0) {
        setDeviceId(res[0]);
      }
    });
  }, []);

  useEffect(() => {
    deviceRef.current = deviceId;
  }, [deviceId]);

  useEffect(() => {
    if (!deviceId) return;

    let isMounted = true;

    setLoading(true);
    setData([]);

    getTelemetry(deviceId, 10).then((res) => {
      if (isMounted) {
        setData(res);
        setLoading(false);
      }
    });

    return () => {
      isMounted = false;
    };
  }, [deviceId]);

  useEffect(() => {
    if (startedRef.current) return;
    startedRef.current = true;

    startConnection((incoming) => {
      setData((prev) => {
        if (incoming.deviceId !== deviceRef.current) return prev;

        const exists = prev.some(
          (p) =>
            p.deviceId === incoming.deviceId &&
            p.timestamp === incoming.timestamp
        );

        if (exists) return prev;

        return [...prev, incoming]
          .sort(
            (a, b) =>
              new Date(a.timestamp).getTime() -
              new Date(b.timestamp).getTime()
          )
          .slice(-20);
      });
    });
  }, []);

  useEffect(() => {
    if (!deviceId) return;

    let isMounted = true;

    const fetchStatus = async () => {
      try {
        const res = await getDeviceStatus(deviceId);
        if (isMounted) setStatus(res.online);
      } catch {
        if (isMounted) setStatus(false);
      }
    };

    fetchStatus();
    const interval = setInterval(fetchStatus, 5000);

    return () => {
      isMounted = false;
      clearInterval(interval);
    };
  }, [deviceId]);

  useEffect(() => {
    if (!deviceId || !connection) return;

    const conn = connection;

    const switchGroup = async () => {
      try {
        while (conn.state !== "Connected") {
          await new Promise((res) => setTimeout(res, 100));
        }

        if (previousDeviceRef.current) {
          await conn.invoke("LeaveDevice", previousDeviceRef.current);
        }

        await conn.invoke("JoinDevice", deviceId);
        previousDeviceRef.current = deviceId;
      } catch (err) {
        console.error(err);
      }
    };

    switchGroup();
  }, [deviceId]);

  // ✅ ALL HOOKS ABOVE - conditional returns below
  if (!deviceId) {
    return (
      <div className="max-w-5xl mx-auto p-6">
        <div className="text-center py-12">
          <div className="text-gray-500">No devices found. Please add a device first.</div>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="max-w-5xl mx-auto p-6">
        <div className="text-center py-12">
          <div className="animate-pulse text-gray-500">Loading telemetry data...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6 text-center">
        Telemetry Dashboard
      </h1>
      <div className="flex justify-center items-center gap-2 mb-4">
        <span className={`w-3 h-3 rounded-full ${status ? "bg-green-500" : "bg-red-500"}`} />
        <span className="font-medium">
          {status ? "Online" : "Offline"}
        </span>
      </div>

      <div className="mb-6 flex justify-center">
        <select
          value={deviceId}
          onChange={(e) => setDeviceId(e.target.value)}
          className="border rounded px-3 py-2 shadow-sm"
        >
          {devices.map((d) => (
            <option key={d} value={d}>
              Device {d.slice(0, 6)}
            </option>
          ))}
        </select>
      </div>

      <div className="bg-white rounded-xl shadow p-4 mb-6 overflow-x-auto">
        <LineChart width={700} height={300} data={formatted}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" />
          <YAxis />
          <Tooltip />
          <Line type="monotone" dataKey="temperature" stroke="#8884d8" />
          <Line type="monotone" dataKey="speed" stroke="#82ca9d" />
          <Line type="monotone" dataKey="battery" stroke="#ffc658" />
        </LineChart>
      </div>

      <div className="grid gap-4">
        {formatted.length === 0 ? (
          <div className="text-center text-gray-500">
            No telemetry for this device
          </div>
        ) : (
          [...formatted].reverse().map((t) => (
            <div
              key={`${t.deviceId}-${t.timestamp}`}
              className="bg-white rounded-2xl shadow-md p-5 hover:shadow-lg transition"
            >
              <div className="font-semibold mb-2">{t.deviceId}</div>
              <div className="grid grid-cols-3 gap-4 text-sm">
                <div className="flex items-center gap-2">
                  <Thermometer
                    className={`w-4 h-4 ${
                      t.temperature > 30 ? "text-red-500" : "text-blue-500"
                    }`}
                  />
                  <span>{t.temperature}°C</span>
                </div>

                <div className="flex items-center gap-2">
                  <Gauge className="w-4 h-4 text-blue-500" />
                  <span>{t.speed} km/h</span>
                </div>

                <div className="flex items-center gap-2">
                  <Battery
                    className={`w-4 h-4 ${
                      t.battery > 50 ? "text-green-500" :
                      t.battery > 20 ? "text-yellow-500" :
                      "text-red-500"
                    }`}
                  />
                  <span>{t.battery}%</span>
                </div>
              </div>
              <div className="text-xs text-gray-500 mt-2">
                {new Date(t.timestamp).toLocaleString()}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}