# Etapa 10 — Performance (frontend y backend) paso a paso

Esta guía te permite **probar rendimiento end-to-end** con foco en métricas reales de usuario y eficiencia de API/DB.

> Objetivo de la etapa: reducir latencia percibida, uso de CPU/memoria y bytes transferidos, sin romper funcionalidad.

---

## 0) Pre-requisitos

- Backend .NET levantando `FulSpectrum.Api`.
- Frontend Vite levantando `fulspectrum-web`.
- SQL Server accesible para el backend.
- Redis disponible (idealmente local por Docker).
- Node.js + npm instalados.
- `curl` y (opcional) `jq`.
- Chrome/Edge para Lighthouse.

### 0.1 Variables/config recomendadas

Revisa en `FulSpectrum/FulSpectrum.Api/appsettings.Development.json`:

- `ConnectionStrings:DefaultConnection`
- `ConnectionStrings:Redis`
- `Cors:AllowedOrigins`

Si `ConnectionStrings:Redis` está vacío, el backend cae a `DistributedMemoryCache` (sirve para dev, pero no valida Redis real).

---

## 1) Levantar servicios

### 1.1 Redis local (si no lo tienes)

```bash
docker run --name fs-redis -p 6379:6379 -d redis:7
```

### 1.2 Backend

```bash
cd FulSpectrum
dotnet run --project FulSpectrum.Api
```

### 1.3 Frontend

```bash
cd fulspectrum-web
npm install
npm run dev
```

---

## 2) Backend — Cache de catálogo (Redis)

En esta API, el catálogo usa `ICatalogCacheService` para cachear:

- `GET /api/v1/products`
- `GET /api/v1/products/{id}`

Además, cada cambio `POST/PUT/DELETE` invalida versión de catálogo.

### 2.1 Prueba rápida de latencia (cold vs warm)

```bash
API="http://localhost:5000"

time curl -s "$API/api/v1/products?page=1&pageSize=50" > /tmp/p1.json

time curl -s "$API/api/v1/products?page=1&pageSize=50" > /tmp/p2.json
```

Esperado:
- Primera llamada (cold) más lenta.
- Segunda llamada (warm cache) más rápida o igual con menor variación.

### 2.2 Validar invalidación de cache

1) Lee catálogo (calienta cache):
```bash
curl -s "$API/api/v1/products?page=1&pageSize=20" > /tmp/pre.json
```

2) Crea o edita un producto (como Admin).

3) Vuelve a consultar:
```bash
curl -s "$API/api/v1/products?page=1&pageSize=20" > /tmp/post.json
```

Esperado:
- El resultado refleja el cambio.
- No se queda “pegado” a la versión vieja.

---

## 3) Backend — ETags + Response Caching

La API ya emite ETag en productos y responde `304 Not Modified` con `If-None-Match`.

### 3.1 Obtener ETag

```bash
curl -i "$API/api/v1/products?page=1&pageSize=20"
```

Guarda el valor del header `ETag`.

### 3.2 Revalidar con If-None-Match

```bash
ETAG='"PEGA_AQUI_EL_ETAG"'
curl -i -H "If-None-Match: $ETAG" "$API/api/v1/products?page=1&pageSize=20"
```

Esperado:
- `304 Not Modified` cuando no hay cambios.
- Si cambió catálogo, vuelve `200` con ETag nuevo.

### 3.3 Verificar cabeceras de caché de respuesta

```bash
curl -I "$API/api/v1/products?page=1&pageSize=20"
```

Esperado:
- `Cache-Control` coherente con `[ResponseCache(Duration=...)]`.

---

## 4) Backend — Query optimization, proyecciones e índices

### 4.1 Confirmar paginación + proyección

Probar distintos tamaños para evitar payload innecesario:

```bash
curl -s "$API/api/v1/products?page=1&pageSize=10" | jq '.items | length'
curl -s "$API/api/v1/products?page=1&pageSize=50" | jq '.items | length'
```

Esperado:
- El backend respeta `pageSize`.
- No trae campos de más (usa DTO/proyección).

### 4.2 SQL plan / índices (recomendado)

Desde SQL Server, ejecuta una consulta típica de catálogo filtrando por categoría/publicación y orden.

Valida que use índices (no full scan) para:
- `Slug` (unique)
- `Sku` (unique)
- `(CategoryId, IsPublished)`

Si ves scans frecuentes en queries de catálogo, crea índice adicional según patrón real (por ejemplo para `CreatedAtUtc` si ordenas siempre por reciente).

### 4.3 Medición simple de throughput

Con `hey`, `wrk` o similar:

```bash
# ejemplo con hey (si lo tienes instalado)
hey -n 300 -c 30 "$API/api/v1/products?page=1&pageSize=20"
```

