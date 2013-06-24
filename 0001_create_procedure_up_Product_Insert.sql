CREATE PROCEDURE [dbo].[up_Product_Insert]
(
	@ProductName			[nvarchar](100),
	@UnitPrice			[decimal](12,2)
)
AS

BEGIN TRY
	BEGIN TRANSACTION trans_Product_Insert

	IF (@ProductName IS NULL OR LTRIM(@ProductName) = '')
		BEGIN
		RAISERROR('@ProductName cannot be null or empty.', 16, 1)
		END

	INSERT INTO [dbo].[Product]
	(
		[ProductName],
		[UnitPrice]
	)
	VALUES
	(
		@ProductName,
		@UnitPrice
	)

	COMMIT TRANSACTION trans_Product_Insert

	SELECT	SCOPE_IDENTITY() AS [ProductId]
END TRY

BEGIN CATCH
	
	ROLLBACK TRANSACTION trans_Product_Insert
	
	SELECT		ERROR_NUMBER(), 
			ERROR_SEVERITY(), 
			ERROR_STATE(), 
			ERROR_PROCEDURE(),
			ERROR_MESSAGE()

END CATCH
GO