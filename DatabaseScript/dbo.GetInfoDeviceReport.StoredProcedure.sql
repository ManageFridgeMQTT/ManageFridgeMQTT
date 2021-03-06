DROP PROCEDURE [GetInfoDeviceReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Thuan Tran
-- Create date: 2016-04-28
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [GetInfoDeviceReport]
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
			--TT.TinhTrang = 0
			--AND TT.TrangThaiDiChuyen IS NULL
			--AND 
			TT.ThietBiId = @ThietBiID
			AND TT.KhoID = CONVERT(uniqueidentifier, 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF')
		ORDER BY TT.ThoiGian DESC
	)
	

	SELECT DISTINCT TOP 100
		ThietBiID = @ThietBiID
		,ThoiGian = dbo.fnFormatDateTime(TT.ThoiGian)
		,strTrangThai = [dbo].[fnTuSoRaMoTa](TT.TrangThai , 'TrangThai')
		,strTinhTrang = [dbo].[fnTuSoRaMoTa](TT.TinhTrang , 'TinhTrang')
		,GhiChu = MoTa
		,AvatarThietBi = @Avatar--'1c8e6b05-0f47-4021-bfa2-a2e33bbb43cd.jpg'--@Avatar
		--cần lấy 1 list hình cho vào field này
		,HinhAnh = HA.TenFile
	FROM 
		[dbo].[TrangThaiThietBi] TT WITH (NOLOCK) --ORDER BY TT.ThietBiId
		--JOIN [dbo].[HaTrangThaiThietBi] HA WITH (NOLOCK)
		--	ON TT.TrangThaiThietBiId = HA.[TrangThaiThietBiId]
		JOIN (
			SELECT 
				Main.[TrangThaiThietBiId],
				Left(Main.Students,Len(Main.Students)-1) As TenFile
			FROM
				(
					Select distinct ST2.[TrangThaiThietBiId], 
						(
							Select ST1.TenFile + ',' AS [text()]
							From dbo.[HaTrangThaiThietBi] ST1 WITH (NOLOCK)
							Where ST1.[TrangThaiThietBiId] = ST2.[TrangThaiThietBiId]
							ORDER BY ST1.[TrangThaiThietBiId]
							For XML PATH ('')
						) [Students]
					From dbo.[HaTrangThaiThietBi] ST2 WITH (NOLOCK)
				) [Main]
		)AS HA
		ON TT.TrangThaiThietBiId = HA.[TrangThaiThietBiId]
	

	WHERE
		TT.ThietBiId = @ThietBiID
		AND (ISNULL(@FromDate, '') = '' OR CONVERT(Date, TT.ThoiGian) >= CONVERT(Date, @FromDate))
		AND (ISNULL(@ToDate, '') = '' OR CONVERT(Date, TT.ThoiGian) <= CONVERT(Date, @ToDate))
		--AND TT.LoaiHinhBC = 2
END




GO
