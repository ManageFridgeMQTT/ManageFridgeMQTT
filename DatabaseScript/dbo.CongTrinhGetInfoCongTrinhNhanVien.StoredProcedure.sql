DROP PROCEDURE [CongTrinhGetInfoCongTrinhNhanVien]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoCongTrinhNhanVien] '', '2016-07-02' 
CREATE PROCEDURE [CongTrinhGetInfoCongTrinhNhanVien]
	@CongTrinhId NVARCHAR(500)
	,@FromDate DATE
AS
BEGIN
	DECLARE @_CongTrinhId uniqueidentifier
	SET @_CongTrinhId = CONVERT(uniqueidentifier, @CongTrinhId)
	SET NOCOUNT ON;
	SELECT DISTINCT
		--Thong tin cong trinh
		[CongTrinhId] = CONVERT(NVARCHAR(500), CT.CongTrinhID)  --'1C6E5DD8-E3F9-4054-BD22-0B07A99900A3'
		,[TenCongTrinh] = CT.[TenCongTrinh]
		,[TinhTrang] = CT.[TinhTrang]
		,TrangThai = CT.TrangThai
		,DiaDiem = CT.DiaDiem
		,LatitudeCongTrinh = ISNULL(CT.Latitude,'0')
		,LongtitudeCongTrinh = ISNULL(CT.Longitude,'0')

		--Thong tin nhan vien
		,NV.NhanVienID
		,NV.[TenNhanVien]
		,NV.[SDT]
		,NV.[Email]
		,LatitudeNhanVien = ISNULL(TDNV.Latitude,0)
		,LongtitudeNhanVien = ISNULL(TDNV.[Longitude],0)
	FROM 
		[dbo].[CongTrinh] AS CT
		JOIN [NhanVienCongTrinh] AS NVCT
			ON CT.[CongTrinhId] = NVCT.CongTrinhId
		JOIN [dbo].[NhanVien] AS NV
			ON NV.NhanVienID = NVCT.NhanVienId
		JOIN [dbo].ToaDoNhanVien AS TDNV
			ON NV.NhanVienID = TDNV.NhanVienID
END





GO
