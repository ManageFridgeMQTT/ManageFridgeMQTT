DROP PROCEDURE [CongTrinhGetInfoDSNhanVien]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoDSNhanVien] '', '2011-01-01', '2017-01-01'
CREATE PROCEDURE [CongTrinhGetInfoDSNhanVien]
	@CongTrinhId NVARCHAR(500)
	,@FromDate DATE
	,@ToDate DATE
AS
BEGIN
	--DECLARE @_CongTrinhId uniqueidentifier
	--SET @_CongTrinhId = CONVERT(uniqueidentifier, @CongTrinhId)
	SET NOCOUNT ON;

DECLARE @tngay DATE = '2011-01-01'
DECLARE @dngay DATE = '2017-01-01'

SELECT DISTINCT
	NV.NhanVienID,
	NV.TenNhanVien,
	NV.ChucVu,
	NgaySinh = [dbo].[fnFormatDate](NV.NgaySinh),
	GioiTinh = CASE WHEN NV.GioiTinh = 1 THEN N'Nữ' ELSE 'Nam' END,
	Mobile = NV.SDT
FROM
	NhanVien NV
	JOIN [dbo].[NhanVienCongTrinh] NVCT
		ON NV.NhanVienID = NVCT.NhanVienId
WHERE
	NVCT.CongTrinhId = @CongTrinhId
ORDER BY NV.TenNhanVien
END





GO
