# Etapa 2 — API profesional (CRUD + filtros + paginación + sorting)

Esta guía está pensada para **probar paso a paso** que la Etapa 2 está completa en backend y frontend.

---

## 0) Preparación

1. Levanta backend (ajusta ruta/proyecto si cambia):

```bash
dotnet run --project FulSpectrum/src/FulSpectrum.Api
```

2. Levanta frontend:

```bash
cd fulspectrum-web
npm run dev
```

3. Ten a mano:
- Swagger: `http://localhost:5000/swagger` (o el puerto que indique tu API)
- Frontend: `http://localhost:5173`
- Health checks (si existen): `/health/live`, `/health/ready`

---

## 1) Backend — REST completo (CRUD)

> Recurso ejemplo: `products` (adáptalo al recurso real: users, categories, etc.).

## 1.1 Crear (POST)

```bash
curl -i -X POST "http://localhost:5000/api/v1/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name":"Mouse Pro",
    "price":49.99,
    "stock":120,
    "category":"Perifericos"
  }'
```

✅ Esperado:
- `201 Created`
- `Location` header con `/api/v1/products/{id}`
- Body con el objeto creado.

## 1.2 Obtener por id (GET)

```bash
curl -i "http://localhost:5000/api/v1/products/{id}"
```

✅ Esperado:
- `200 OK` si existe.
- `404 Not Found` si no existe.

## 1.3 Listar (GET)

```bash
curl -i "http://localhost:5000/api/v1/products"
```

✅ Esperado:
- `200 OK`
- colección en body.

## 1.4 Actualizar (PUT/PATCH)

```bash
curl -i -X PUT "http://localhost:5000/api/v1/products/{id}" \
  -H "Content-Type: application/json" \
  -d '{
    "name":"Mouse Pro X",
    "price":59.99,
    "stock":100,
    "category":"Perifericos"
  }'
```

✅ Esperado:
- `200 OK` o `204 No Content`.
- GET posterior refleja cambios.

## 1.5 Eliminar (DELETE)

```bash
curl -i -X DELETE "http://localhost:5000/api/v1/products/{id}"
```

✅ Esperado:
- `204 No Content`.
- GET posterior devuelve `404`.

---

## 2) Backend — Paginación real

Prueba con varios registros (20+):

```bash
curl -i "http://localhost:5000/api/v1/products?page=1&pageSize=10"
curl -i "http://localhost:5000/api/v1/products?page=2&pageSize=10"
```

✅ Esperado:
- Cambian los items entre página 1 y 2.
- `page`, `pageSize`, `total` (o metadatos equivalentes) coherentes.
- Límite de `pageSize` respetado (si max=100, pedir 999 debe capearse o validar).

Casos borde:

```bash
curl -i "http://localhost:5000/api/v1/products?page=0&pageSize=10"
curl -i "http://localhost:5000/api/v1/products?page=1&pageSize=0"
```

✅ Esperado:
- `400 Bad Request` con `problem+json` si valores inválidos.

---

## 3) Backend — Filtros dinámicos

Ejemplos:

```bash
curl -i "http://localhost:5000/api/v1/products?category=Perifericos"
curl -i "http://localhost:5000/api/v1/products?minPrice=30&maxPrice=80"
curl -i "http://localhost:5000/api/v1/products?search=mouse"
```

✅ Esperado:
- Solo devuelve registros que cumplen filtros.
- Combinaciones AND correctas (`category + minPrice + search`).
- Si filtro inválido, responde `400` con detalle.

---

## 4) Backend — Sorting

```bash
curl -i "http://localhost:5000/api/v1/products?sortBy=price&sortDir=asc"
curl -i "http://localhost:5000/api/v1/products?sortBy=price&sortDir=desc"
```

✅ Esperado:
- Orden cambia correctamente.
- Campo no permitido (`sortBy=hackerField`) => `400` o fallback explícito documentado.

---

## 5) Backend — DTOs y mapping

Checklist:
- El request DTO **no** expone campos internos (ej. `createdAt`, `internalCost`, `rowVersion`).
- El response DTO devuelve solo contrato público.
- No se exponen entidades de dominio directamente.

