import { useSearchParams } from "react-router-dom";
import { RetryPaymentButton } from "../components/RetryPaymentButton";
import { useSelector } from "react-redux";
import type { RootState } from "../state/store";


export function PaymentFailPage() {
  const [params] = useSearchParams();
  const token = useSelector((state: RootState) => state.auth.accessToken);
  const orderId = params.get("orderId") ?? "";
  const provider = (params.get("provider") as "Stripe" | "MercadoPago") ?? "Stripe";
  const reason = params.get("reason") ?? "El proveedor rechazó el pago.";

  return (
    <main>
      <h1>❌ Pago rechazado</h1>
      <p>{reason}</p>
 {!token ? <p role="alert">No hay sesión activa. Inicia sesión para reintentar.</p> : null}
      {!orderId ? <p>No se encontró el pedido.</p> : null}
      {orderId && token ? <RetryPaymentButton orderId={orderId} token={token} provider={provider} /> : null}
    </main>
  );
}
