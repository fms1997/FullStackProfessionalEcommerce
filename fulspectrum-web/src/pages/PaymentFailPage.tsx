import { useSearchParams } from "react-router-dom";
import { RetryPaymentButton } from "../components/RetryPaymentButton";

type PaymentFailPageProps = {
  token: string;
};

export function PaymentFailPage({ token }: PaymentFailPageProps) {
  const [params] = useSearchParams();
  const orderId = params.get("orderId") ?? "";
  const provider = (params.get("provider") as "Stripe" | "MercadoPago") ?? "Stripe";
  const reason = params.get("reason") ?? "El proveedor rechazó el pago.";

  return (
    <main>
      <h1>❌ Pago rechazado</h1>
      <p>{reason}</p>
      {orderId ? <RetryPaymentButton orderId={orderId} token={token} provider={provider} /> : <p>No se encontró el pedido.</p>}
    </main>
  );
}
