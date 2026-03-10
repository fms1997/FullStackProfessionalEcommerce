export default function Forbidden() {
  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold text-red-700">403 - Acceso denegado</h1>
      <p className="mt-2 text-sm opacity-80">No tienes permisos para acceder a este recurso.</p>
      <a className="inline-block mt-4 text-sm underline" href="/">
        Volver al inicio
      </a>
    </div>
  );
}
