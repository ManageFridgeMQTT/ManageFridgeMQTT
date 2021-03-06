DROP PROCEDURE [Sp_GetInfoDeviceByIdAll]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================

-- =============================================
--EXEC [dbo].[Sp_GetInfoDeviceByIdAll] 'C18CEB2F-EF65-4034-B145-123DFA232204'
CREATE PROCEDURE [Sp_GetInfoDeviceByIdAll] 
AS
BEGIN
	SET NOCOUNT ON;

SELECT DISTINCT
	ROW_NUMBER() OVER (
							PARTITION BY 
									TB.ThietBiId
							ORDER BY (select 1)
					) AS STT
	,HinhAnhAvatar = ISNULL(TBAvatar.TenFile,'NULL.JPG')
	,ThietBiId = CONVERT(NVARCHAR(250), TB.ThietBiId)
	,CT.TenCongTrinh
	,CT.DiaDiem
	,TB.TenVietTat
	,TB.TenThietBi
	,TB.TrangThai
	,strTinhTrang = [dbo].[fnTuSoRaMoTa](TB.TrangThai,'TrangThai')
	,TrangThaiHienTai = ISNULL(TB.TrangThaiHienTai, 10)
	,StrTrangThai = [dbo].[fnTuSoRaMoTa](TB.TrangThaiHienTai,'TrangThaiHienTai')
	,DaHoatDong = CONVERT(NVARCHAR, DATEADD(ss, 10000, 0), 108)
	,LoaiHinh = CASE WHEN TB.LoaiHinh = 1 THEN N'Máy Thuê' ELSE N'Máy Công Ty' END  --1 là máy thuê, 0 là máy công ty
	,ThoiGianVao = [dbo].[fnFormatDateTime](DC.ThoiGianVao) -- Thời gian thiết bị đến công trình
	,NgayOCongTrinh = CONVERT(NVARCHAR(50), CASE WHEN DC.NgayOCongTrinh > 0 THEN DC.NgayOCongTrinh ELSE 0 END) + ' Ngày'
	,GiaThue = [dbo].[FormatNumber](CASE WHEN TB.GiaThue > 0 THEN TB.GiaThue ELSE 0 END)
	,ThanhTien = [dbo].[FormatNumber](CASE WHEN TB.GiaThue > 0 AND DC.NgayOCongTrinh > 0 THEN TB.GiaThue * DC.NgayOCongTrinh / 30 ELSE 0 END)

	--Chưa biết
	,ThayDauLanCuoi = ''--GETDATE()
	--Chưa biết
	,ThayLocLanCuoi = ''--GETDATE()

	,SoGioThayDau = CONVERT(NVARCHAR(250), TB.DinhKyThayDau)
	,SoGioThayLoc = CONVERT(NVARCHAR(250), TB.DinhKyThayLoc)

	,ThayDauTiep = CASE WHEN TB.DinhKyThayDau IS NOT NULL AND TB.ThoiGianHoatDong IS NOT NULL AND TB.DinhKyThayDau > 0 
					THEN TB.DinhKyThayDau - TB.ThoiGianHoatDong 
					ELSE 0 END
	,ThayLocTiep = CASE WHEN TB.DinhKyThayLoc IS NOT NULL AND TB.ThoiGianHoatDong IS NOT NULL AND TB.DinhKyThayLoc > 0 
					THEN TB.DinhKyThayLoc - TB.ThoiGianHoatDong 
					ELSE 0 END
	
	,Latitude = ISNULL(TB.LatitudeHienTai,0)
	,Longitude = ISNULL(TB.LongtitudeHienTai,0)
FROM [dbo].[ThietBi] TB WITH (NOLOCK)
LEFT JOIN dbo.KhoCongTrinh KCT WITH (NOLOCK)
ON TB.KhoId = KCT.KhoId
LEFT JOIN [dbo].[CongTrinh] CT WITH (NOLOCK)
ON CT.CongTrinhId = KCT.CongTrinhId
LEFT JOIN 
(
	SELECT DISTINCT VAO.[ThietBiId]
		,VAO.[KhoId]
		,VAO.[ThoiGian] AS ThoiGianVao-- Thời gian vào
		,RA.[ThoiGian] AS ThoiGianRa
		,NgayOCongTrinh = DATEDIFF(day,VAO.[ThoiGian],ISNULL(RA.ThoiGian, GETDATE()))
	FROM 
	(
		SELECT [ThietBiId]
				,[KhoId]
				,[ThoiGian]
			FROM
			(
				SELECT [ThietBiId]
					,[KhoId]
					,[ThoiGian]
					,ROW_NUMBER() OVER (
									PARTITION BY 
									ThietBiId 
									,KhoId 
									ORDER BY [ThoiGian] DESC
									) AS RN
				FROM [dbo].[TrangThaiThietBi]
				WHERE LoaiHinhBC = 5
					AND TrangThaiDiChuyen = 1
			) AS TB
			WHERE RN = 1
		) AS VAO
		LEFT JOIN 
		(
			SELECT [ThietBiId]
				,[KhoId]
				,[ThoiGian]
			FROM
			(
				SELECT [ThietBiId]
					,[KhoId]
					,[ThoiGian]
					,ROW_NUMBER() OVER (
									PARTITION BY 
									ThietBiId 
									,KhoId 
									ORDER BY [ThoiGian] DESC
									) AS RN
				FROM [dbo].[TrangThaiThietBi]
				WHERE LoaiHinhBC = 5
					AND TrangThaiDiChuyen = 2
			) AS TB
			WHERE RN = 1
		) AS RA
		ON VAO.ThietBiId = RA.ThietBiId
			AND VAO.KhoId = RA.KhoId
) AS DC
ON TB.ThietBiId = DC.ThietBiId
	AND TB.KhoId = DC.KhoId
LEFT JOIN
(
	SELECT DISTINCT
		HA.TenFile
		,TT.ThietBiId 
		--,HA.HaTrangThaiThietBiId
		--,HA.TrangThaiThietBiId
		,ROW_NUMBER() OVER (
						PARTITION BY 
							TT.ThietBiId 
						ORDER BY TT.ThoiGian DESC
					) AS RN
	FROM 
		[dbo].[TrangThaiThietBi] TT WITH (NOLOCK)-- ORDER BY TT.ThietBiId
		JOIN [dbo].[HaTrangThaiThietBi] HA WITH (NOLOCK)
			ON TT.TrangThaiThietBiId = HA.[TrangThaiThietBiId]
	WHERE
		--TT.TinhTrang = 0
		--AND TT.TrangThaiDiChuyen IS NULL
		--AND 
		TT.KhoID = CONVERT(uniqueidentifier, 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF')
) AS TBAvatar
	ON  TB.ThietBiId = TBAvatar.ThietBiId
END



GO
