DROP FUNCTION [fnFormatDateTime]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [fnFormatDateTime]
(
	@Date DATETIME
)
RETURNS NVARCHAR(100)
AS
BEGIN
	--19/02/1972 + 
	RETURN CONVERT(NVARCHAR(5), @Date, 103) + ' ' + CONVERT(NVARCHAR(5), @Date, 108) 
END



GO
