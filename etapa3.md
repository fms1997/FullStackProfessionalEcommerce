# Etapa 3 — Autenticación y autorización completa (paso a paso)

Esta guía es para **probar toda la etapa 3 de punta a punta**: backend + frontend, incluyendo JWT, refresh con rotación/revocación, roles/policies, guards y manejo de 401/403.

---

## 0) Pre-requisitos

- Backend .NET corriendo.
- Frontend Vite corriendo.
- Base de datos accesible para el backend.
- Tener `curl` instalado.

### 0.1 Configuración mínima backend (`FulSpectrum.Api/appsettings.Development.json`)

Verifica:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:SecretKey` (larga y segura)
- `Jwt:AccessTokenMinutes`
- `Jwt:RefreshTokenDays`
- `Cors:AllowedOrigins` incluye `http://localhost:5173`

### 0.2 Configuración frontend

En `fulspectrum-web/.env`:

```bash
VITE_API_BASE_URL=http://localhost:5000
```

> Ajusta el puerto según tu API.

---

## 1) Levantar servicios

### 1.1 Backend

Desde la carpeta raíz del repo:

```bash
cd FulSpectrum
dotnet run --project FulSpectrum.Api
```

### 1.2 Frontend

En otra terminal:

```bash
cd fulspectrum-web
npm install
npm run dev
```

---

## 2) Smoke checks del backend

```bash
curl -i http://localhost:5000/health/live
curl -i http://localhost:5000/health/ready
curl -i http://localhost:5000/api/v1/products
```

Esperado:
- `200 OK` en health.
- listado público de products disponible (`GET` está anónimo).

---

## 3) Flujo AUTH backend completo (curl)

> En los ejemplos uso:
> - API base: `http://localhost:5000`
> - cookie jar: `cookies.txt`

```bash
API="http://localhost:5000"
```

### 3.1 Register

```bash
curl -i -c cookies.txt -X POST "$API/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email":"cliente1@test.com",
    "password":"Cliente123!",
    "firstName":"Cliente",
    "lastName":"Uno"
  }'
```

Esperado:
- `200 OK`
- body con `accessToken`, `expiresAtUtc`, `profile`
- cookie `refreshToken` seteada (httpOnly)

Guarda el token en shell:

```bash
ACCESS_TOKEN="<pega_access_token_aqui>"
```

### 3.2 Endpoint protegido `me`

```bash
curl -i "$API/api/v1/auth/me" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

Esperado:
- `200 OK`
- perfil del usuario autenticado

### 3.3 Refresh token (rotación)

```bash
curl -i -b cookies.txt -c cookies.txt -X POST "$API/api/v1/auth/refresh"
```

Esperado:
- `200 OK`
- nuevo `accessToken`
- nueva cookie `refreshToken` (rota)

### 3.4 Logout + revocación sesión

```bash
curl -i -b cookies.txt -X POST "$API/api/v1/auth/logout" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

Después prueba refresh:

```bash
curl -i -b cookies.txt -X POST "$API/api/v1/auth/refresh"
```

Esperado:
- logout `204 No Content`
- refresh posterior `401 Unauthorized`

---

## 4) Forgot/Reset password (backend)

### 4.1 Forgot

```bash
curl -i -X POST "$API/api/v1/auth/forgot-password" \
  -H "Content-Type: application/json" \
  -d '{"email":"cliente1@test.com"}'
```

Esperado:
- `200 OK`
- en entorno demo puede devolver `resetToken` en respuesta

### 4.2 Reset

```bash
curl -i -X POST "$API/api/v1/auth/reset-password" \
  -H "Content-Type: application/json" \
  -d '{
    "token":"<reset_token>",
    "newPassword":"Nueva123!"
  }'
```

Esperado:
- `200 OK`
- sesiones refresh activas del usuario revocadas

---

## 5) Roles + policies + protección de recursos

La API define policy `CanManageCatalog` para acciones de catálogo de escritura (`POST/PUT/DELETE` products).

## 5.1 Probar con usuario Customer (de register)

```bash
curl -i -X DELETE "$API/api/v1/products/<product_id>" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

Esperado:
- `403 Forbidden` (sin rol admin)

## 5.2 Probar con Admin

El backend crea admin bootstrap:
- email: `admin@fulspectrum.local`
- password: `Admin123!`

Login admin:

```bash
curl -i -c admin-cookies.txt -X POST "$API/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fulspectrum.local","password":"Admin123!"}'
```

Luego:

```bash
ADMIN_ACCESS_TOKEN="<token_admin>"

curl -i -X DELETE "$API/api/v1/products/<product_id>" \
  -H "Authorization: Bearer $ADMIN_ACCESS_TOKEN"
```

Esperado:
- con admin sí pasa (`204` o `404` si no existe el producto)

---

## 6) Flujo frontend completo

Abre `http://localhost:5173`.

### 6.1 Register
1. Ir a `/register`
2. Completar form y enviar
3. Debe redirigir al home autenticado

### 6.2 Login
1. Salir (`logout`)
2. Ir a `/login`
3. Entrar con credenciales válidas
4. Ver email/rol en el header

### 6.3 Forgot password
1. Ir a `/forgot-password`
2. Enviar email
3. Usar token retornado en demo y cambiar password

### 6.4 Guard de rutas por rol
1. Sin login, entrar a `/` → redirige a `/login`
2. Con login, entrar a `/` → deja pasar
3. Si una ruta exige rol no permitido → `/forbidden`

### 6.5 Persistencia + refresh automático
1. Login correcto
2. Recargar navegador (F5)
3. Debe mantener sesión en frontend
4. Cuando expire access token, una request debe disparar refresh y continuar sin expulsarte

### 6.6 Manejo centralizado de 401/403
- Simular 401: borrar cookie refresh y dejar access token vencido ⇒ debe limpiar auth y pedir login.
- Simular 403: usuario Customer intentando borrar producto ⇒ mostrar mensaje global de permisos.

---

## 7) Checklist final de aceptación de Etapa 3

Marca todo en ✅ para darla por cerrada:

- [ ] Register/login funcionando.
- [ ] `accessToken` se usa en `Authorization: Bearer`.
- [ ] `refreshToken` via cookie httpOnly.
- [ ] Refresh rota token (token viejo invalidado).
- [ ] Logout revoca sesión refresh.
- [ ] Forgot/reset password operativo.
- [ ] Roles en claims/token y policies activas.
- [ ] Endpoints críticos protegidos (`POST/PUT/DELETE products`).
- [ ] Guards frontend por autenticación/rol.
- [ ] Persistencia de sesión frontend.
- [ ] Reauth automático en 401.
- [ ] 403 centralizado visible en UI.

---

## 8) Troubleshooting rápido

- **401 constante al refrescar**: revisar cookie `refreshToken`, CORS con `AllowCredentials` y origin exacto.
- **403 inesperado**: revisar claim `role` en access token y policy endpoint.
- **Frontend sin pegar al backend**: validar `VITE_API_BASE_URL`.
- **No aparecen tablas nuevas**: ejecutar migraciones EF en entorno con SDK `dotnet`.

