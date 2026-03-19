1) Levanta backend y frontend
Backend
cd FulSpectrum
dotnet run --project FulSpectrum.Api
Frontend
cd fulspectrum-web
npm install
npm run dev
Nota: la API crea un admin bootstrap automáticamente:

email: admin@fulspectrum.local

password: Admin123!

2) Smoke test de backend y auth
API="http://localhost:5000"
curl -i "$API/health/live"
curl -i "$API/health/ready"
Login admin:

curl -i -c admin-cookies.txt -X POST "$API/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fulspectrum.local","password":"Admin123!"}'
Guarda el accessToken de la respuesta:

ADMIN_TOKEN="<pega_token>"
3) Prueba RBAC (que solo admin pueda usar /admin)
Todos los endpoints admin están bajo api/v1/admin y protegidos con policy CanManageCatalog.

Debe pasar con admin
curl -i "$API/api/v1/admin/catalog/products" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
Debe fallar con customer (403)
Haz login con un usuario customer y prueba lo mismo.

4) CRUD admin de catálogo (API)
4.1 Listar productos admin con filtros
curl -i "$API/api/v1/admin/catalog/products?search=pulse&isPublished=true" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
(Filtros search + isPublished implementados).

4.2 Crear producto con variantes + stock
curl -i -X POST "$API/api/v1/admin/catalog/products" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryId":"<uuid_categoria_existente>",
    "name":"Producto Admin Test",
    "slug":"producto-admin-test",
    "sku":"ADM-TEST-001",
    "basePrice":120,
    "currency":"USD",
    "isPublished":false,
    "variants":[
      {
        "variantSku":"ADM-TEST-001-BLK",
        "name":"Negro",
        "priceDelta":0,
        "isDefault":true,
        "quantityOnHand":20,
        "reservedQuantity":0,
        "reorderThreshold":5
      }
    ]
  }'
(Crea producto + variantes + inventory).

4.3 Actualizar producto
curl -i -X PUT "$API/api/v1/admin/catalog/products/<product_id>" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ ...mismo payload modificado... }'
(Replace completo de variantes + inventario).

4.4 Borrar producto
curl -i -X DELETE "$API/api/v1/admin/catalog/products/<product_id>" \
  -H "Authorization: Bearer $ADMIN_TOKEN"

5) Bulk actions (admin)
Publicar/despublicar masivo:

curl -i -X POST "$API/api/v1/admin/catalog/products/bulk-publish" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"productIds":["<id1>","<id2>"],"isPublished":true}'

Borrado masivo:

curl -i -X DELETE "$API/api/v1/admin/catalog/products/bulk-delete" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"productIds":["<id1>","<id2>"]}'

6) Gestión de stock por variante
curl -i -X PATCH "$API/api/v1/admin/catalog/variants/<variant_id>/stock" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"quantityOnHand":25,"reservedQuantity":2,"reorderThreshold":5}'

7) Gestión de órdenes (listar + cambiar estado)
Listar con filtro:

curl -i "$API/api/v1/admin/orders?status=Paid&search=juan" \
  -H "Authorization: Bearer $ADMIN_TOKEN"

Cambiar estado:

curl -i -X PATCH "$API/api/v1/admin/orders/<order_id>/status" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"Processing"}'
Valida transición con TryTransitionTo (si no válida => 409).

8) Subida de imágenes (upload)
curl -i -X POST "$API/api/v1/admin/uploads/images" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -F "file=@/ruta/local/imagen.jpg"
límite request: 10 MB.

guarda local en wwwroot/uploads y devuelve URL pública.

interfaz preparada para cambiar luego a Cloudinary/S3.

9) Prueba completa en Frontend (UI)
Entra a /login, inicia sesión como admin.

Verifica que aparezca link Admin en el header.

Entra a /admin (ruta protegida para rol Admin).

En panel:

usa filtros + acciones masivas de catálogo,

prueba +1 stock por variante,

crea producto con formulario validado por Zod,

sube imagen,

filtra órdenes y cambia estado.

Además, el frontend ya tiene todos los endpoints admin cableados en RTK Query para esas operaciones.F:fulspectrum-web/src/state/api.ts†L129-L177】

