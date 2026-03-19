import { useMemo, useState } from "react";
import { z } from "zod";
import {
  useBulkDeleteProductsMutation,
  useBulkPublishProductsMutation,
  useCreateAdminProductMutation,
  useGetAdminOrdersQuery,
  useGetAdminProductsQuery,
  useUpdateAdminOrderStatusMutation,
  useUpdateVariantStockMutation,
  useUploadAdminImageMutation,
} from "../state/api";

const productSchema = z.object({
  categoryId: z.string().uuid("CategoryId inválido"),
  name: z.string().min(3),
  slug: z.string().min(3),
  sku: z.string().min(3),
  basePrice: z.number().nonnegative(),
  currency: z.string().length(3),
  isPublished: z.boolean(),
  variants: z.array(
    z.object({
      variantSku: z.string().min(2),
      name: z.string().min(2),
      priceDelta: z.number().nonnegative(),
      isDefault: z.boolean(),
      quantityOnHand: z.number().int().nonnegative(),
      reservedQuantity: z.number().int().nonnegative(),
      reorderThreshold: z.number().int().nonnegative(),
    }),
  ).min(1),
});

const defaultProduct = {
  categoryId: "",
  name: "",
  slug: "",
  sku: "",
  basePrice: 0,
  currency: "USD",
  isPublished: false,
  variants: [{ variantSku: "", name: "", priceDelta: 0, isDefault: true, quantityOnHand: 0, reservedQuantity: 0, reorderThreshold: 0 }],
};

