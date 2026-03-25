import { useEffect, useState } from "react";
import { getTelemetry } from "../services/telemetryService";
import { Telemetry } from "../types/Telemetry";

const DEVICE_ID = "0a60c7a4-6bb1-4253-93fc-0523ecac2f3e";

export default function Dashboard() {
  const [data, setData] = useState<Telemetry[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getTelemetry(DEVICE_ID, 10)
      .then(setData)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Loading...</p>;

  return (
    <div>
      <h1>Telemetry Dashboard</h1>

      {data.map((t, i) => (
        <div key={i}>
          <p>Temp: {t.temperature}</p>
          <p>Speed: {t.speed}</p>
          <p>Battery: {t.battery}</p>
          <p>Time: {t.timestamp}</p>
          <hr />
        </div>
      ))}
    </div>
  );
}