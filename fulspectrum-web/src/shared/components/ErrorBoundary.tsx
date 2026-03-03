import React from "react";

type Props = { children: React.ReactNode };
type State = { hasError: boolean };

export class ErrorBoundary extends React.Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: unknown, info: React.ErrorInfo) {
    // Etapa 0: console. Luego se integra con Sentry/monitoring.
    console.error("ErrorBoundary caught:", error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex items-center justify-center p-6">
          <div className="max-w-md w-full rounded-[var(--radius-md)] border p-4">
            <h1 className="text-lg font-semibold">Ocurrió un error</h1>
            <p className="mt-2 text-sm opacity-80">
              Probá recargar la página o volver al inicio.
            </p>
            <div className="mt-4 flex gap-2">
              <button
                className="px-3 py-2 text-sm rounded-md border"
                onClick={() => window.location.reload()}
              >
                Recargar
              </button>
              <a className="px-3 py-2 text-sm rounded-md border" href="/">
                Ir al inicio
              </a>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}