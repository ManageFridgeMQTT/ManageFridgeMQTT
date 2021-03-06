DROP PROCEDURE [CongTrinhGetInfoBCSanLuong]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoBCSanLuong] '', '2011-01-01', '2017-01-01'
CREATE PROCEDURE [CongTrinhGetInfoBCSanLuong]
	@CongTrinhId NVARCHAR(500)
	,@FromDate DATE
	,@ToDate DATE
AS
BEGIN
	DECLARE @_CongTrinhId uniqueidentifier
	SET @_CongTrinhId = CONVERT(uniqueidentifier, @CongTrinhId)
	SET NOCOUNT ON;

DECLARE @tngay DATE = @FromDate--'2011-01-01'
DECLARE @dngay DATE = @ToDate--'2017-01-01'

SELECT DISTINCT
	    ct.CongTrinhId,
       ct.TenCongTrinh,
       bcsl.ThoiGian AS [ThoiGian],
       kltc.TenCongViec,
	   kltc.DonViTinh,
       --bcsl.SanLuongChenhLech,
       --
       --bcsl.SanLuong,
       dbo.FormatNumberDecimal(kltc.KhoiLuong) AS KhoiLuong,
       --kltc.DonGia,       
       --kltc.DonGia2,
	   dbo.FormatNumberDecimal(bcsl.SanLuong*kltc.DonGia) AS DaThucHien,
       dbo.FormatNumberDecimal(bcsl.SanLuong*kltc.DonGia2) AS ThanhTien,
       Tien = bcsl.SanLuong*kltc.DonGia2
FROM BaoCaoSanLuong bcsl
INNER JOIN CongTrinh ct ON ct.CongTrinhId = bcsl.CongTrinhID
INNER JOIN KhoiLuongThiCong kltc ON bcsl.KhoiLuongThiCongID = kltc.KhoiLuongThiCongID
WHERE bcsl.ThoiGian >= @tngay
  AND bcsl.ThoiGian <= @dngay -- 
  AND ct.CongTrinhId = @_CongTrinhId 
  --AND ct.CongTrinhID = '31AE38B6-C72A-081F-1F87-6EB72AA9DFB4'
ORDER BY bcsl.ThoiGian DESC

END





GO
