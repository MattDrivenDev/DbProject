CREATE PROCEDURE [dbo].[up_Customer_Insert]
(
	@FullName			[nvarchar](100)
)
AS

BEGIN TRY
	BEGIN TRANSACTION trans_Customer_Insert

	IF (@FullName IS NULL OR LTRIM(@FullName) = '')
		BEGIN
		RAISERROR('@FullName cannot be null or empty.', 16, 1)
		END

	INSERT INTO [dbo].[Customer]
	(
		[FullName],
		[DateCreatedUTC]
	)
	VALUES
	(
		@FullName,
		GETUTCDATE()
	)

	COMMIT TRANSACTION trans_Customer_Insert

	SELECT	SCOPE_IDENTITY() AS [CustomerId]
END TRY

BEGIN CATCH
	
	ROLLBACK TRANSACTION trans_Customer_Insert
	
	SELECT	ERROR_NUMBER(), 
			ERROR_SEVERITY(), 
			ERROR_STATE(), 
			ERROR_PROCEDURE(),
			ERROR_MESSAGE()

END CATCH
GO