import { Link } from "react-router-dom";
import { useGetMyOrdersQuery } from "../state/api";

export default function MyOrders() {
  const { data, isLoading, isError } = useGetMyOrdersQuery();

  if (isLoading) return <section className="p-6">Cargando pedidos...</section>;
  if (isError) return <section className="p-6 text-red-700">No se pudieron cargar tus pedidos.</section>;

  return (
    <section className="mx-auto max-w-4xl p-6 space-y-4">
      <h1 className="text-2xl font-semibold">Mis pedidos</h1>
      {!data?.length && <p>Aún no tienes pedidos.</p>}
      <ul className="space-y-3">
        {data?.map((order) => (
          <li key={order.id} className="border rounded p-4 flex items-center justify-between">
            <div>
              <p className="font-medium">#{order.id.slice(0, 8)}</p>
              <p className="text-sm text-gray-600">Estado: {order.status} · {order.totalItems} items</p>
              <p className="text-sm">Total: {order.total.toFixed(2)} {order.currency}</p>
            </div>
            <Link className="underline" to={`/orders/${order.id}`}>
              Ver detalle
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}
