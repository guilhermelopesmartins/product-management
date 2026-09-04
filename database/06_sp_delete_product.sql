-- =============================================================================
-- 06_sp_delete_product.sql
-- Parte 2 — Microsserviço de Produtos (Azure Function + Dapper)
--
-- Cria dbo.sp_DeleteProduct, usada pelo endpoint DELETE /api/products/item/{id}.
--
-- Decisão de design: hard delete (remoção física), não soft delete. O
-- enunciado não exige exclusão lógica para Products (diferente de Stores,
-- que usa IsActive na Parte 1). Retorna a contagem de linhas afetadas, para
-- o chamador diferenciar "removido" (1) de "não encontrado" (0).
--
-- Execução:
--   sqlcmd -S sql-backend-challenge-guilhermehiringprocess123.database.windows.net ^
--          -d StoreManagementDb -U sqladmin -P "<senha>" -i .\06_sp_delete_product.sql
-- =============================================================================

IF OBJECT_ID(N'dbo.sp_DeleteProduct', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DeleteProduct;
GO

CREATE PROCEDURE dbo.sp_DeleteProduct
    @ProductId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Products
    WHERE ProductId = @ProductId;

    SELECT @@ROWCOUNT AS DeletedCount;
END
GO

PRINT 'Procedure sp_DeleteProduct criada com sucesso.';
GO
