DROP PROCEDURE [CongTrinhGetInfoBCQuyTrinhThiCong]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoBCQuyTrinhThiCong] '', '2011-01-01', '2017-01-01'
CREATE PROCEDURE [CongTrinhGetInfoBCQuyTrinhThiCong]
	@CongTrinhId NVARCHAR(500)
	,@FromDate DATE
	,@ToDate DATE
AS
BEGIN
	DECLARE @_CongTrinhId uniqueidentifier
	SET @_CongTrinhId = CONVERT(uniqueidentifier, @CongTrinhId)
	SET NOCOUNT ON;

DECLARE @tngay DATE = '2011-01-01'
DECLARE @dngay DATE = '2017-01-01'

SELECT 
	   co.CongTrinhID,
	   co.CocID,
       ThoiGian = [dbo].[fnFormatDateTime](co.ThoiGian),
       co.TenCoc,
       MetDat = [dbo].[fnFormatDateTime](co.MetDat),
       MetDa = [dbo].FormatNumberDecimal(co.MetDa),
       CASE
           WHEN tc.ThiCongID = 'fc21c685-2d98-4ea5-879f-3a0bb7394b29' THEN 'Hoàn thành'
           ELSE tc.ThiCong
       END AS ThiCong
FROM Coc co
JOIN ThiCongCoc tcc ON tcc.ThiCongCocID =
  ( SELECT TOP 1 ThiCongCocID
   FROM ThiCongCoc a
   INNER JOIN ThiCong b ON a.ThiCongID = b.ThiCongID
   WHERE a.CocID = co.CocID
   ORDER BY a.ThoiGian DESC, b.ThuTu DESC )
JOIN ThiCong tc ON tc.ThiCongID = tcc.ThiCongID
WHERE 
	co.CongTrinhID = @_CongTrinhId
	AND co.ThoiGian >= @tngay
	AND co.ThoiGian <= @dngay
ORDER BY co.ThoiGian DESC

END





GO
