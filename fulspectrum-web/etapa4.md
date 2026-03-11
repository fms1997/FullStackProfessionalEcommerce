# Etapa 4 — Carrito avanzado + sincronización (paso a paso)

Esta guía sirve para probar de punta a punta:

- **Backend:** carrito persistente, endpoints completos, reglas de negocio, concurrencia básica con row version.
- **Frontend:** carrito anónimo, merge al loguear, optimistic UI y debounce de updates.

---

## 0) Prerrequisitos

- Backend corriendo (`FulSpectrum.Api`).
- Frontend corriendo (`fulspectrum-web`).
- Base de datos accesible.
- `curl` instalado.

> Variables usadas en ejemplos:

```bash
API="http://localhost:5000"
WEB="http://localhost:5173"
```

---

## 1) Levantar servicios

### 1.1 Backend

```bash
cd FulSpectrum
dotnet run --project FulSpectrum.Api
```

### 1.2 Frontend

```bash
cd fulspectrum-web
npm install
npm run dev
```

---

## 2) Smoke checks iniciales

```bash
curl -i "$API/health/live"
curl -i "$API/health/ready"
curl -i "$API/api/v1/products"
```

Esperado:

- `200 OK` en health.
- catálogo responde con productos.

---

## 3) Preparar usuario autenticado para pruebas de carrito

### 3.1 Register (si no existe)

```bash
curl -i -c cookies.txt -X POST "$API/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email":"cliente.cart@test.com",
    "password":"Cliente123!",
    "firstName":"Cliente",
    "lastName":"Cart"
  }'
```

Si ya existe, usa login:

```bash
curl -i -c cookies.txt -X POST "$API/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"cliente.cart@test.com","password":"Cliente123!"}'
```

Guarda el `accessToken` en variable:

```bash
ACCESS_TOKEN="<pega_token_aqui>"
```

---

## 4) Backend — endpoints completos de carrito

## 4.1 Obtener carrito (creación implícita)

```bash
curl -i "$API/api/v1/cart" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -b cookies.txt
```

Esperado:

- `200 OK`
- body con `id`, `rowVersion`, `items`.

Guarda valores:

```bash
CART_ROW_VERSION="<row_version_actual>"
PRODUCT_ID="<id_producto_existente>"
```

> Puedes sacar un `PRODUCT_ID` desde:

```bash
curl -s "$API/api/v1/products" | jq -r '.items[0].id'
```

## 4.2 Agregar item al carrito

```bash
curl -i -X POST "$API/api/v1/cart/items" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d "{\"productId\":\"$PRODUCT_ID\",\"quantity\":1,\"rowVersion\":\"$CART_ROW_VERSION\"}"
```

Esperado:

- `200 OK`
- item agregado
- `rowVersion` nuevo.

## 4.3 Actualizar cantidad

```bash
NEW_ROW_VERSION="<row_version_nuevo>"

curl -i -X PUT "$API/api/v1/cart/items/$PRODUCT_ID" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d "{\"quantity\":3,\"rowVersion\":\"$NEW_ROW_VERSION\"}"
```

Esperado:

- `200 OK`
- cantidad actualizada.

## 4.4 Quitar item

```bash
ROW_VERSION_2="<row_version_actual>"

curl -i -X DELETE "$API/api/v1/cart/items/$PRODUCT_ID?rowVersion=$ROW_VERSION_2" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -b cookies.txt
```

Esperado:

- `200 OK`
- item removido.

## 4.5 Merge de carrito anónimo

```bash
ROW_VERSION_3="<row_version_actual>"

curl -i -X POST "$API/api/v1/cart/merge" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d "{
    \"items\": [
      { \"productId\": \"$PRODUCT_ID\", \"quantity\": 2 }
    ],
    \"rowVersion\": \"$ROW_VERSION_3\"
  }"
```

Esperado:

- `200 OK`
- cantidades mergeadas y limitadas por reglas.

## 4.6 Limpiar carrito completo

```bash
ROW_VERSION_4="<row_version_actual>"

curl -i -X DELETE "$API/api/v1/cart?rowVersion=$ROW_VERSION_4" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -b cookies.txt
```

Esperado:

- `200 OK`
- carrito sin items.

---

## 5) Backend — reglas de negocio

### 5.1 Validar cantidad inválida (<= 0)

```bash
curl -i -X POST "$API/api/v1/cart/items" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"productId":"'$PRODUCT_ID'","quantity":0}'
```

Esperado: error de validación (400).

### 5.2 Validar límite por producto

```bash
curl -i -X POST "$API/api/v1/cart/items" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"productId":"'$PRODUCT_ID'","quantity":999}'
```

Esperado:

- no debe permitir superar límite/capacidad de stock.
- respuesta de conflicto o ajuste según lógica implementada.

### 5.3 Validar stock insuficiente

Repite con un producto de stock bajo y cantidad alta.
Esperado: conflicto por stock insuficiente.

---

## 6) Backend — concurrencia básica (row version)

Abrir **dos terminales** (A y B), ambos con mismo usuario y carrito.

1. A pide carrito y guarda `rowVersion = RV1`.
2. B pide carrito y guarda también `RV1`.
3. A hace `POST /cart/items` usando `RV1` (éxito, ahora rowVersion cambia a `RV2`).
4. B intenta `POST /cart/items` usando **RV1 viejo**.

Esperado:

- B debe recibir `409 Conflict` por versión desactualizada.

---

## 7) Frontend — carrito anónimo

1. Abrir la web en incógnito **sin login**.
2. En Home, click en “Agregar al carrito” en 2–3 productos.
3. Recargar página.

Esperado:

- los items siguen presentes (persistencia localStorage).

Verificación opcional (DevTools):

- `Application > Local Storage` y revisar clave del carrito anónimo.

---

## 8) Frontend — merge al loguear

1. Sin login, agrega items al carrito.
2. Inicia sesión con usuario customer.
3. Vuelve a Home.

Esperado:

- el carrito local se fusiona al carrito del servidor.
- el carrito anónimo local queda limpio tras merge exitoso.

---

## 9) Frontend — optimistic UI

1. Ya logueado, click “Agregar al carrito”.
2. Observa la UI inmediatamente.

Esperado:

- la cantidad/lista refleja el cambio **antes** de terminar roundtrip de red.
- luego se reconcilia con respuesta del backend.

---

## 10) Frontend — debounce en updates

1. Ya logueado, cambia cantidad en input rápidamente (ej: 1 → 2 → 3 → 4).
2. En Network, filtra requests a `/api/v1/cart/items/{productId}`.

Esperado:

- no se envía una request por cada tecla.
- se envía update consolidado tras la ventana de debounce.

---

## 11) Checklist final de aceptación

- [ ] Carrito backend persiste por usuario.
- [ ] Endpoints de carrito responden correctamente.
- [ ] Reglas de stock/límites aplican.
- [ ] Concurrencia por row version devuelve conflicto cuando corresponde.
- [ ] Carrito anónimo persiste en frontend.
- [ ] Merge al loguear funciona.
- [ ] Optimistic UI se percibe fluido.
- [ ] Debounce reduce tráfico de updates.

---

## 12) Troubleshooting rápido

- Si `401` en carrito: revisar `ACCESS_TOKEN` y cookie refresh.
- Si `403`: revisar rol y policy (`CustomerOrAdmin`).
- Si no mergea en frontend: revisar request `POST /api/v1/cart/merge` en Network.
- Si no persiste anónimo: validar `localStorage` y ausencia de errores JS.
