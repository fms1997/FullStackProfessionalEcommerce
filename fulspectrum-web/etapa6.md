1) Qué funcionalidades vamos a validar
Crear intento de pago con Idempotency-Key (evitar duplicados).

Consultar estado de pago por orden.

Webhook firmado y transición de estados de pago/orden.

Frontend: pantallas pending, success, fail; polling y reintento controlado.

2) Pre-requisitos antes de probar
Tener API corriendo (ej: https://localhost:5001 o similar).

Tener un usuario logueado y JWT válido (porque create/status requieren auth).

Tener un orderId existente del usuario autenticado.

Confirmar secretos de webhooks en appsettings.Development.json.

3) Prueba backend — Crear intento de pago (idempotencia)
Paso 3.1 — Crear intento con header idempotente
curl -i -X POST "https://localhost:5001/api/v1/payments/orders/<ORDER_ID>/attempts" \
  -H "Authorization: Bearer <JWT>" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: test-key-001" \
  -d '{"provider":"Stripe","returnUrl":"http://localhost:5173/payment/pending?orderId=<ORDER_ID>"}'
Esperado: 201 con paymentId, externalReference, checkoutUrl.
El backend usa Idempotency-Key y si ya existe ese intento para orden+proveedor+key, devuelve el existente.

Paso 3.2 — Repetir exactamente la misma request
Mismo curl, misma key test-key-001.

Esperado: 200 (no crea otro pago, te devuelve el previo).

4) Prueba backend — Consultar estado del pago
curl -i "https://localhost:5001/api/v1/payments/orders/<ORDER_ID>/status" \
  -H "Authorization: Bearer <JWT>"
Esperado inicial: Pending (si recién creaste intento).
Si no hay pago aún, devuelve NotStarted.

5) Prueba backend — Webhook firmado (caso exitoso)
El endpoint valida HMAC SHA256 del body con secret de config y header X-Signature.

Paso 5.1 — Armar payload webhook
Ejemplo:

{
  "eventId": "evt-1001",
  "eventType": "payment.updated",
  "paymentId": "<PROVIDER_PAYMENT_ID>",
  "status": "Succeeded",
  "amount": 120.5,
  "currency": "USD",
  "errorCode": null,
  "errorMessage": null
}
DTO esperado por backend.

Paso 5.2 — Calcular firma (ejemplo rápido con Python)
python - <<'PY'
import hmac, hashlib, json
secret = b"dev_stripe_webhook_secret"
payload = {
  "eventId":"evt-1001",
  "eventType":"payment.updated",
  "paymentId":"<PROVIDER_PAYMENT_ID>",
  "status":"Succeeded",
  "amount":120.5,
  "currency":"USD",
  "errorCode":None,
  "errorMessage":None
}
body = json.dumps(payload, separators=(",",":"), ensure_ascii=False).encode()
sig = hmac.new(secret, body, hashlib.sha256).hexdigest().upper()
print(body.decode())
print(sig)
PY
Paso 5.3 — Enviar webhook con firma
curl -i -X POST "https://localhost:5001/api/v1/payments/webhooks/stripe" \
  -H "Content-Type: application/json" \
  -H "X-Signature: <HEX_UPPERCASE_GENERADA>" \
  -d '<BODY_EXACTO_USADO_PARA_FIRMA>'
Esperado: 200 “Webhook procesado.” y luego status pasa a Succeeded.

Paso 5.4 — Reenviar el mismo eventId
Esperado: 200 “Evento ya procesado.” (idempotencia webhook).

Paso 5.5 — Probar firma inválida
Mandá cualquier X-Signature incorrecta.

Esperado: 400 “Firma inválida.” y log de webhook rechazado.

6) Prueba frontend — flujo pending/success/fail
Las rutas frontend implementadas son:

/payment/pending

/payment/success

/payment/fail

Paso 6.1 — Entrar a pending con orderId
Abrí:

http://localhost:5173/payment/pending?orderId=<ORDER_ID>
pending hace polling cada 3s al backend y redirige según estado final.

Paso 6.2 — Simular transición a success con webhook
Cuando mandes webhook Succeeded, la pantalla debería redirigir sola a:

/payment/success?orderId=<ORDER_ID>

Paso 6.3 — Simular fail/canceled
Mandá webhook con status = Failed o Canceled.
Debe redirigir a /payment/fail?... y mostrar botón de reintento.

Paso 6.4 — Probar reintento controlado
En pantalla fail, clic en Reintentar pago:

botón se deshabilita mientras reintenta,

usa idempotency key en sessionStorage,

redirige al nuevo checkoutUrl.

7) Checklist rápido de aceptación (tu “done” de pruebas)
 Repetir POST attempts con misma key no duplica pago.

 GET status refleja último estado real.

 Webhook con firma válida procesa.

 Webhook duplicado (mismo eventId) no reprocesa.

 Webhook con firma inválida rechaza con 400.

 Frontend pending redirige a success/fail según backend.

 Fail permite reintento y no dispara clicks múltiples.

8) Comandos que utilicé para prepararte esta guía
nl -ba FulSpectrum/FulSpectrum.Api/Controllers/PaymentsController.cs | sed -n '1,280p'

nl -ba FulSpectrum/FulSpectrum.Api/Controllers/PaymentsDtos.cs | sed -n '1,120p'

nl -ba FulSpectrum/FulSpectrum.Domain/Catalog/Payment.cs | sed -n '1,180p'

nl -ba FulSpectrum/FulSpectrum.Api/appsettings.Development.json | sed -n '1,120p'

nl -ba FulSpectrum/frontend/src/App.tsx | sed -n '1,80p'

nl -ba FulSpectrum/frontend/src/api/payments.ts | sed -n '1,220p'

nl -ba FulSpectrum/frontend/src/pages/PaymentPendingPage.tsx | sed -n '1,220p'

nl -ba FulSpectrum/frontend/src/pages/PaymentFailPage.tsx | sed -n '1,180p'

nl -ba FulSpectrum/frontend/src/pages/PaymentSuccessPage.tsx | sed -n '1,160p'

nl -ba FulSpectrum/frontend/src/components/RetryPaymentButton.tsx | sed -n '1,200p'

nl -ba FulSpectrum/frontend/README.md | sed -n '1,220p'

Si quieres, en el siguiente mensaje te doy una plantilla exacta de colección de Postman (requests listas para copiar/pegar) para ejecutar todo esto en 5 minutos.