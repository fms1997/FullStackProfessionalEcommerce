import { Outlet } from "react-router-dom";

export default function AppLayout() {
  return (
    <div className="min-h-screen">
      <header className="border-b p-4">
        <div className="font-semibold">FulSpectrum</div>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
}