export default function NotFound() {
  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold">404</h1>
      <p className="mt-2 text-sm opacity-80">Ruta no encontrada.</p>
      <a className="inline-block mt-4 text-sm underline" href="/">
        Volver
      </a>
    </div>
  );
}