Prueba práctica:
1. Envía campos extra en POST.
2. Verifica que se ignoran/rechazan según diseño.
3. Revisa shape de respuesta en Swagger.

✅ Esperado:
- Contrato estable y consistente entre endpoints.

---

## 6) Backend — FluentValidation

Forzar errores de validación:

```bash
curl -i -X POST "http://localhost:5000/api/v1/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name":"",
    "price":-10,
    "stock":-1
  }'
```

✅ Esperado:
- `400 Bad Request`
- `application/problem+json`
- Mensajes por campo (`name`, `price`, `stock`).

---

## 7) Backend — Rate limiting básico

Lanza múltiples requests rápido:

```bash
for i in {1..30}; do
  curl -s -o /dev/null -w "%{http_code}\n" "http://localhost:5000/api/v1/products";
done
```

✅ Esperado:
- Al superar umbral, aparecen `429 Too Many Requests`.
- Si configurado, headers tipo `Retry-After`.

---

## 8) Frontend — RTK Query (cache, invalidación, re-fetch)

## 8.1 Cache en listado
1. Entra al listado (ej. `/products`).
2. Abre DevTools Network.
3. Navega a otra ruta y vuelve.

✅ Esperado:
- No siempre re-dispara request inmediato (depende de `keepUnusedDataFor` y políticas).
- Usa cache al volver en ventana corta.

## 8.2 Invalidación
1. Crea o edita un item desde UI.
2. Vuelve al listado.

✅ Esperado:
- Se refresca automáticamente (invalidación por tags).
- El item nuevo/editado aparece sin recargar manual.

## 8.3 Re-fetch
Prueba:
- botón manual de reintento/refresh.
- `refetchOnFocus` (cambia de pestaña y vuelve).
- `refetchOnReconnect` (simula offline/online).

✅ Esperado:
- La data se sincroniza según configuración.

---

## 9) Frontend — Filtros en querystring

1. Aplica filtros en UI (search, category, sort, page).
2. Revisa URL (ejemplo):

`/products?search=mouse&category=Perifericos&sortBy=price&sortDir=asc&page=2&pageSize=10`

✅ Esperado:
- URL refleja el estado.
- Al recargar página se conserva estado desde querystring.
- Botones back/forward mantienen filtros correctamente.

---

## 10) Frontend — UI states (loading / empty / error / retry)

## 10.1 Loading
- Simula red lenta (DevTools throttling).

✅ Esperado:
- skeleton/spinner visible y sin layout shift brusco.

## 10.2 Empty
- Aplica filtros que no retornen resultados.

✅ Esperado:
- mensaje “sin resultados” claro + acción sugerida (limpiar filtros).

## 10.3 Error
- Rompe endpoint temporalmente o usa URL incorrecta en `.env`.

✅ Esperado:
- estado de error visible y entendible.
- no rompe toda la app.

## 10.4 Retry
- Usa botón “Reintentar”.

✅ Esperado:
- dispara nueva request y recupera estado si backend vuelve.

---

## 11) Criterio de salida (Definition of Done)

La Etapa 2 está lista si:

- CRUD completo y consistente en API.
- Paginación/filtros/sorting funcionando juntos.
- DTOs y mapping protegen el contrato.
- Validaciones de FluentValidation devuelven errores claros.
- Rate limiting responde 429 al exceder límite.
- Frontend con RTK Query cachea, invalida y hace re-fetch según configuración.
- Filtros persisten en querystring.
- Estados de UI (loading, empty, error, retry) están implementados y probados.

---

## 12) Plantilla de reporte de prueba (rápida)

```txt
[Etapa 2 - Resultado]
Fecha:
Commit:

Backend
- CRUD: OK/FAIL (detalle)
- Paginación: OK/FAIL
- Filtros: OK/FAIL
- Sorting: OK/FAIL
- DTO/mapping: OK/FAIL
- FluentValidation: OK/FAIL
- Rate limiting: OK/FAIL

Frontend
- RTK Query cache: OK/FAIL
- Invalidación: OK/FAIL
- Re-fetch: OK/FAIL
- Querystring filtros: OK/FAIL
- UI states: OK/FAIL

Bloqueantes:
- ...
```
