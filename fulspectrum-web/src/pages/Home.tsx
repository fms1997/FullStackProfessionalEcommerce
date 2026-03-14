import { useSearchParams } from "react-router-dom";
import { useDeleteProductMutation } from "../state/api";
import { setForbidden } from "../state/authSlice";
import { useAppDispatch, useAppSelector } from "../state/hooks";
import type { ProductDto } from "../types/api";
import { useEffect, useMemo, useState } from "react";
import {
  useAddCartItemMutation,
  useGetCartQuery,
  useGetProductsQuery,
  useRemoveCartItemMutation,
  useUpdateCartItemMutation,
} from "../state/api";
// import { addLocalItem, hydrateServerCart } from "../state/cartSlice";
import { addLocalItem, hydrateServerCart, removeLocalItem, updateLocalItem } from "../state/cartSlice";
const parseBoolean = (value: string | null): boolean | undefined => {
  if (value === "true") return true;
  if (value === "false") return false;
  return undefined;
};

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

  const query = useMemo(() => {
    return {
      search: searchParams.get("search") ?? "",
      isPublished: parseBoolean(searchParams.get("isPublished")),
      sortBy: searchParams.get("sortBy") ?? "createdAt",
      sortDirection:
        (searchParams.get("sortDirection") as "asc" | "desc" | null) ?? "desc",
      pageSize: Number(searchParams.get("pageSize") ?? "10"),
    };
  }, [searchParams]);
  const { data, isLoading, isError, isFetching, refetch } =
    useGetProductsQuery(query);
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

    return () => timers.forEach((id) => window.clearTimeout(id));
  }, [pendingQty, profile, serverCart, updateCartItem]);
  const updateParam = (key: string, value: string) => {
    const params = new URLSearchParams(searchParams);

    if (!value) params.delete(key);
    else params.set(key, value);

    if (key !== "page") params.set("page", "1");

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

    const optimistic = {
      ...(serverCart ?? {
        id: "",
        userId: "",
        rowVersion: "",
        items: [],
        totalItems: 0,
        subtotal: 0,
        currency: item.currency,
      }),
      items: [
        ...(serverCart?.items ?? []).filter((x) => x.productId !== item.id),
        {
          id: `tmp-${item.id}`,
          productId: item.id,
          productName: item.name,
          sku: item.sku,
          unitPrice: item.basePrice,
          quantity:
            (serverCart?.items.find((x) => x.productId === item.id)?.quantity ??
              0) + 1,
          maxAllowedQuantity: 10,
          availableStock: 999,
          lineTotal: item.basePrice,
        },
      ],
      totalItems: (serverCart?.totalItems ?? 0) + 1,
      subtotal: (serverCart?.subtotal ?? 0) + item.basePrice,
    };

    dispatch(hydrateServerCart(optimistic));
    const result = await addCartItem({
      productId: item.id,
      quantity: 1,
      rowVersion: serverCart?.rowVersion,
    });
    if ("data" in result && result.data) {
      dispatch(hydrateServerCart(result.data));
    }
  };

  const cartItems = profile
    ? (serverCart?.items ?? [])
    : localItems.map((x) => ({
        ...x,
        productName: x.name,
        lineTotal: x.unitPrice * x.quantity,
      }));
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
          </button>{" "}
        </div>
      )}

      <div className="grid gap-2 sm:grid-cols-4">
        <input
          className="border rounded px-2 py-1"
          placeholder="Buscar por nombre, SKU o slug"
          value={query.search}
          onChange={(e) => updateParam("search", e.target.value)}
        />{" "}
        <select
          className="border rounded px-2 py-1"
          value={searchParams.get("isPublished") ?? ""}
          onChange={(e) => updateParam("isPublished", e.target.value)}
        >
          <option value="">Todos</option>
          <option value="true">Publicados</option>
          <option value="false">Borrador</option>{" "}
        </select>
        <select
          className="border rounded px-2 py-1"
          value={query.sortBy}
          onChange={(e) => updateParam("sortBy", e.target.value)}
        >
          <option value="createdAt">Más recientes</option>
          <option value="name">Nombre</option>
          <option value="price">Precio</option>
          <option value="sku">SKU</option>{" "}
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
        <ul className="space-y-2">
          {data.items.map((item: ProductDto) => (
            <li
              key={item.id}
              className="border rounded p-3 flex items-center justify-between gap-3"
            >
              <div>
                <h2 className="font-medium">{item.name}</h2>
                <p className="text-sm opacity-70">SKU: {item.sku}</p>
              </div>
              <div className="text-right space-y-1">
                <p>
                  {item.currency} {item.basePrice.toFixed(2)}
                </p>
                <button
                  className="text-xs underline"
                  onClick={() => onAdd(item)}
                >
                  Agregar al carrito
                </button>
                {profile?.role === "Admin" && (
                  <button
                    className="block text-xs underline"
                    onClick={() => deleteProduct(item.id)}
                  >
                    Eliminar
                  </button>
                )}
              </div>
            </li>
          ))}
        </ul>
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
                min={0}
                value={
                  profile
                    ? (pendingQty[item.productId] ?? item.quantity)
                    : item.quantity
                }
                onChange={(e) => {
                  const qty = Number(e.target.value);
                  if (!profile)
                    dispatch(
                      updateLocalItem({
                        productId: item.productId,
                        quantity: qty,
                      }),
                    );
                  else
                    setPendingQty((prev) => ({
                      ...prev,
                      [item.productId]: qty,
                    }));
                }}
              />
              <button
                className="text-xs underline"
                onClick={() => {
                  if (!profile) dispatch(removeLocalItem(item.productId));
                  else
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
                min={0}
                value={
                  profile
                    ? (pendingQty[item.productId] ?? item.quantity)
                    : item.quantity
                }
                onChange={(e) => {
                  const qty = Number(e.target.value);
                  if (!profile)
                    dispatch(
                      updateLocalItem({
                        productId: item.productId,
                        quantity: qty,
                      }),
                    );
                  else
                    setPendingQty((prev) => ({
                      ...prev,
                      [item.productId]: qty,
                    }));
                }}
              />
              <button
                className="text-xs underline"
                onClick={() => {
                  if (!profile) dispatch(removeLocalItem(item.productId));
                  else
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
                min={0}
                value={
                  profile
                    ? (pendingQty[item.productId] ?? item.quantity)
                    : item.quantity
                }
                onChange={(e) => {
                  const qty = Number(e.target.value);
                  if (!profile)
                    dispatch(
                      updateLocalItem({
                        productId: item.productId,
                        quantity: qty,
                      }),
                    );
                  else
                    setPendingQty((prev) => ({
                      ...prev,
                      [item.productId]: qty,
                    }));
                }}
              />
              <button
                className="text-xs underline"
                onClick={() => {
                  if (!profile) dispatch(removeLocalItem(item.productId));
                  else
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
                min={0}
                value={
                  profile
                    ? (pendingQty[item.productId] ?? item.quantity)
                    : item.quantity
                }
                onChange={(e) => {
                  const qty = Number(e.target.value);
                  if (!profile)
                    dispatch(
                      updateLocalItem({
                        productId: item.productId,
                        quantity: qty,
                      }),
                    );
                  else
                    setPendingQty((prev) => ({
                      ...prev,
                      [item.productId]: qty,
                    }));
                }}
              />
              <button
                className="text-xs underline"
                onClick={() => {
                  if (!profile) dispatch(removeLocalItem(item.productId));
                  else
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
          Mostrando {data.items.length} de {data.totalCount} productos{" "}
          {isFetching ? "(actualizando...)" : ""}
        </p>
      )}
    </div>
  );
}
