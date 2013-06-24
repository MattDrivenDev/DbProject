CREATE TABLE [dbo].[ProductImage]
(
	[ProductImageId]			[int]			IDENTITY(1,1) NOT FOR REPLICATION,
	[ProductId]				[int]			NOT NULL,
	[ImageData]				[image]			NOT NULL,

	CONSTRAINT [PK_ProductImage] PRIMARY KEY CLUSTERED
	(
		[ProductImageId]					ASC
	)
)
GO