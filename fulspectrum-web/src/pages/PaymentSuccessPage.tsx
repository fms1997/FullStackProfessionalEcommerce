import { Link, useSearchParams } from "react-router-dom";

export function PaymentSuccessPage() {
  const [params] = useSearchParams();
  const orderId = params.get("orderId") ?? "";

  return (
    <main>
      <h1>✅ Pago confirmado</h1>
      <p>Tu pago fue acreditado. ¡Gracias por tu compra!</p>
      {orderId ? <p>Pedido: {orderId}</p> : null}
      <Link to={orderId ? `/orders/${orderId}` : "/orders"}>Ver pedido</Link>
    </main>
  );
}
