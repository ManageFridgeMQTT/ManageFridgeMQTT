/****** Object:  UserDefinedFunction [dbo].[FormatNumber]    Script Date: 16/06/2016 11:45:07 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[FormatNumber]
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
