CREATE TABLE [dbo].[Customer]
(
	[CustomerId]		[int]				IDENTITY(1,1) NOT FOR REPLICATION,
	[FullName]			[nvarchar](100)		NOT NULL,
	[DateCreatedUTC]	[datetime]			NOT NULL,

	CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED
	(
		[CustomerId] 						ASC
	)
)
GO