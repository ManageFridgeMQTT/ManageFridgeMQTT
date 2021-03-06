DROP FUNCTION [FormatNumber]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [FormatNumber]
(
   @unFormattedNumber DECIMAL(26,10)
)
 RETURNS NVARCHAR(200)
  AS
 BEGIN
   DECLARE @val NVARCHAR(200)
   SET @val = CONVERT(NVARCHAR, CAST(@unFormattedNumber AS MONEY), 1)
   RETURN (SELECT left(@val, len(@val) - 3))
 END



GO
