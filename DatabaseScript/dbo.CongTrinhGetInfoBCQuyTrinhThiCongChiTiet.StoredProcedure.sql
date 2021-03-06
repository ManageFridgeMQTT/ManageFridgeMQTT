DROP PROCEDURE [CongTrinhGetInfoBCQuyTrinhThiCongChiTiet]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoBCQuyTrinhThiCongChiTiet] '', '', '2011-01-01', '2017-01-01'
CREATE PROCEDURE [CongTrinhGetInfoBCQuyTrinhThiCongChiTiet]
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


SELECT DISTINCT 
	   tcc.CocID,
	   tcc.ThiCongCocID,
       tc.ThiCongID,
       tc.ThiCong,
       ThoiGianThiCong = dbo.FormatNumber(tcc.ThoiGianThiCong),
       ThoiGian = dbo.[fnFormatDateTime](tcc.ThoiGian),
       tcc.GhiChu,
	   tc.ThuTu
FROM ThiCong tc
LEFT JOIN ThiCongCoc tcc 
	ON tcc.ThiCongID = tc.ThiCongID
WHERE 
	tcc.[CocID] = @_CocId
	--AND co.[CongTrinhID] = @_CongTrinhId
ORDER BY tc.ThuTu

END





GO
