-- =============================================================================
-- 01_create_products_table.sql
-- Parte 2 — Microsserviço de Produtos (Azure Function + Dapper)
--
-- Cria a tabela Products no banco StoreManagementDb (Azure SQL).
-- Pré-requisito: tabelas Companies e Stores já existem (criadas via EF Core
-- Migrations na Parte 1). Não recriar essas tabelas aqui.
--
-- FK: Products.StoreId -> Stores.Id (confirmado via INFORMATION_SCHEMA.COLUMNS
-- em 2026-09-04: Stores.Id é uniqueidentifier NOT NULL).
--
-- Execução:
--   sqlcmd -S sql-backend-challenge-guilhermehiringprocess123.database.windows.net ^
--          -d StoreManagementDb -U sqladmin -P "<senha>" -i .\01_create_products_table.sql
-- =============================================================================

IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
BEGIN
    PRINT 'Tabela Products já existe. Nenhuma ação executada.';
END
ELSE
BEGIN
    CREATE TABLE dbo.Products (
        ProductId    UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        StoreId      UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Stores(Id),
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

    -- Índice para consultas frequentes por StoreId (ex: fn_GetProductsAsJson, listagem paginada)
    CREATE INDEX IX_Products_StoreId ON dbo.Products(StoreId);

    -- Evita SKUs duplicados dentro da mesma loja
    CREATE UNIQUE INDEX UX_Products_StoreId_Sku ON dbo.Products(StoreId, Sku);

    PRINT 'Tabela Products criada com sucesso.';
END
GO
