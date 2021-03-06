DROP FUNCTION [FormatNumberDecimal]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [FormatNumberDecimal]
(
   @unFormattedNumber DECIMAL(26,10)
)
 RETURNS NVARCHAR(200)
  AS
 BEGIN
   DECLARE @val NVARCHAR(200)
   SET @val = CONVERT(NVARCHAR, CAST(@unFormattedNumber AS MONEY), 1)
   RETURN @val--(SELECT left(@val, len(@val) - 3))
 END



GO
