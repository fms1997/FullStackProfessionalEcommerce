# Etapa 1 — Plan de pruebas (Datos y dominio ecommerce)

## Objetivo

Validar que el modelado fuerte de ecommerce esté correctamente implementado en backend y frontend.

## 1) Backend (EF Core + SQL Server)

### 1.1 Restaurar y compilar solución

```bash
cd FulSpectrum
dotnet restore FulSpectrum.slnx
dotnet build FulSpectrum.slnx
```

Esperado: build sin errores.

### 1.2 Aplicar migraciones y verificar esquema

```bash
cd FulSpectrum
dotnet ef database update \
  --project FulSpectrum.Infrastructure \
  --startup-project FulSpectrum.Api

dotnet ef migrations list \
  --project FulSpectrum.Infrastructure \
  --startup-project FulSpectrum.Api
```

Esperado:

- Migraciones aplicadas sin errores.
- Se observan `InitialCreate` y `SyncFulSpectrumModel`.

### 1.3 Verificar seeds versionados

La API aplica migraciones al iniciar y crea usuario admin por código de inicialización (`Program.cs`).

Comprobar con SQL:

```sql
SELECT COUNT(*) AS CategoriesCount FROM Categories;
SELECT COUNT(*) AS ProductsCount FROM Products;
SELECT COUNT(*) AS VariantsCount FROM Variants;
SELECT COUNT(*) AS InventoryCount FROM Inventory;
SELECT Email, Role FROM Users WHERE NormalizedEmail = 'ADMIN@FULSPECTRUM.LOCAL';
```

Esperado:

- Categorías >= 2
- Productos >= 2
- Variantes >= 3
- Inventario >= 3
- Existe admin `admin@fulspectrum.local` con rol `Admin`.

### 1.4 Verificar integridad (FKs, índices, constraints)

#### Constraints de dominio

```sql
-- Debe fallar: precio negativo
INSERT INTO Products (Id, CategoryId, Name, Slug, Sku, BasePrice, Currency, IsPublished, CreatedAtUtc)
VALUES (NEWID(), (SELECT TOP 1 Id FROM Categories), 'x', 'x-neg', 'NEG-1', -1, 'USD', 1, SYSUTCDATETIME());

-- Debe fallar: stock negativo
INSERT INTO Inventory (Id, VariantId, QuantityOnHand, ReservedQuantity, ReorderThreshold, UpdatedAtUtc)
VALUES (NEWID(), (SELECT TOP 1 Id FROM Variants), -1, 0, 0, SYSUTCDATETIME());
```

Esperado: error por `CHECK CONSTRAINT`.

#### Índices únicos

```sql
-- Debe fallar por índice único en Products.Sku
INSERT INTO Products (Id, CategoryId, Name, Slug, Sku, BasePrice, Currency, IsPublished, CreatedAtUtc)
VALUES (NEWID(), (SELECT TOP 1 Id FROM Categories), 'dup sku', 'dup-sku', 'PULSE-ANC', 100, 'USD', 1, SYSUTCDATETIME());
```

Esperado: error por índice único.

### 1.5 Probar endpoint catálogo (lectura)

Levantar API:

```bash
cd FulSpectrum/FulSpectrum.Api
dotnet run
```

Pedir productos:

```bash
curl -i "http://localhost:5124/api/v1/products?page=1&pageSize=10"
```

Esperado: `200 OK`, lista paginada y datos consistentes con seeds.

## 2) Frontend (modelado de datos TS/JS)

### 2.1 Verificar compilación y tipos

```bash
cd fulspectrum-web
npm install
npm run build
npm run lint
```

Esperado: build y lint sin errores.

### 2.2 Verificar normalización en estado

Puntos a revisar:

- `src/domain/ecommerce.ts`: contratos de dominio.
- `src/state/normalizeCatalog.ts`: normalización en memoria (entidades/ids).
- `src/types/api.ts`: contratos de API tipados para consumo frontend.

### 2.3 Smoke test UI

```bash
cd fulspectrum-web
npm run dev
```

Navegar a:

- `/login`
- `/register`
- `/forgot-password`
- `/forbidden`

Esperado: rutas renderizan y no hay errores de importación/tipos.

## 3) Criterios de aceptación de Etapa 1

- Entidades `Products`, `Categories`, `Variants`, `Inventory`, `Users` presentes.
- Migraciones aplicables en limpio.
- Seeds consistentes y repetibles.
- Índices/constraints activos y efectivos.
- Frontend compila y mantiene contratos tipados del dominio.
- Estado normalizado donde aplica.
