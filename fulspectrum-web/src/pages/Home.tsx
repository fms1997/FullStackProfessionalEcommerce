import { useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { useGetProductsQuery } from "../state/api";
import type { ProductDto } from "../types/api";

const parseBoolean = (value: string | null): boolean | undefined => {
  if (value === "true") return true;
  if (value === "false") return false;
  return undefined;
};

export default function Home() {
  const [searchParams, setSearchParams] = useSearchParams();

  const query = useMemo(() => {
    return {
      search: searchParams.get("search") ?? "",
      isPublished: parseBoolean(searchParams.get("isPublished")),
      sortBy: searchParams.get("sortBy") ?? "createdAt",
      sortDirection: searchParams.get("sortDirection") ?? "desc",
      page: Number(searchParams.get("page") ?? "1"),
      pageSize: Number(searchParams.get("pageSize") ?? "10"),
    };
  }, [searchParams]);

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

  const { data, isLoading, isError, isFetching, refetch } = useGetProductsQuery(query);

  return (
    <div className="p-6 space-y-4">
      <h1 className="text-xl font-semibold">Catálogo de productos</h1>

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
          <button className="mt-2 border rounded px-2 py-1" onClick={() => refetch()}>
            Reintentar
          </button>
        </div>
      )}

      {!isLoading && !isError && data && data.items.length === 0 && (
        <div className="border rounded p-3">No hay resultados para los filtros actuales.</div>
      )}

      {!isLoading && !isError && data && data.items.length > 0 && (
        <>
          <p className="text-sm opacity-70">
            Mostrando {data.items.length} de {data.totalCount} productos{" "}
            {isFetching ? "(actualizando...)" : ""}
          </p>

          <ul className="space-y-2">
            {data.items.map((item: ProductDto) => (
              <li key={item.id} className="border rounded p-3 flex items-center justify-between">
                <div>
                  <h2 className="font-medium">{item.name}</h2>
                  <p className="text-sm opacity-70">SKU: {item.sku}</p>
                </div>
                <div className="text-right">
                  <p>
                    {item.currency} {item.basePrice.toFixed(2)}
                  </p>
                  <p className="text-xs">{item.isPublished ? "Publicado" : "Borrador"}</p>
                </div>
              </li>
            ))}
          </ul>

          <div className="flex items-center gap-2">
            <button
              className="border rounded px-3 py-1 disabled:opacity-50"
              disabled={query.page <= 1}
              onClick={() => updateParam("page", String(query.page - 1))}
            >
              Anterior
            </button>

            <span className="text-sm">
              Página {data.page} de {data.totalPages || 1}
            </span>

            <button
              className="border rounded px-3 py-1 disabled:opacity-50"
              disabled={data.page >= (data.totalPages || 1)}
              onClick={() => updateParam("page", String(query.page + 1))}
            >
              Siguiente
            </button>
          </div>
        </>
      )}
    </div>
  );
}