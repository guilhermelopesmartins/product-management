-- =============================================================================
-- 02_fn_get_products_as_json.sql
-- Parte 2 — Microsserviço de Produtos (Azure Function + Dapper)
--
-- Cria a função escalar dbo.fn_GetProductsAsJson, que retorna a lista de
-- produtos (opcionalmente filtrada por StoreId) como uma string JSON.
--
-- Nota: funções escalares em SQL Server têm limitações de performance
-- conhecidas (não fazem bom uso de índices, forçam RBAR em alguns planos).
-- Em produção real, uma view ou procedure com FOR JSON seria preferível,
-- mas o enunciado pede explicitamente uma função escalar.
--
-- Execução:
--   sqlcmd -S sql-backend-challenge-guilhermehiringprocess123.database.windows.net ^
--          -d StoreManagementDb -U sqladmin -P "<senha>" -i .\02_fn_get_products_as_json.sql
-- =============================================================================

IF OBJECT_ID(N'dbo.fn_GetProductsAsJson', N'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetProductsAsJson;
GO

CREATE FUNCTION dbo.fn_GetProductsAsJson (@StoreId UNIQUEIDENTIFIER = NULL)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @Result NVARCHAR(MAX);

    SELECT @Result = (
        SELECT
            ProductId,
            StoreId,
            Sku,
            Name,
            Price,
            Currency,
            StockQty,
            IsActive
        FROM dbo.Products
        WHERE (@StoreId IS NULL OR StoreId = @StoreId)
        FOR JSON PATH
    );

    RETURN ISNULL(@Result, '[]');
END
GO

PRINT 'Função fn_GetProductsAsJson criada com sucesso.';
GO
