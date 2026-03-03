import { env } from "../config/env";

export default function Home() {
  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold">FulSpectrum</h1>
      <p className="mt-2 text-sm opacity-80">
        API: <span className="font-mono">{env.API_BASE_URL}</span>
      </p>
    </div>
  );
}