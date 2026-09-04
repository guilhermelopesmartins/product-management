-- =============================================================================
-- 05_sp_update_product.sql
-- Parte 2 — Microsserviço de Produtos (Azure Function + Dapper)
--
-- Cria dbo.sp_UpdateProduct, usada pelo endpoint PUT /api/products/item/{id}.
--
-- Decisão de design: Sku e StoreId são imutáveis após a criação do produto —
-- não fazem parte do payload de atualização. Se precisar trocar de loja ou
-- SKU, o fluxo esperado é excluir e recriar o produto.
--
-- Retorna zero ou uma linha (via OUTPUT) — o chamador trata "não encontrado"
-- como 404 quando nenhuma linha é retornada.
--
-- Execução:
--   sqlcmd -S sql-backend-challenge-guilhermehiringprocess123.database.windows.net ^
--          -d StoreManagementDb -U sqladmin -P "<senha>" -i .\05_sp_update_product.sql
-- =============================================================================

IF OBJECT_ID(N'dbo.sp_UpdateProduct', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateProduct;
GO

CREATE PROCEDURE dbo.sp_UpdateProduct
    @ProductId   UNIQUEIDENTIFIER,
    @Name        NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @Price       DECIMAL(18,2),
    @Currency    CHAR(3),
    @StockQty    INT,
    @IsActive    BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UpdatedRows TABLE (
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

    UPDATE dbo.Products
    SET
        Name        = @Name,
        Description = @Description,
        Price       = @Price,
        Currency    = @Currency,
        StockQty    = @StockQty,
        IsActive    = @IsActive,
        UpdatedAt   = SYSUTCDATETIME()
    OUTPUT
        inserted.ProductId, inserted.StoreId, inserted.Sku, inserted.Name,
        inserted.Description, inserted.Price, inserted.Currency,
        inserted.StockQty, inserted.IsActive, inserted.CreatedAt, inserted.UpdatedAt
    INTO @UpdatedRows
    WHERE ProductId = @ProductId;

    SELECT * FROM @UpdatedRows;
END
GO

PRINT 'Procedure sp_UpdateProduct criada com sucesso.';
GO
