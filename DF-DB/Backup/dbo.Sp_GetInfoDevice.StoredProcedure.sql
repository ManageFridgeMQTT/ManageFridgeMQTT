USE [DF_RELEASE]
GO
/****** Object:  StoredProcedure [dbo].[Sp_GetInfoDevice]    Script Date: 21/05/2016 6:30:43 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Thuan Tran>
-- Create date: <2016/04/27>
-- Description:	<Description,,>
-- =============================================
--EXEC [Sp_GetInfoDevice] 
CREATE PROCEDURE [dbo].[Sp_GetInfoDevice] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
SELECT 
	ROW_NUMBER() OVER (
									PARTITION BY 
									TB.ThietBiId
									ORDER BY (select 1)
									) AS STT
	,TB.ThietBiId
	,CT.TenCongTrinh
	,CT.DiaDiem
	,TB.TenVietTat
	,TB.TenThietBi
	,TB.TrangThai
	,DaHoatDong = CONVERT(NVARCHAR, DATEADD(ss, 10000, 0), 108)
	,LoaiHinh = CASE WHEN TB.LoaiHinh = 1 THEN N'Máy Thuê' ELSE N'Máy Công Ty' END  --1 là máy thuê, 0 là máy công ty
	,DC.ThoiGianVao  -- Thời gian thiết bị đến công trình
	,NgayOCongTrinh = CASE WHEN DC.NgayOCongTrinh > 0 THEN DC.NgayOCongTrinh ELSE 20 END
	,TB.GiaThue
	,ThanhTien = CASE WHEN TB.GiaThue > 0 AND DC.NgayOCongTrinh > 0 THEN TB.GiaThue * DC.NgayOCongTrinh ELSE 0 END
	,ThayDauTiep = CASE WHEN TB.DinhKyThayDau IS NOT NULL AND TB.ThoiGianHoatDong IS NOT NULL AND TB.DinhKyThayDau > 0 THEN TB.DinhKyThayDau - TB.ThoiGianHoatDong ELSE 0 END
	,ThayLocTiep = CASE WHEN TB.DinhKyThayLoc IS NOT NULL AND TB.ThoiGianHoatDong IS NOT NULL AND TB.DinhKyThayLoc > 0 THEN TB.DinhKyThayLoc - TB.ThoiGianHoatDong ELSE 0 END
	,TB.TrangThaiHienTai
	,StrTrangThai = CASE TB.TrangThaiHienTai 
						WHEN 3 THEN N'Đang bảo dưỡng'
						WHEN 4 THEN N'Tắt máy bảo dưỡng'
						WHEN 5 THEN N'Đang di chuyển'
						WHEN 6 THEN N'Tắt máy di chuyển'
						WHEN 7 THEN N'Đang khoan'
						WHEN 8 THEN N'Tắt máy khoan'
						WHEN 9 THEN N'Đang cẩu'
						WHEN 10 THEN N'Tắt máy cẩu'
						ELSE N'Không xác định'
					END 
	,Latitude = TB.LatitudeHienTai
	,Longitude = TB.LongtitudeHienTai
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
END


GO
