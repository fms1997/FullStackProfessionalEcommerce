// import { useEffect, useMemo, useState } from "react";
// import { Link, useSearchParams } from "react-router-dom";
// import { getPaymentStatus, type PaymentStatusResponse } from "../api/payments";

// const POLL_INTERVAL_MS = 3000;
// const POLL_TIMEOUT_MS = 60000;

// type PaymentPendingPageProps = {
//   token: string;
// };

// export function PaymentPendingPage({ token }: PaymentPendingPageProps) {
//   const [params] = useSearchParams();
//   const orderId = params.get("orderId") ?? "";

//   const [status, setStatus] = useState<PaymentStatusResponse | null>(null);
//   const [error, setError] = useState<string | null>(null);

//   const canPoll = useMemo(() => Boolean(orderId), [orderId]);

//   useEffect(() => {
//     if (!canPoll) {
//       return;
//     }

//     let cancelled = false;
//     const start = Date.now();

//     const poll = async () => {
//       if (cancelled) {
//         return;
//       }

//       try {
//         const latest = await getPaymentStatus(orderId, token);
//         if (cancelled) {
//           return;
//         }

//         setStatus(latest);

//         if (latest.status === "Succeeded") {
//           window.location.replace(`/payment/success?orderId=${orderId}`);
//           return;
//         }

//         if (latest.status === "Failed" || latest.status === "Canceled") {
//           window.location.replace(`/payment/fail?orderId=${orderId}&reason=${encodeURIComponent(latest.failureMessage ?? "Pago no completado")}`);
//           return;
//         }

//         if (Date.now() - start >= POLL_TIMEOUT_MS) {
//           setError("El pago sigue pendiente. Puedes intentar nuevamente en unos segundos.");
//           return;
//         }

//         window.setTimeout(poll, POLL_INTERVAL_MS);
//       } catch (e) {
//         if (cancelled) {
//           return;
//         }

//         const message = e instanceof Error ? e.message : "No se pudo consultar el estado.";
//         setError(message);
//       }
//     };

//     void poll();

//     return () => {
//       cancelled = true;
//     };
//   }, [canPoll, orderId, token]);

//   return (
//     <main>
//       <h1>⏳ Pago pendiente</h1>
//       <p>Estamos confirmando tu pago con el proveedor.</p>

//       {status ? <p>Estado actual: {status.status}</p> : <p>Consultando estado...</p>}
//       {error ? <p role="alert">{error}</p> : null}

//       {orderId ? <Link to={`/orders/${orderId}`}>Volver al pedido</Link> : null}
//     </main>
//   );
// }













import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useSelector } from "react-redux";
import type { RootState } from "../state/store";
import { getPaymentStatus, type PaymentStatusResponse } from "../api/payments";

const POLL_INTERVAL_MS = 3000;
const POLL_TIMEOUT_MS = 60000;

export function PaymentPendingPage() {
  const [params] = useSearchParams();
  const navigate = useNavigate();

  const orderId = params.get("orderId") ?? "";

  // token desde Redux
  const token = useSelector((state: RootState) => state.auth.token);

  const [status, setStatus] = useState<PaymentStatusResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  const canPoll = useMemo(() => Boolean(orderId && token), [orderId, token]);

  useEffect(() => {
    if (!canPoll) {
      return;
    }

    let cancelled = false;
    const start = Date.now();

    const poll = async () => {
      if (cancelled) return;

      try {
        const latest = await getPaymentStatus(orderId, token);

        if (cancelled) return;

        setStatus(latest);

        if (latest.status === "Succeeded") {
          navigate(`/payment/success?orderId=${orderId}`, { replace: true });
          return;
        }

        if (latest.status === "Failed" || latest.status === "Canceled") {
          navigate(
            `/payment/fail?orderId=${orderId}&reason=${encodeURIComponent(
              latest.failureMessage ?? "Pago no completado"
            )}`,
            { replace: true }
          );
          return;
        }

        if (Date.now() - start >= POLL_TIMEOUT_MS) {
          setError(
            "El pago sigue pendiente. Puedes intentar nuevamente en unos segundos."
          );
          return;
        }

        setTimeout(poll, POLL_INTERVAL_MS);
      } catch (e) {
        if (cancelled) return;

        const message =
          e instanceof Error
            ? e.message
            : "No se pudo consultar el estado del pago.";

        setError(message);
      }
    };

    poll();

    return () => {
      cancelled = true;
    };
  }, [canPoll, orderId, token, navigate]);

  return (
    <main>
      <h1>⏳ Pago pendiente</h1>

      <p>Estamos confirmando tu pago con el proveedor.</p>

      {status ? (
        <p>Estado actual: {status.status}</p>
      ) : (
        <p>Consultando estado...</p>
      )}

      {error && <p role="alert">{error}</p>}

      {orderId && (
        <Link to={`/orders/${orderId}`}>
          Volver al pedido
        </Link>
      )}
    </main>
  );
}