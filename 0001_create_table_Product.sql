CREATE TABLE [dbo].[Product]
(
	[ProductId]			[int] 			IDENTITY(1,1) NOT FOR REPLICATION,
	[ProductName]			[nvarchar](100)		NOT NULL,
	[UnitPrice]			[decimal](12,2)		NOT NULL,

	CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED
	(
		[ProductId]					ASC
	)
)
GO