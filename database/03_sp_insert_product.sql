-- =============================================================================
-- 03_sp_insert_product.sql
-- Parte 2 — Microsserviço de Produtos (Azure Function + Dapper)
--
-- Cria a stored procedure dbo.sp_InsertProduct, usada pela camada de
-- Infrastructure (Dapper) no endpoint POST /api/products.
--
-- Retorna a linha completa inserida (via OUTPUT), para que a API já devolva
-- o recurso criado (incluindo ProductId gerado e CreatedAt) sem precisar de
-- um SELECT adicional.
--
-- Execução:
--   sqlcmd -S sql-backend-challenge-guilhermehiringprocess123.database.windows.net ^
--          -d StoreManagementDb -U sqladmin -P "<senha>" -i .\03_sp_insert_product.sql
-- =============================================================================

IF OBJECT_ID(N'dbo.sp_InsertProduct', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertProduct;
GO

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

    DECLARE @InsertedRows TABLE (
        ProductId    UNIQUEIDENTIFIER,
        StoreId      UNIQUEIDENTIFIER,
        Sku          NVARCHAR(50),
        Name         NVARCHAR(200),
        Description  NVARCHAR(MAX),
        Price        DECIMAL(18,2),
        Currency     CHAR(3),
        StockQty     INT,
        IsActive     BIT,
        CreatedAt    DATETIME2,
        UpdatedAt    DATETIME2
    );

    INSERT INTO dbo.Products (StoreId, Sku, Name, Description, Price, Currency, StockQty)
    OUTPUT
        inserted.ProductId, inserted.StoreId, inserted.Sku, inserted.Name,
        inserted.Description, inserted.Price, inserted.Currency,
        inserted.StockQty, inserted.IsActive, inserted.CreatedAt, inserted.UpdatedAt
    INTO @InsertedRows
    VALUES (@StoreId, @Sku, @Name, @Description, @Price, @Currency, @StockQty);

    SELECT * FROM @InsertedRows;
END
GO

PRINT 'Procedure sp_InsertProduct criada com sucesso.';
GO
