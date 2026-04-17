const API_BASE = "http://localhost:5000/api";

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = localStorage.getItem("token");

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string> || {}),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Request failed");
  }

  // Handle empty responses (like register endpoint)
  const contentLength = response.headers.get("content-length");
  if (contentLength === "0" || response.status === 204) {
    return {} as T;
  }

  // Parse JSON response
  const text = await response.text();
  if (!text || text.trim() === "") {
    return {} as T;
  }

  return JSON.parse(text) as T;
}