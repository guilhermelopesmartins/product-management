# ProductManagement

C# microservice (Azure Functions, isolated worker) responsible for the **Products** CRUD, deployed through a two-stage pipeline (`staging` slot → swap → `production`).

This service shares the `StoreManagementDb` database with the `StoreManagement` service (the Stores RESTful API): `StoreManagement` owns the `Companies` and `Stores` tables (via EF Core); `ProductManagement` owns the `Products` table (via Dapper and plain SQL).

---

## Table of contents

- [Architecture](#architecture)
- [Database](#database)
- [Endpoints](#endpoints)
- [Running locally](#running-locally)
- [Tests](#tests)
- [Deployment (staging → swap → production)](#deployment-staging--swap--production)
- [Postman collection](#postman-collection)
- [Design decisions](#design-decisions)
- [Known limitations](#known-limitations)

---

## Architecture

```
ProductManagement.sln
 ├─ src/
 │   ├─ ProductManagement.Functions      → Function endpoints (HTTP Trigger)
 │   ├─ ProductManagement.Application    → Services, DTOs
 │   └─ ProductManagement.Infrastructure → Dapper, SQL Server access
 └─ tests/
     └─ ProductManagement.Tests
```

**Stack:**
- Azure Functions v4, .NET isolated worker
- Dapper (`Microsoft.Data.SqlClient`) for SQL Server access
- Database: Azure SQL Database (`StoreManagementDb`), shared with the Stores service

### Why a database shared with the Stores service?

`Products.StoreId` references `Stores.Id`, and the Stores API already owns the `Companies`/`Stores` tables in the same database. Keeping a single database avoids manually duplicating or syncing that data across two separate databases, and it also demonstrates two different data access styles over the same multi-tenant base: EF Core (managed, in the Stores service) and Dapper with procedures/a scalar function (more explicit SQL, in this service).

### Why Dapper instead of EF Core here?

The requirement explicitly asks for a scalar function and a stored procedure in SQL Server. Dapper maps well onto that pattern — calls to the database invoke the procedure/function directly, without a full ORM abstraction layer in between.

---

## Database

### `Products` table

```sql
CREATE TABLE Products (
    ProductId    UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StoreId      UNIQUEIDENTIFIER NOT NULL REFERENCES Stores(Id),
    Sku          NVARCHAR(50)  NOT NULL,
    Name         NVARCHAR(200) NOT NULL,
    Description  NVARCHAR(MAX) NULL,
    Price        DECIMAL(18,2) NOT NULL,
    Currency     CHAR(3)       NOT NULL DEFAULT 'BRL',
    StockQty     INT           NOT NULL DEFAULT 0,
    IsActive     BIT           NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2     NULL
);
```

- `ProductId` and `StoreId` use `UNIQUEIDENTIFIER` (Guid) for consistency with the rest of the system.
- `Price` uses `DECIMAL(18,2)` to avoid floating-point rounding issues.
- `Currency` supports the globalized/multi-company scenario (products priced in different currencies per company/store).
- `StoreId` is a mandatory FK referencing `Stores.Id` — **a Store can only be deleted once every Product linked to it has been removed or reassigned**, otherwise the operation fails due to a FK violation.

### Scalar function — `fn_GetProductsAsJson`

Returns the product list as a single JSON value, optionally filtered by `StoreId`.

```sql
CREATE FUNCTION dbo.fn_GetProductsAsJson (@StoreId UNIQUEIDENTIFIER = NULL)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @Result NVARCHAR(MAX);

    SELECT @Result = (
        SELECT ProductId, StoreId, Sku, Name, Price, Currency, StockQty
        FROM Products
        WHERE (@StoreId IS NULL OR StoreId = @StoreId)
        FOR JSON PATH
    );

    RETURN ISNULL(@Result, '[]');
END
```

> Scalar functions in SQL Server have known performance limitations (they don't parallelize well over large result sets). In a real production scenario, a view or a procedure returning `FOR JSON` directly would be preferred — the scalar function is used here because it's an explicit requirement of the assessment.

### Stored procedure — `sp_InsertProduct`

```sql
CREATE PROCEDURE dbo.sp_InsertProduct
    @StoreId     UNIQUEIDENTIFIER,
    @Sku         NVARCHAR(50),
    @Name        NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @Price       DECIMAL(18,2),
    @Currency    CHAR(3) = 'BRL',
    @StockQty    INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Products (StoreId, Sku, Name, Description, Price, Currency, StockQty)
    VALUES (@StoreId, @Sku, @Name, @Description, @Price, @Currency, @StockQty);

    SELECT * FROM Products WHERE ProductId = SCOPE_IDENTITY();
END
```

---

## Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/products` | Creates a Product (uses `sp_InsertProduct`) |
| `GET` | `/api/products` | Lists products (uses `fn_GetProductsAsJson`); accepts an optional `?storeId=` |
| `GET` | `/api/products/item/{id}` | Retrieves a Product by id |
| `PUT` | `/api/products/item/{id}` | Updates a Product (`Sku` and `StoreId` are immutable) |
| `DELETE` | `/api/products/item/{id}` | Deletes a Product (hard delete) |

All endpoints return `404` when the given `id` does not exist.

In this version, the endpoints are published as `AuthorizationLevel.Anonymous` — see [Known limitations](#known-limitations).

---

## Running locally

Prerequisites: .NET SDK, Azure Functions Core Tools (`func`), access to the Azure SQL database (`StoreManagementDb`) with your IP allowed through the firewall.

1. Configure `local.settings.json` (not committed) with the SQL Server connection string:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "DefaultConnection": "Server=tcp:<your-server>.database.windows.net,1433;Database=StoreManagementDb;User ID=<user>;Password=<password>;Encrypt=true;"
     }
   }
   ```
2. Restore and run:
   ```bash
   dotnet restore
   func start
   ```
3. Test the endpoints locally (e.g. `http://localhost:7071/api/products`) via Postman or `curl`.

> A `Product` requires a valid `StoreId` (mandatory FK). Create a Store first through the Stores API, or use an existing `storeId` from the database.

---

## Tests

```bash
dotnet test tests/ProductManagement.Tests
```

Tests cover the Application layer services (business rules) and, where present, integration tests of the Functions through simulated HTTP calls.

The commit history follows the `test:` → `feat:` → `refactor:` convention, evidencing the TDD cycle (Red → Green → Refactor) step by step, instead of a single commit per feature.

---

## Deployment (staging → swap → production)

Deployment runs through an Azure DevOps pipeline (`azure-pipelines-functions.yml`), in three stages:

1. **Build** — restore, build, run the automated tests (fails the pipeline if any test breaks), and publish the artifact.
2. **DeployStaging** — publishes the artifact to the `staging` slot of the Function App.
3. **SwapToProduction** — swaps the `staging` slot into `production`.

Stages 2 and 3 use the Azure DevOps environments `staging` and `production` respectively — named to match exactly what each stage does, so the run history clearly shows which deployment went to which environment.

Post-deploy validation:
- `https://<function-app>-staging.azurewebsites.net/api/products` — before the swap
- `https://<function-app>.azurewebsites.net/api/products` — after the swap, in production

---

## Postman collection

This service's endpoints are part of the `Backend-Developer-Hiring-Assessment.postman_collection.json` collection, under the **Products** folder. The folder does not require authentication and depends on a valid `storeId`, automatically generated by the `Stores > Create Store` request in the same collection.

---

## Design decisions

- **Guid as the identifier type**: consistent with the rest of the system (`Companies`, `Stores`).
- **`Sku` and `StoreId` immutable on update**: changing a product's store or SKU is treated as a different operation (recreating the product), not a partial update.
- **Physical delete (hard delete)**: no soft delete in this version — removed products are permanently gone from the table.

---

## Known limitations

- **No authentication on the Products endpoints** (`AuthorizationLevel.Anonymous`). A natural next step would be propagating the same `companyId`/JWT claim used by the Stores API, validating that the given `storeId` actually belongs to the authenticated company.
- **Scalar function for listing**: kept because it's an explicit requirement, with the performance caveat already noted in the database section.
- **Deployment Slots depend on the Azure Function App plan**: the Consumption plan does not support slots — a Premium (Elastic Premium) or Flex Consumption plan is required. Document this if the subscription in use does not support the required plan.