export default function AdminPanel() {
  const [search, setSearch] = useState("");
  const [selected, setSelected] = useState<string[]>([]);
  const [form, setForm] = useState(defaultProduct);
  const [statusFilter, setStatusFilter] = useState("");
  const [imageUrl, setImageUrl] = useState("");

  const products = useGetAdminProductsQuery({ search });
  const orders = useGetAdminOrdersQuery({ status: statusFilter });

  const [createProduct, createState] = useCreateAdminProductMutation();
  const [bulkPublish] = useBulkPublishProductsMutation();
  const [bulkDelete] = useBulkDeleteProductsMutation();
  const [updateStock] = useUpdateVariantStockMutation();
  const [uploadImage] = useUploadAdminImageMutation();
  const [updateOrderStatus] = useUpdateAdminOrderStatusMutation();

  const allSelected = useMemo(() => products.data?.length && selected.length === products.data.length, [products.data, selected.length]);

  const submit = async () => {
    const parsed = productSchema.safeParse(form);
    if (!parsed.success) {
      alert(parsed.error.issues[0]?.message ?? "Formulario inválido");
      return;
    }

    await createProduct(parsed.data).unwrap();
    setForm(defaultProduct);
  };

  return (
    <section className="p-6 space-y-8">
      <h1 className="text-2xl font-semibold">Admin Panel</h1>

      <article className="border rounded p-4 space-y-3">
        <h2 className="font-semibold">Catálogo (filtros + bulk actions)</h2>
        <div className="flex gap-2">
          <input className="border rounded px-2 py-1" placeholder="Buscar" value={search} onChange={(e) => setSearch(e.target.value)} />
          <button className="border rounded px-2" onClick={() => bulkPublish({ productIds: selected, isPublished: true })}>Publicar</button>
          <button className="border rounded px-2" onClick={() => bulkPublish({ productIds: selected, isPublished: false })}>Despublicar</button>
          <button className="border rounded px-2" onClick={() => bulkDelete({ productIds: selected })}>Eliminar</button>
        </div>

        <table className="w-full text-sm border-collapse">
          <thead>
            <tr className="border-b">
              <th><input type="checkbox" checked={Boolean(allSelected)} onChange={(e) => setSelected(e.target.checked ? (products.data ?? []).map((p) => p.id) : [])} /></th>
              <th>Nombre</th><th>SKU</th><th>Estado</th><th>Variantes</th>
            </tr>
          </thead>
          <tbody>
            {(products.data ?? []).map((p) => (
              <tr key={p.id} className="border-b">
                <td><input type="checkbox" checked={selected.includes(p.id)} onChange={(e) => setSelected((prev) => e.target.checked ? [...prev, p.id] : prev.filter((x) => x !== p.id))} /></td>
                <td>{p.name}</td>
                <td>{p.sku}</td>
                <td>{p.isPublished ? "Publicado" : "Borrador"}</td>
                <td>
                  {p.variants.map((v) => (
                    <div key={v.id} className="flex items-center gap-2">
                      <span>{v.name} ({v.quantityOnHand})</span>
                      <button className="border rounded px-1" onClick={() => updateStock({ variantId: v.id, quantityOnHand: v.quantityOnHand + 1, reservedQuantity: v.reservedQuantity, reorderThreshold: v.reorderThreshold })}>+1 stock</button>
                    </div>
                  ))}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </article>

      <article className="border rounded p-4 space-y-3">
        <h2 className="font-semibold">Crear producto (form complejo con Zod)</h2>
        <div className="grid grid-cols-2 gap-2">
          <input className="border rounded px-2 py-1" placeholder="CategoryId (uuid)" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })} />
          <input className="border rounded px-2 py-1" placeholder="Nombre" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          <input className="border rounded px-2 py-1" placeholder="Slug" value={form.slug} onChange={(e) => setForm({ ...form, slug: e.target.value })} />
          <input className="border rounded px-2 py-1" placeholder="SKU" value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} />
          <input className="border rounded px-2 py-1" type="number" placeholder="Base price" value={form.basePrice} onChange={(e) => setForm({ ...form, basePrice: Number(e.target.value) })} />
          <input className="border rounded px-2 py-1" placeholder="Currency" value={form.currency} onChange={(e) => setForm({ ...form, currency: e.target.value })} />
        </div>
        <label className="flex items-center gap-2"><input type="checkbox" checked={form.isPublished} onChange={(e) => setForm({ ...form, isPublished: e.target.checked })} /> Publicado</label>

        <div className="space-y-2">
          {form.variants.map((v, idx) => (
            <div key={idx} className="grid grid-cols-4 gap-2 border rounded p-2">
              <input className="border rounded px-2 py-1" placeholder="Variant SKU" value={v.variantSku} onChange={(e) => setForm({ ...form, variants: form.variants.map((x, i) => i === idx ? { ...x, variantSku: e.target.value } : x) })} />
              <input className="border rounded px-2 py-1" placeholder="Nombre variante" value={v.name} onChange={(e) => setForm({ ...form, variants: form.variants.map((x, i) => i === idx ? { ...x, name: e.target.value } : x) })} />
              <input className="border rounded px-2 py-1" type="number" placeholder="Price delta" value={v.priceDelta} onChange={(e) => setForm({ ...form, variants: form.variants.map((x, i) => i === idx ? { ...x, priceDelta: Number(e.target.value) } : x) })} />
              <input className="border rounded px-2 py-1" type="number" placeholder="Stock" value={v.quantityOnHand} onChange={(e) => setForm({ ...form, variants: form.variants.map((x, i) => i === idx ? { ...x, quantityOnHand: Number(e.target.value) } : x) })} />
            </div>
          ))}
          <button className="border rounded px-2 py-1" onClick={() => setForm({ ...form, variants: [...form.variants, { variantSku: "", name: "", priceDelta: 0, isDefault: false, quantityOnHand: 0, reservedQuantity: 0, reorderThreshold: 0 }] })}>Agregar variante</button>
        </div>

        <div className="flex items-center gap-2">
          <input type="file" onChange={async (e) => {
            const file = e.target.files?.[0];
            if (!file) return;
            const response = await uploadImage(file).unwrap();
            setImageUrl(response.url);
          }} />
          {imageUrl && <a className="underline" href={imageUrl} target="_blank">Imagen subida</a>}
        </div>

        <button className="border rounded px-3 py-1" onClick={submit} disabled={createState.isLoading}>Guardar</button>
      </article>

      <article className="border rounded p-4 space-y-3">
        <h2 className="font-semibold">Órdenes (gestión de estado)</h2>
        <select className="border rounded px-2 py-1" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          <option value="">Todos</option>
          <option value="PendingPayment">PendingPayment</option>
          <option value="Paid">Paid</option>
          <option value="Processing">Processing</option>
          <option value="Shipped">Shipped</option>
          <option value="Completed">Completed</option>
        </select>
        <table className="w-full text-sm border-collapse">
          <thead><tr className="border-b"><th>ID</th><th>Estado</th><th>Total</th><th>Acción</th></tr></thead>
          <tbody>
            {(orders.data ?? []).map((o) => (
              <tr key={o.id} className="border-b">
                <td>{o.id.slice(0, 8)}</td>
                <td>{o.status}</td>
                <td>{o.total.toFixed(2)} {o.currency}</td>
                <td><button className="border rounded px-2" onClick={() => updateOrderStatus({ id: o.id, status: "Processing" })}>Mover a Processing</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </article>
    </section>
  );
}
