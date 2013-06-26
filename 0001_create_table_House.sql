CREATE TABLE [dbo].[House] (
	[HouseId]			[int]			IDENTITY(1,1),
	[HouseNumber]			[nvarchar](10)		NOT NULL,
	[AddressLine1]			[nvarchar](100)		NOT NULL,
	[AddressLine2]			[nvarchar](100)		NULL,
	[AddressLine3]			[nvarchar](100)		NULL,
	[PostalCode]			[nvarchar](10)		NOT NULL,

	CONSTRAINT [PK_House] PRIMARY KEY CLUSTERED
	(
		[HouseId]					ASC
	)
)
GO