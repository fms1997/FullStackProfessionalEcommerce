import { useMemo } from "react";
import { Link, useParams } from "react-router-dom";
import { useGetOrderDetailQuery, useGetOrderTrackingQuery } from "../state/api";

export default function OrderDetail() {
  const { orderId = "" } = useParams();
  const order = useGetOrderDetailQuery(orderId, { skip: !orderId, pollingInterval: 10000 });
  const tracking = useGetOrderTrackingQuery(orderId, { skip: !orderId, pollingInterval: 10000 });
  const notification = tracking.data ? `Estado actualizado: ${tracking.data.currentStatus}` : null;

  const steps = useMemo(() => tracking.data?.steps ?? [], [tracking.data]);

  if (order.isLoading) return <section className="p-6">Cargando detalle...</section>;
  if (order.isError || !order.data) return <section className="p-6 text-red-700">No se encontró la orden.</section>;

  return (
    <section className="mx-auto max-w-4xl p-6 space-y-4">
      <Link className="underline text-sm" to="/orders">← Volver a mis pedidos</Link>
      <h1 className="text-2xl font-semibold">Detalle de orden #{order.data.id.slice(0, 8)}</h1>

      {notification && (
        <div className="rounded border border-emerald-200 bg-emerald-50 p-2 text-sm" role="status" aria-live="polite">
          {notification}
        </div>
      )}

      <div className="grid gap-4 md:grid-cols-2">
        <article className="border rounded p-4 space-y-2">
          <h2 className="font-medium">Resumen</h2>
          <p>Estado: <strong>{order.data.status}</strong></p>
          <p>Total: {order.data.total.toFixed(2)} {order.data.currency}</p>
          <p>Creada: {new Date(order.data.createdAtUtc).toLocaleString()}</p>
        </article>

        <article className="border rounded p-4 space-y-2">
          <h2 className="font-medium">Tracking</h2>
          <ul className="space-y-2 text-sm">
            {steps.map((step) => (
              <li key={step.status} className={step.isCompleted ? "text-emerald-700" : "text-gray-500"}>
                {step.isCompleted ? "✅" : "⏳"} {step.label}
              </li>
            ))}
          </ul>
        </article>
      </div>

      <article className="border rounded p-4 space-y-2">
        <h2 className="font-medium">Items</h2>
        <ul className="space-y-2 text-sm">
          {order.data.items.map((item) => (
            <li key={item.productId} className="flex justify-between">
              <span>{item.productName} x {item.quantity}</span>
              <strong>{item.lineTotal.toFixed(2)} {order.data.currency}</strong>
            </li>
          ))}
        </ul>
      </article>
    </section>
  );
}
