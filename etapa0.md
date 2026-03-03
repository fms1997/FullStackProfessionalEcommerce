3) Checklist de “Etapa 0 lista” (rápido)

Backend:

 corre con dotnet run

 /swagger OK

 /api/v1/ping OK

 /health/live y /health/ready OK

 logs en consola + archivo logs/

 errores devuelven application/problem+json

 CORS permite http://localhost:5173

Frontend:

 corre con npm run dev

 router base OK

 tailwind + tokens OK

 env VITE_API_BASE_URL visible en Home

 ErrorBoundary implementado