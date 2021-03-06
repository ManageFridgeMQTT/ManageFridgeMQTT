DROP PROCEDURE [CongTrinhGetInfoBCQuyTrinhThiCongCoc]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoBCQuyTrinhThiCongChiTiet] '', '2011-01-01', '2017-01-01'
CREATE PROCEDURE [CongTrinhGetInfoBCQuyTrinhThiCongCoc]
	@CongTrinhId NVARCHAR(500)
	,@CocId NVARCHAR(500)
	,@FromDate DATE
	,@ToDate DATE
AS
BEGIN
	DECLARE @_CongTrinhId uniqueidentifier
	SET @_CongTrinhId = CONVERT(uniqueidentifier, @CongTrinhId)

	DECLARE @_CocId uniqueidentifier
	SET @_CocId = CONVERT(uniqueidentifier, @CocId)

	SET NOCOUNT ON;

DECLARE @tngay DATE = @FromDate--'2011-01-01'
DECLARE @dngay DATE = @ToDate--'2017-01-01'

SELECT TOP 1
	   co.CongTrinhID,
	   co.CocID,
       ThoiGian = [dbo].[fnFormatDateTime](co.ThoiGian),
       co.TenCoc,
       MetDat = [dbo].[fnFormatDateTime](co.MetDat),
       MetDa = [dbo].FormatNumberDecimal(co.MetDa),
	   TongThoiGianThucHien = [dbo].FormatNumberDecimal(co.TongThoiGian),
	   TongGio = [dbo].FormatNumberDecimal(co.TongThoiGian)
FROM Coc co
WHERE 
	co.[CocID] = @_CocId
	AND co.[CongTrinhID] = @_CongTrinhId

END





GO
