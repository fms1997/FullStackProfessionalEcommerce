Qué vamos a validar
Backend
CSRF real en endpoints mutantes (POST/PUT/PATCH/DELETE).

Headers de hardening (CSP, XFO, nosniff, etc.).

Rate limit por IP/usuario.

Auditoría de eventos en logs.

Validación/sanitización de input.

Gestión de secretos JWT.

Frontend
Flujo de CSRF automático en RTK Query.

Manejo seguro de token en sessionStorage (no localStorage).

Sanitización de orderId/reason en páginas de pago.

CSP awareness en index.html.

0) Preparación rápida
Si estás en este mismo entorno, el backend .NET no corre porque no está instalado dotnet.
En tu máquina local sí podés correr todo normal.

API base de auth y endpoint CSRF ya existen. 

Antiforgery global con header X-CSRF-TOKEN y cookie __Host-csrf ya está activo.

Validación automática antiforgery en controllers ya está activada.

1) Probar CSRF en backend (curl)
Objetivo: confirmar que sin token CSRF falla y con token CSRF pasa.

Paso 1.1 — obtener token CSRF y cookies
curl -i -c cookies.txt http://localhost:5000/api/v1/auth/csrf-token
Esperado:

200 OK

JSON con csrfToken

cookie __Host-csrf en cookies.txt
(esto lo emite GetCsrfToken). 

Paso 1.2 — intentar login SIN header CSRF
curl -i -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fulspectrum.local","password":"Admin123!"}' \
  http://localhost:5000/api/v1/auth/login
Esperado:

Rechazo por antiforgery (400/forbidden según pipeline).

Paso 1.3 — repetir login CON header CSRF correcto
Copiá csrfToken del paso 1.1

Ejecutá:

curl -i -b cookies.txt -c cookies.txt \
  -H "Content-Type: application/json" \
  -H "X-CSRF-TOKEN: <TOKEN_CSRF>" \
  -d '{"email":"admin@fulspectrum.local","password":"Admin123!"}' \
  http://localhost:5000/api/v1/auth/login
Esperado:

200 OK

accessToken en body y cookie de refresh.
Flujo login/refresh/logout está en controller. 

2) Probar headers de hardening
curl -I http://localhost:5000/health/live
Validar presencia de:

Content-Security-Policy

X-Frame-Options: DENY

X-Content-Type-Options: nosniff

Referrer-Policy

Permissions-Policy

Cross-Origin-Opener-Policy

Cross-Origin-Resource-Policy
Headers definidos en middleware. 

3) Probar rate limit por IP/usuario
Rate limiter global:

anónimo: 40 req/min

autenticado: 120 req/min

Paso 3.1 — anónimo
for i in $(seq 1 60); do
  curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5000/health/live
done
Esperado:

al principio 200, luego 429.

Paso 3.2 — autenticado
Repetí prueba sobre endpoint protegido usando Authorization: Bearer <token>.
Esperado:

aguanta más requests antes de 429 (límite mayor).

4) Probar auditoría de eventos
La auditoría registra rutas sensibles y métodos mutantes. 

Ejecutá login, refresh, logout, forgot-password.

Revisá logs (FulSpectrum.Api/logs/ o consola).

Confirmá mensajes con patrón AuditEvent y campos Path/Method/StatusCode/UserId/Ip/TraceId.

También hay eventos de auth desde controller (success/fail). 

5) Probar validación y sanitización
Validadores agregados para auth DTOs. 

Casos prácticos
register con email inválido -> debe fallar.

password muy corta -> debe fallar.

nombre con caracteres fuera de regex -> debe fallar.

Sanitización de campos de auth aplicada antes de procesar (InputSanitizer.Clean). 

6) Probar gestión de secretos JWT
La app bloquea arranque si secreto JWT es débil fuera de Development.

Test
En entorno no Development, deja Jwt:SecretKey con placeholder o <32 chars.

Arranque esperado: excepción y no levanta.

Con secreto fuerte: arranca normal.

7) Probar frontend (paso a paso)
RTK Query:

obtiene CSRF automático para métodos mutantes,

agrega X-CSRF-TOKEN,

usa credentials: include. 

Paso 7.1 — abrir app y login
En DevTools > Network, hacé login.

Verificá primero request a /api/v1/auth/csrf-token y luego POST /auth/login con header X-CSRF-TOKEN.

Paso 7.2 — verificar almacenamiento de token
En DevTools > Application:

sessionStorage debe tener fulspectrum_auth. 

localStorage no debería contener auth nuevo.

Paso 7.3 — validar sanitización visual en pagos
Abrí URL con payload raro, por ejemplo:

/payment/success?orderId=<script>alert(1)</script>

/payment/fail?reason=<img src=x onerror=alert(1)>

Debe mostrarse sanitizado/no ejecutado.
Implementado en páginas de pago con helpers. 

Paso 7.4 — revisar CSP del frontend
En index.html está meta CSP base. 

8) Checklist final (rápido)
 CSRF bloquea mutaciones sin token.

 Headers de seguridad presentes.

 Rate limit devuelve 429 al superar ventana.

 Logs contienen AuditEvent útil para trazabilidad.

 Validación rechaza payloads inválidos.

 sessionStorage usado para access token.

 Sanitización en URLs de pago efectiva.