DROP PROCEDURE [CongTrinhGetInfoBCThietbi]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoBCThietbi] '', '2011-01-01', '2017-01-01'
CREATE PROCEDURE [CongTrinhGetInfoBCThietbi]
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
--<th>#</th>
--<th>Tên thiết bị</th>
--<th>Đơn vị thuê</th>
--<th>Ngày đến</th>
--<th>Ngày đi</th>
--<th>Đơn giá</th>
--<th>Số ngày</th>
--<th>Số tiền</th>
--<th>Tổng ngày ở CT</th>
--<th>Tổng Số tiền</th>


SELECT DISTINCT
	   tb.ThietBiId
	   ,kct.CongTrinhID
	   ,tb.TenThietBi,
       CASE
           WHEN tb.LoaiHinh = 1 THEN tb.DonViChoThue
           ELSE tb.NhaCungCap
       END AS DonViThue,
       [dbo].[fnFormatDate](tttb.ThoiGian) AS NgayDen,
	   [dbo].[fnFormatDate](tttb.ThoiGian) AS NgayDi,
       DonGia = [dbo].[FormatNumberDecimal](100000),
       SoNgay = [dbo].[FormatNumberDecimal](100000),
       SoTien = [dbo].[FormatNumberDecimal](100000),
	   TongNgayOCongTrinh = [dbo].[FormatNumberDecimal](100000),
	   TongSoTien = [dbo].[FormatNumberDecimal](100000),
	   Tien = 100000
FROM TrangThaiThietBi AS tttb
INNER JOIN KhoCongTrinh AS kct ON kct.KhoId = tttb.KhoId
INNER JOIN ThietBi AS tb ON tb.ThietBiId = tttb.ThietBiId
WHERE 
	tttb.LoaiHinhBC = 4
	AND tttb.ThoiGian >= @tngay
	AND tttb.ThoiGian <= @dngay
	AND kct.CongTrinhId = @_CongTrinhId

END





GO
