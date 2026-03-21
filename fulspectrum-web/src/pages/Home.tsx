import { useSearchParams } from "react-router-dom";
import { memo, useEffect, useMemo, useState } from "react";

import { setForbidden } from "../state/authSlice";
import { useAppDispatch, useAppSelector } from "../state/hooks";
import type { ProductDto } from "../types/api";
import {
  useAddCartItemMutation,
  useDeleteProductMutation,
  useGetCartQuery,
  useGetProductsQuery,
  useRemoveCartItemMutation,
  useUpdateCartItemMutation,
} from "../state/api";
import {
  addLocalItem,
  hydrateServerCart,
  removeLocalItem,
  updateLocalItem,
} from "../state/cartSlice";
import { VirtualizedList } from "../components/VirtualizedList";

const parseBoolean = (value: string | null): boolean | undefined => {
  if (value === "true") return true;
  if (value === "false") return false;
  return undefined;
};

type ProductRowProps = {
  item: ProductDto;
  isAdmin: boolean;
  onAdd: (item: ProductDto) => void;
  onDelete: (id: string) => void;
};

const ProductRow = memo(function ProductRow({
  item,
  isAdmin,
  onAdd,
  onDelete,
}: ProductRowProps) {
  return (
    <div className="border rounded p-3 flex items-center justify-between gap-3 h-[88px]">
      <div className="flex items-center gap-3">
        <img
          src={`https://picsum.photos/seed/${item.sku}/72/72`}
          srcSet={`https://picsum.photos/seed/${item.sku}/72/72 1x, https://picsum.photos/seed/${item.sku}/144/144 2x`}
          sizes="72px"
          width={72}
          height={72}
          loading="lazy"
          decoding="async"
          alt={item.name}
          className="rounded object-cover"
        />

        <div>
          <h2 className="font-medium">{item.name}</h2>
          <p className="text-sm opacity-70">SKU: {item.sku}</p>
        </div>
      </div>

      <div className="text-right space-y-1">
        <p>
          {item.currency} {item.basePrice.toFixed(2)}
        </p>

        <button className="text-xs underline" onClick={() => onAdd(item)}>
          Agregar al carrito
        </button>

        {isAdmin && (
          <button
            className="block text-xs underline"
            onClick={() => onDelete(item.id)}
          >
            Eliminar
          </button>
        )}
      </div>
    </div>
  );
});

