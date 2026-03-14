## 1) Levantar servicios
## 1.1 Backend
cd FulSpectrum
dotnet run --project FulSpectrum.Api
## 1.2 Frontend
cd fulspectrum-web
npm install
npm run dev
## 2) Smoke checks iniciales
curl -i "$API/health/live"
curl -i "$API/health/ready"
curl -i "$API/api/v1/products"
Esperado:

200 OK en health.

Catálogo responde con productos.

## 3) Preparar autenticación y carrito
## 3.1 Register/Login
Si no existe el usuario de pruebas:

curl -i -c cookies.txt -X POST "$API/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email":"cliente.checkout@test.com",
    "password":"Cliente123!",
    "firstName":"Cliente",
    "lastName":"Checkout"
  }'
Si ya existe:

curl -i -c cookies.txt -X POST "$API/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"cliente.checkout@test.com","password":"Cliente123!"}'
Guardar accessToken:

ACCESS_TOKEN="<pega_access_token_aqui>"
## 3.2 Cargar carrito con ítems
Tomar un producto del catálogo:

PRODUCT_ID=$(curl -s "$API/api/v1/products" | jq -r '.items[0].id')
Obtener carrito para leer rowVersion:

curl -s "$API/api/v1/cart" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -b cookies.txt > cart.json

ROW_VERSION=$(jq -r '.rowVersion' cart.json)
Agregar producto:

curl -i -X POST "$API/api/v1/cart/items" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d "{\"productId\":\"$PRODUCT_ID\",\"quantity\":2,\"rowVersion\":\"$ROW_VERSION\"}"
## 4) Backend — preview del checkout
Este endpoint valida dirección y calcula subtotal/shipping/tax/total en servidor.

curl -i -X POST "$API/api/v1/checkout/preview" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "shippingAddress": {
      "fullName": "Ada Lovelace",
      "addressLine1": "742 Evergreen Terrace",
      "addressLine2": "Depto 2",
      "city": "Springfield",
      "state": "IL",
      "postalCode": "62701",
      "countryCode": "US"
    }
  }'
Validar en respuesta:

items[] con snapshot de checkout (productName, sku, unitPrice, quantity, lineTotal).

totals.subtotal, totals.shippingAmount, totals.taxAmount, totals.total.

shippingAddress reflejando la dirección validada.

## 5) Backend — crear orden
curl -i -X POST "$API/api/v1/checkout/orders" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "shippingAddress": {
      "fullName": "Ada Lovelace",
      "addressLine1": "742 Evergreen Terrace",
      "addressLine2": "Depto 2",
      "city": "Springfield",
      "state": "IL",
      "postalCode": "62701",
      "countryCode": "US"
    }
  }' | tee order-create.json
Esperado:

201 Created.

Estado inicial de orden: PendingPayment.

Items persistidos con snapshot (nombre/SKU/precio/cantidad).

Carrito vacío luego de crear la orden.

Guardar ID:

ORDER_ID=$(jq -r '.id' order-create.json)
## 6) Backend — consultar orden y verificar snapshot
curl -i "$API/api/v1/checkout/orders/$ORDER_ID" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -b cookies.txt
Validar:

Totales consistentes con preview/creación.

Snapshot de ítems/precios persistido en la orden.

La orden no depende del catálogo en tiempo real para mostrar histórico.

## 7) Backend — state machine de orden
## 7.1 Transición válida
curl -i -X PATCH "$API/api/v1/checkout/orders/$ORDER_ID/status" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"status":"Paid"}'
Esperado: 200 OK y estado actualizado.

## 7.2 Transición inválida (debe fallar)
curl -i -X PATCH "$API/api/v1/checkout/orders/$ORDER_ID/status" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"status":"Completed"}'
Esperado:

409 Conflict.

Mensaje indicando transición inválida.

## 8) Frontend — validar wizard de checkout
Abrir http://localhost:5173.

Iniciar sesión con usuario Customer.

Agregar productos al carrito.

Ir a Checkout desde el header.

Paso Dirección:

Intentar continuar con campos vacíos.

Deben aparecer errores por campo.

Completar dirección válida y continuar.

Paso Revisión:

Confirmar ítems + totales calculados por backend.

Confirmar compra.

Paso Resultado:

Ver ID de orden, estado y total final.

Accesibilidad mínima esperada
Cambio de foco al título/encabezado del paso actual.

Mensajes de progreso con aria-live.

Resumen de errores de validación con role="alert".

## 9) Checklist de aceptación (Etapa 5)
 Dirección validada correctamente por backend.

 Shipping e impuestos aplicados en backend.

 Totales finales vienen del backend (fuente de verdad).

 Snapshot de ítems/precios guardado al crear orden.

 State machine de orden activa (acepta/rechaza transiciones correctamente).

 Wizard frontend por pasos funcionando.

 Validación por paso funcionando en UI.

 Accesibilidad básica (foco + alertas + live region) presente.

## 10) Casos de error recomendados
countryCode inválido (no ISO de 2 letras) en preview/create.

Crear orden con carrito vacío.

Estado inexistente en PATCH (ej. "RandomStatus") ⇒ validación.

Transición inválida en state machine ⇒ 409.

Leer orden de otro usuario ⇒ 404.

Producto ya no publicado entre carrito y checkout ⇒ error consistente