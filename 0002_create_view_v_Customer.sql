CREATE VIEW [dbo].[v_Customer]
AS

SELECT	[CustomerID],
		[FullName],
		[DateCreatedUTC]
FROM	[dbo].[Customer]
GO	