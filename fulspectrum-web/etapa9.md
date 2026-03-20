Summary
Implementé la Etapa 9 de observabilidad backend con OpenTelemetry para trazas/métricas (ASP.NET Core, HttpClient, EF Core, runtime/proceso) y exportación OTLP configurable por OpenTelemetry:Otlp:Endpoint.

Integré la observabilidad en el arranque del backend (AddBrioObservability) y reforcé logging estructurado en Serilog con enriquecimiento de Application, Environment, MachineName, ThreadId, además de enriquecimiento por request (CorrelationId, UserName, RequestHost).

Agregué middleware de Correlation ID (X-Correlation-ID) que toma el header entrante o genera uno, lo propaga en respuesta y lo inyecta en LogContext/scope para correlación end-to-end en logs y trazas.

Añadí los paquetes necesarios en el backend para OpenTelemetry y enriquecedores de Serilog.

Implementé la Etapa 9 de observabilidad frontend inicializando: error reporting, Web Vitals/performance y logging opcional de eventos de UI desde main.jsx.

Extendí el cliente Axios para enviar X-Correlation-ID, medir tiempos de requests y reportar errores de API automáticamente.

Implementé módulos frontend para:

Error reporting (window.error, unhandledrejection, Sentry vía window.Sentry + fallback sendBeacon).

Performance marks + Web Vitals (LCP/INP/CLS).

UI event logs opcionales con data-ui-event.

Agregué medición de performance en cambios de ruta en AppRoutes para observabilidad de navegación real de usuario.

Documenté la etapa completa “paso a paso” (backend, frontend, dashboards conceptuales, variables de entorno y concepto clave de monitoreo/debug en producción).

También dejé commit realizado en la rama actual: 380db81, y ya generé el PR con título y descripción.

Testing

⚠️ dotnet build Backend/Backend/Backend.csproj (no disponible en el entorno: dotnet: command not found).

❌ npm run build (falló por errores preexistentes fuera de esta etapa: import no resuelto ./useSucursalesTipoS y JSX con atributo duplicado en módulos no tocados).


Archivos (12)
Quiero probar las funcionalidades de esta etapa paso a paso
Copiar
Perfecto — te dejo un plan de pruebas paso a paso para validar toda la Etapa 9 (backend + frontend) en tu entorno.

0) Preparación (una sola vez)
Configurá variables de entorno sugeridas (backend/frontend).

Verificá que el backend esté levantando OpenTelemetry y Serilog enriquecido en Program.cs.

Verificá que el frontend inicialice observabilidad en main.jsx.

1) Probar Correlation ID end-to-end (Backend)
Objetivo
Confirmar que X-Correlation-ID:

entra por header o se genera,

se devuelve en response,

se inyecta en logs.

Middleware implementado aquí.

Pasos
Hacer request sin header:

curl -i http://localhost:5000/healthz
Esperado: response con X-Correlation-ID generado.

Hacer request con header:

curl -i -H "X-Correlation-ID: prueba-manual-123" http://localhost:5000/healthz
Esperado: response devuelve exactamente prueba-manual-123.

Revisar logs (consola/Seq) y validar presencia de CorrelationId en request logging.

2) Probar Tracing y métricas OpenTelemetry (Backend)
Objetivo
Confirmar que salen traces/metrics de ASP.NET, HttpClient, EF, runtime/proceso.

Instrumentación definida aquí.

Pasos
Levantar collector/stack observabilidad (OTLP endpoint).

Lanzar backend con OpenTelemetry__Otlp__Endpoint.

Generar tráfico:

for i in {1..20}; do curl -s http://localhost:5000/healthz > /dev/null; done
En tu backend de observabilidad (Jaeger/Tempo/Grafana/Datadog):

buscar servicio ExpresoBrio.API,

verificar spans HTTP entrantes y métricas runtime/proceso.

3) Probar logs estructurados + enrichment (Backend)
Objetivo
Verificar propiedades enriquecidas:
Application, Environment, MachineName, ThreadId, CorrelationId, UserName, RequestHost.

Configuración en Program.cs.

Pasos
Ejecutar:

curl -i http://localhost:5000/logtest
Endpoint de test de log ya existe.

En consola/Seq, validar campos estructurados mencionados.

4) Probar Correlation ID desde Frontend hacia Backend
Objetivo
Validar que Axios agrega X-Correlation-ID automáticamente.

Interceptor implementado aquí.

Pasos
Abrí frontend en navegador.

Dispará cualquier acción que haga request API.

En DevTools > Network > Request Headers:

verificar X-Correlation-ID.

En backend logs, verificar mismo correlation id.

Generación del id aquí.

5) Probar Performance marks (Frontend)
Objetivo
Ver mediciones de:

requests HTTP,

cambios de ruta,

Web Vitals.

Implementación aquí.

Pasos
En modo dev, abrir consola y navegar entre rutas.

Esperado:

logs [PerfMeasure] para route changes / HTTP.【F:frontend/src/observability/performance.js†L20-L26

logs [WebVitals] (LCP/INP/CLS).

6) Probar Error Reporting (Frontend)
Objetivo
Ver captura de:

window.error,

unhandledrejection,
con Sentry (si está en window.Sentry) o fallback sendBeacon.

Implementación aquí.

Pasos
Activar:

VITE_ENABLE_ERROR_REPORTING=true
(Opcional) configurar VITE_SENTRY_DSN.

En consola del navegador, forzar error:

setTimeout(() => { throw new Error("test-window-error"); }, 0)
Promise.reject("test-unhandled-rejection")
Verificar:

evento en Sentry (si SDK cargado),

o llamada beacon al endpoint fallback.

7) Probar UI Event Log opcional (Frontend)
Objetivo
Ver logs de clicks sobre elementos con data-ui-event.

Implementación aquí.

Pasos
Activar:

VITE_ENABLE_UI_EVENT_LOG=true
En cualquier componente, agregar temporalmente:

<button data-ui-event="boton-prueba">Probar UI Event</button>
Click y revisar consola: [UIEvent] ...

8) Checklist final (aceptación de etapa)
Correlation ID propagado frontend → backend → logs.

Traces y métricas visibles en plataforma OTLP.

Logs estructurados con enrichment.

Error reporting frontend funcionando.

Performance marks + Web Vitals visibles.

UI events opcionales funcionando.

Si querés, en el próximo paso te armo una matriz QA lista para copiar en Jira/TestRail (caso, precondición, pasos, resultado esperado, evidencia).