Compara con y sin Redis (vacía `ConnectionStrings:Redis`) para medir impacto real.

---

## 5) Backend — CDN para imágenes

La app hoy puede servir imágenes locales; en producción, prueba CDN.

### 5.1 Verificar URL de imagen servida por CDN

En respuestas de producto/admin, comprueba que URL final sea de CDN (ej: CloudFront, Cloudflare, Azure CDN).

### 5.2 Revisar headers CDN

```bash
curl -I "https://tu-cdn.com/ruta/imagen.jpg"
```

Esperado:
- `Cache-Control` largo para assets versionados.
- (opcional) headers de cache hit/miss del proveedor.

### 5.3 Prueba visual de impacto

- Carga home por primera vez (cold).
- Recarga dura (Ctrl+Shift+R).
- En DevTools → Network compara tiempo total de imágenes vs origen sin CDN.

---

## 6) Frontend — Code splitting por ruta

El router ya usa `React.lazy` + `Suspense` por página.

### 6.1 Verificar chunks

```bash
cd fulspectrum-web
npm run build
```

Esperado:
- Se generen múltiples archivos JS (no un único bundle enorme).

### 6.2 Validar carga bajo demanda

1) Abre `/` (home).
2) En Network filtra por `js`.
3) Navega a `/admin` o `/orders`.

Esperado:
- Se descarga chunk de la ruta al navegar, no en el primer render.

---

## 7) Frontend — Memoization estratégica

Home usa `memo`, `useMemo` y derivación de estado para reducir renders.

### 7.1 Perfilado de render

- Abre React DevTools Profiler.
- Graba interacción en filtros (`search`, `sortBy`, `sortDirection`).

Esperado:
- Re-render focalizado en componentes impactados.
- `ProductRow` evita rerenders innecesarios cuando props no cambian.

---

## 8) Frontend — Virtualización de listas

Home usa `VirtualizedList` para pintar solo filas visibles.

### 8.1 Prueba con lista grande

- Asegúrate de tener 200+ productos (seed o script).
- Abre Home y haz scroll largo.

Esperado:
- DOM contiene solo subset visible + overscan, no 200 nodos simultáneos.
- Scroll fluido sin jank evidente.

### 8.2 Verificación rápida en DevTools

- En Elements inspecciona contenedor de lista.
- Cuenta aproximada de filas renderizadas.

Debe ser cercano a viewport/alto de item, no al total del catálogo.

---

## 9) Frontend — Optimización de imágenes (lazy + srcset)

Home ya renderiza imágenes con:
- `loading="lazy"`
- `srcSet` y `sizes`
- dimensiones explícitas (`width/height`)

### 9.1 Validar lazy load

- Network → Img.
- Al abrir Home no deberían descargarse imágenes fuera de viewport.
- Al hacer scroll, se descargan progresivamente.

### 9.2 Validar resolución responsive

- Simula pantalla retina y no-retina.
- Verifica selección de recurso `1x/2x` según densidad.

---

## 10) Lighthouse + Core Web Vitals

### 10.1 Lighthouse (local)

1) Abre Home en Chrome.
2) DevTools → Lighthouse.
3) Ejecuta (Mobile y Desktop).

Objetivos recomendados:
- Performance > 85
- Accessibility > 90
- Best Practices > 90
- SEO > 85

### 10.2 Core Web Vitals a revisar

- **LCP** < 2.5s
- **INP** < 200ms
- **CLS** < 0.1

Usa:
- Lighthouse para laboratorio.
- (ideal) RUM/CrUX para usuarios reales.

---

## 11) Checklist de aceptación Etapa 10

- [ ] Redis habilitado y mejora medible en lecturas repetidas de catálogo.
- [ ] ETag retorna `304` en revalidación sin cambios.
- [ ] Response caching con headers correctos.
- [ ] Queries de catálogo usan índices y paginación/proyección.
- [ ] Imágenes servidas por CDN en ambiente productivo.
- [ ] Frontend con code splitting efectivo por ruta.
- [ ] Memoization reduce renders evitables.
- [ ] Virtualización mantiene DOM pequeño en listados grandes.
- [ ] Imágenes optimizadas (`lazy`, `srcset`, tamaño definido).
- [ ] Lighthouse y CWV dentro de umbrales objetivo.

---

## Concepto clave de la etapa

**Rendimiento real end-to-end** = no optimizar una capa aislada, sino la cadena completa:

1. Base de datos (índices + query shape).
2. Backend (cache + ETag + payload eficiente).
3. Red/CDN (distancia y caching HTTP).
4. Frontend (chunks, render, imágenes).
5. UX real (CWV + percepción de velocidad).

Si quieres, te armo en el siguiente paso una **matriz QA ejecutable** (caso, comando, esperado, evidencia) para que solo marques PASS/FAIL.
