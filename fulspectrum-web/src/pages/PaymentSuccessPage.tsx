import { Link, useSearchParams } from "react-router-dom";
import { sanitizeIdentifier } from "../security/input";
export function PaymentSuccessPage() {
  const [params] = useSearchParams();
  const orderId = sanitizeIdentifier(params.get("orderId") ?? "");
  return (
    <main>
      <h1>✅ Pago confirmado</h1>
      <p>Tu pago fue acreditado. ¡Gracias por tu compra!</p>
      {orderId ? <p>Pedido: {orderId}</p> : null}
      <Link to={orderId ? `/orders/${orderId}` : "/orders"}>Ver pedido</Link>
    </main>
  );
}