export default function Home() {
  const [searchParams, setSearchParams] = useSearchParams();

  const { profile, forbiddenMessage } = useAppSelector((s) => s.auth);
  const { localItems, serverCart } = useAppSelector((s) => s.cart);

  const dispatch = useAppDispatch();

  const [deleteProduct] = useDeleteProductMutation();
  const [addCartItem] = useAddCartItemMutation();
  const [removeCartItem] = useRemoveCartItemMutation();
  const [updateCartItem] = useUpdateCartItemMutation();

  const [pendingQty, setPendingQty] = useState<Record<string, number>>({});

  const query = useMemo(
    () => ({
      search: searchParams.get("search") ?? "",
      isPublished: parseBoolean(searchParams.get("isPublished")),
      sortBy: searchParams.get("sortBy") ?? "createdAt",
      sortDirection:
        (searchParams.get("sortDirection") as "asc" | "desc" | null) ?? "desc",
      pageSize: Number(searchParams.get("pageSize") ?? "50"),
    }),
    [searchParams],
  );

  const { data, isLoading, isError, refetch } = useGetProductsQuery(query);
  const { data: cartData } = useGetCartQuery(undefined, { skip: !profile });

  useEffect(() => {
    if (cartData) {
      dispatch(hydrateServerCart(cartData));
    }
  }, [cartData, dispatch]);

  useEffect(() => {
    if (!profile || !serverCart) return;

    const timers = Object.entries(pendingQty).map(([productId, quantity]) =>
      window.setTimeout(async () => {
        await updateCartItem({
          productId,
          quantity,
          rowVersion: serverCart.rowVersion,
        });

        setPendingQty((prev) => {
          const next = { ...prev };
          delete next[productId];
          return next;
        });
      }, 450),
    );

    return () => {
      timers.forEach((id) => window.clearTimeout(id));
    };
  }, [pendingQty, profile, serverCart, updateCartItem]);

  const updateParam = (key: string, value: string) => {
    const params = new URLSearchParams(searchParams);

    if (!value) {
      params.delete(key);
    } else {
      params.set(key, value);
    }

    if (key !== "page") {
      params.set("page", "1");
    }

    setSearchParams(params);
  };

  const onAdd = async (item: ProductDto) => {
    if (!profile) {
      dispatch(
        addLocalItem({
          productId: item.id,
          name: item.name,
          sku: item.sku,
          unitPrice: item.basePrice,
          quantity: 1,
        }),
      );
      return;
    }

    const result = await addCartItem({
      productId: item.id,
      quantity: 1,
      rowVersion: serverCart?.rowVersion,
    });

    if ("data" in result && result.data) {
      dispatch(hydrateServerCart(result.data));
    }
  };

  const cartItems = useMemo(
    () =>
      profile
        ? (serverCart?.items ?? [])
        : localItems.map((x) => ({
            ...x,
            productName: x.name,
            lineTotal: x.unitPrice * x.quantity,
          })),
    [localItems, profile, serverCart?.items],
  );

  return (
    <div className="p-6 space-y-4">
      {forbiddenMessage && (
        <div className="border border-amber-300 bg-amber-50 rounded p-3 text-sm">
          {forbiddenMessage}
          <button
            className="ml-3 underline"
            onClick={() => dispatch(setForbidden(null))}
          >
            Cerrar
          </button>
        </div>
      )}

      <div className="grid gap-2 sm:grid-cols-4">
        <input
          className="border rounded px-2 py-1"
          placeholder="Buscar por nombre, SKU o slug"
          value={query.search}
          onChange={(e) => updateParam("search", e.target.value)}
        />

        <select
          className="border rounded px-2 py-1"
          value={searchParams.get("isPublished") ?? ""}
          onChange={(e) => updateParam("isPublished", e.target.value)}
        >
          <option value="">Todos</option>
          <option value="true">Publicados</option>
          <option value="false">Borrador</option>
        </select>

        <select
          className="border rounded px-2 py-1"
          value={query.sortBy}
          onChange={(e) => updateParam("sortBy", e.target.value)}
        >
          <option value="createdAt">Más recientes</option>
          <option value="name">Nombre</option>
          <option value="price">Precio</option>
          <option value="sku">SKU</option>
        </select>

        <select
          className="border rounded px-2 py-1"
          value={query.sortDirection}
          onChange={(e) => updateParam("sortDirection", e.target.value)}
        >
          <option value="desc">Descendente</option>
          <option value="asc">Ascendente</option>
        </select>
      </div>

      {isLoading && <p>Cargando productos...</p>}

      {isError && (
        <div className="border border-red-300 bg-red-50 rounded p-3">
          <p className="text-red-700">No se pudo cargar el catálogo.</p>
          <button
            className="mt-2 border rounded px-2 py-1"
            onClick={() => refetch()}
          >
            Reintentar
          </button>
        </div>
      )}

      {!isLoading && !isError && data && data.items.length > 0 && (
        <VirtualizedList
          items={data.items}
          height={420}
          itemHeight={96}
          renderItem={(item) => (
            <ProductRow
              item={item}
              isAdmin={profile?.role === "Admin"}
              onAdd={onAdd}
              onDelete={(id) => deleteProduct(id)}
            />
          )}
        />
      )}

      <section className="border rounded p-4">
        <h2 className="font-semibold mb-2">Carrito</h2>

        {cartItems.length === 0 && (
          <p className="text-sm opacity-70">Tu carrito está vacío.</p>
        )}

        {cartItems.map((item) => (
          <div
            key={item.productId}
            className="flex items-center justify-between py-2 border-b last:border-b-0"
          >
            <div>
              <p className="font-medium">{item.productName}</p>
              <p className="text-xs opacity-70">{item.sku}</p>
            </div>

            <div className="flex items-center gap-2">
              <input
                type="number"
                className="w-16 border rounded px-2 py-1"
                min={1}
                value={
                  profile
                    ? (pendingQty[item.productId] ?? item.quantity)
                    : item.quantity
                }
                onChange={(e) => {
                  const next = Math.max(1, Number(e.target.value) || 1);

                  if (!profile) {
                    dispatch(
                      updateLocalItem({
                        productId: item.productId,
                        quantity: next,
                      }),
                    );
                    return;
                  }

                  setPendingQty((prev) => ({
                    ...prev,
                    [item.productId]: next,
                  }));
                }}
              />

              <button
                className="text-xs underline"
                onClick={() => {
                  if (!profile) {
                    dispatch(removeLocalItem(item.productId));
                    return;
                  }

                  removeCartItem({
                    productId: item.productId,
                    rowVersion: serverCart?.rowVersion,
                  });
                }}
              >
                Quitar
              </button>
            </div>
          </div>
        ))}
      </section>

      {!isLoading && !isError && data && (
        <p className="text-sm opacity-70">
          Mostrando {data.items.length} de {data.totalCount} productos
        </p>
      )}
    </div>
  );
}