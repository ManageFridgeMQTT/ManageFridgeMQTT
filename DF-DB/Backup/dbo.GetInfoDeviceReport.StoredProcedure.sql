/****** Object:  StoredProcedure [dbo].[GetInfoDeviceReport]    Script Date: 16/06/2016 11:45:07 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Thuan Tran
-- Create date: 2016-04-28
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetInfoDeviceReport]
	@strThietBiID NVARCHAR(Max)
	,@FromDate DATETIME
	,@ToDate DATETIME
AS
BEGIN
	DECLARE @ThietBiID uniqueidentifier
	SET @ThietBiID = CONVERT(uniqueidentifier, @strThietBiID)
	DECLARE @Avatar NVARCHAR(250)

	--GET AVATAR
	SET @Avatar = 
	(
		SELECT TOP 1
			--TT.ThietBiId 
			HA.TenFile
			--,HA.HaTrangThaiThietBiId
			--,HA.TrangThaiThietBiId
			--,ROW_NUMBER() OVER (
			--				PARTITION BY 
			--					TT.ThietBiId 
			--				ORDER BY TT.ThoiGian DESC
			--			) AS RN
		FROM 
			[dbo].[TrangThaiThietBi] TT WITH (NOLOCK)-- ORDER BY TT.ThietBiId
			JOIN [dbo].[HaTrangThaiThietBi] HA WITH (NOLOCK)
				ON TT.TrangThaiThietBiId = HA.[TrangThaiThietBiId]
		WHERE
			TT.TinhTrang = 0
			AND TT.TrangThaiDiChuyen IS NULL
			--AND TT.ThietBiId = @ThietBiID
			AND TT.KhoID = CONVERT(uniqueidentifier, 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF')
		ORDER BY TT.ThoiGian DESC
	)
	

	SELECT DISTINCT
		ThietBiID = @ThietBiID
		,ThoiGian = dbo.fnFormatDateTime(TT.ThoiGian)
		,strTrangThai = [dbo].[fnTuSoRaMoTa](TT.TrangThai , 'TrangThai')
		,strTinhTrang = [dbo].[fnTuSoRaMoTa](TT.TinhTrang , 'TinhTrang')
		,GhiChu = MoTa
		,AvatarThietBi = @Avatar
		--cần lấy 1 list hình cho vào field này
		,HinhAnh = HA.TenFile
	FROM 
		[dbo].[TrangThaiThietBi] TT WITH (NOLOCK) --ORDER BY TT.ThietBiId
		JOIN [dbo].[HaTrangThaiThietBi] HA WITH (NOLOCK)
			ON TT.TrangThaiThietBiId = HA.[TrangThaiThietBiId]
	WHERE
		--TT.ThietBiId = @ThietBiID
		--TT.ThoiGian >= @FromDate
		--AND TT.ThoiGian <= @ToDate
		TT.LoaiHinhBC = 2
END


GO
