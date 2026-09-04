-- =============================================================================
-- 04_sp_get_product_by_id.sql
-- Parte 2 — Microsserviço de Produtos (Azure Function + Dapper)
--
-- Cria dbo.sp_GetProductById, usada pelo endpoint GET /api/products/item/{id}.
-- Retorna zero ou uma linha (o chamador trata "não encontrado" como 404).
--
-- Execução:
--   sqlcmd -S sql-backend-challenge-guilhermehiringprocess123.database.windows.net ^
--          -d StoreManagementDb -U sqladmin -P "<senha>" -i .\04_sp_get_product_by_id.sql
-- =============================================================================

IF OBJECT_ID(N'dbo.sp_GetProductById', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetProductById;
GO

CREATE PROCEDURE dbo.sp_GetProductById
    @ProductId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ProductId, StoreId, Sku, Name, Description,
        Price, Currency, StockQty, IsActive, CreatedAt, UpdatedAt
    FROM dbo.Products
    WHERE ProductId = @ProductId;
END
GO

PRINT 'Procedure sp_GetProductById criada com sucesso.';
GO
