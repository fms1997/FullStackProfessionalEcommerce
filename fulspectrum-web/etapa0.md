# Etapa 0 — Guía de pruebas (Setup profesional)

Esta guía te permite validar de forma ordenada los puntos de la **Etapa 0** en backend y frontend.

## 0) Prerrequisitos

- .NET SDK (para correr la API)
- Node.js + npm (para correr frontend con Vite)
- `curl` y `jq` (para pruebas HTTP)

## 1) Backend — Clean setup y observabilidad

### 1.1 Levantar la API en Development

```bash
cd FulSpectrum
ASPNETCORE_ENVIRONMENT=Development dotnet run --project FulSpectrum.Api/FulSpectrum.Api.csproj
```

Esperado:

- La app inicia sin excepciones.
- Expone `http://localhost:5006` y `https://localhost:7193` (según `launchSettings.json`).

### 1.2 Swagger + versionado básico

```bash
curl -I http://localhost:5006/swagger/index.html
curl -s http://localhost:5006/swagger/v1/swagger.json | jq '.info.version'
```

Esperado:

- HTTP `200` en Swagger.
- Versión `"v1"`.

### 1.3 Endpoint versionado de salud funcional

```bash
curl -s http://localhost:5006/api/v1/ping
```

Esperado:

- HTTP `200` y respuesta tipo pong/ok.

### 1.4 Health checks (live/ready)

```bash
curl -s http://localhost:5006/health/live | jq .
curl -s http://localhost:5006/health/ready | jq .
```

Esperado:

- HTTP `200` y payload de health checks.

### 1.5 Middleware global de errores (ProblemDetails)

Provoca un error controlado llamando a un endpoint inválido o forzando un body incorrecto:

```bash
curl -i http://localhost:7193/api/v1/no-existe
```

Esperado:

- Respuesta con contrato estándar de error (`application/problem+json`) cuando aplique.

### 1.6 Logging estructurado (Serilog)

Con la API corriendo, ejecuta algunas requests y valida log en consola y archivo:

```bash
ls -la FulSpectrum.Api/logs
```

Esperado:

- Existe archivo `log-YYYYMMDD.txt` en `FulSpectrum.Api/logs/`.

### 1.7 CORS + HTTPS + headers de seguridad

**CORS** (preflight):

```bash
curl -i -X OPTIONS https://localhost:7193/api/v1/ping \
  -H "Origin: http://localhost:5173" \
  -H "Access-Control-Request-Method: GET"
```

**Headers de seguridad**:

```bash
curl -I http://localhost:5006/api/v1/ping
```

Esperado:

- CORS permite el origin configurado.
- Respuesta incluye `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`.

## 2) Frontend — Vite/React base escalable

> Si todavía no existe la app frontend en el repo, esta sección sirve como checklist objetivo.

### 2.1 Levantar frontend

```bash
npm run dev
```

Esperado:

- App disponible (normalmente en `http://localhost:5173`).

### 2.2 Router base y Error Boundary

Pruebas manuales:

- Navegar ruta principal `/`.
- Navegar a ruta inexistente para validar fallback de routing.
- Forzar error en un componente para confirmar captura por Error Boundary.

### 2.3 Tailwind + design tokens

Pruebas manuales:

- Verificar estilos Tailwind aplicados.
- Confirmar uso de tokens (colores/spacing/typography) en componentes base.

### 2.4 Env vars frontend

Prueba:

- Mostrar `VITE_API_BASE_URL` en Home o panel debug.

Esperado:

- Valor visible y correcto para el entorno.

### 2.5 ESLint / Prettier

```bash
npm run lint
npm run format:check
```

Esperado:

- Sin errores críticos de lint/format.

## 3) Criterio de aceptación “Etapa 0 lista”

Backend:

- [ ] `dotnet run` OK
- [ ] `/swagger` OK
- [ ] `/api/v1/ping` OK
- [ ] `/health/live` y `/health/ready` OK
- [ ] logs en consola + archivo `logs/`
- [ ] errores bajo estándar ProblemDetails
- [ ] CORS para `http://localhost:5173`
- [ ] HTTPS + headers de seguridad activos

Frontend:

- [ ] `npm run dev` OK
- [ ] Router base OK
- [ ] Tailwind + tokens OK
- [ ] Env vars (`VITE_API_BASE_URL`) OK
- [ ] ErrorBoundary implementado
- [ ] Estructura feature-based confirmada
