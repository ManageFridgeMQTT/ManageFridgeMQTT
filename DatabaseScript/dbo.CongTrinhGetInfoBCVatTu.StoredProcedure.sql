DROP PROCEDURE [CongTrinhGetInfoBCVatTu]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoBCVatTu] '', '2011-01-01', '2017-01-01'
CREATE PROCEDURE [CongTrinhGetInfoBCVatTu]
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
	   CT.CongTrinhId,
	   VT.VatTuID,
	   ct.TenCongTrinh,
       vt.TenVatTu,
       [dbo].[FormatNumberDecimal](Sum(ctpnk.SoLuong)) AS SoLuong,
       [dbo].[FormatNumberDecimal](gbvt.GiaBan) AS GiaBan,
       [dbo].[FormatNumberDecimal](sum(ctpnk.SoLuong)*gbvt.GiaBan) AS ThanhTien,
       Tien = sum(ctpnk.SoLuong)*gbvt.GiaBan
FROM 
	ChiTietXuatNhapKho AS ctpnk
	INNER JOIN PhieuNhapKho AS pnk 
		ON pnk.PhieuNhapKhoID = ctpnk.PhieuNhapKhoID
		AND pnk.ThoiGian >= @tngay
		AND pnk.ThoiGian <= @dngay
	INNER JOIN VatTu AS vt 
		ON vt.VatTuID = ctpnk.VatTuID
	INNER JOIN KhoCongTrinh AS kct 
		ON kct.KhoId = pnk.KhoID
	INNER JOIN GiaBanVatTu AS gbvt 
		ON gbvt.KhoID = kct.KhoId
		AND vt.VatTuID = gbvt.VatTuID
	INNER JOIN CongTrinh AS ct 
		ON ct.CongTrinhId = kct.CongTrinhId
WHERE kct.CongTrinhId = @_CongTrinhId
GROUP BY 
		 CT.CongTrinhId,
	     VT.VatTuID,
		 vt.TenVatTu,
         gbvt.GiaBan,
         ct.TenCongTrinh
ORDER BY vt.TenVatTu

END





GO
