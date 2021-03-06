DROP PROCEDURE [GetInfoDeviceActivity]
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
--EXEC [GetInfoDeviceActivity] 'C18CEB2F-EF65-4034-B145-123DFA232204', '20160530', '20160710'
CREATE PROCEDURE [GetInfoDeviceActivity]
	@strThietBiID NVARCHAR(Max)
	,@FromDate DATETIME
	,@ToDate DATETIME
AS
BEGIN
	DECLARE @ThietBiID uniqueidentifier
	SET @ThietBiID = CONVERT(uniqueidentifier, @strThietBiID)
	SELECT TOP 100
		TB.ThietBiID,
		SC.ThietBiID AS DeviceId


		--3 cột này là 3 cột thông tin chung
		,ThoiGianHoatDong = [dbo].[fnFormatDateTime](GETDATE())
		,LanThayDauTiepTheo = [dbo].[fnFormatDateTime](GETDATE())
		,LanThayLocTiepTheo = [dbo].[fnFormatDateTime](GETDATE())



		,ThoiGian = [dbo].[fnFormatDateTime](SC.ThoiGian)
		,SC.Loai
		,SC.[Time]
		,SC.TrangThai
		,StrTrangThai = CASE SC.TrangThai
						WHEN 1 THEN N'Đang nổ máy'
						WHEN 2 THEN N'Tắt máy'
						WHEN 3 THEN N'Đang bảo dưỡng'
						WHEN 4 THEN N'Tắt máy bảo dưỡng'
						WHEN 5 THEN N'Đang di chuyển'
						WHEN 6 THEN N'Tắt máy di chuyển'
						WHEN 7 THEN N'Đang khoan'
						WHEN 8 THEN N'Tắt máy khoan'
						WHEN 9 THEN N'Đang cẩu'
						WHEN 10 THEN N'Tắt máy cẩu'
						ELSE CASE WHEN SC.TrangThai IS NULL AND StatusMay IS NOT NULL THEN N'Định kỳ GPS' ELSE N'Không xác định' END
					END 
	FROM
		--[dbo].[DeviceStatusMess] SC
		(
			SELECT 
					*,			
					ROW_NUMBER() OVER (
								PARTITION BY 
									SC.ThietBiID 
									,SC.TrangThai 
									,SC.Loai
									,SC.CommandAction
								ORDER BY SC.ThoiGian DESC
							) AS RN
			FROM DeviceStatusMess SC
			WHERE 
				(ISNULL(@FromDate, '') = '' OR CONVERT(Date, SC.ThoiGian) >= CONVERT(Date, @FromDate))
				AND (ISNULL(@ToDate, '') = '' OR CONVERT(Date, SC.ThoiGian) <= CONVERT(Date, @ToDate))
				AND SC.TrangThai IS NOT NULL
		)AS SC
		LEFT JOIN dbo.DeviceThietBi DT
		ON SC.ThietBiID = DT.DeviceId
		LEFT JOIN dbo.ThietBi TB 
		ON DT.ThietBiId = TB.ThietBiId
	WHERE 
		TB.ThietBiID = CONVERT(uniqueidentifier, @ThietBiID)
		--AND RN = 1
	ORDER BY SC.ThoiGian DESC

	--SELECT 
	--		*
	--FROM
	--(
	--	SELECT 
	--			*,			
	--			ROW_NUMBER() OVER (
	--						PARTITION BY 
	--							SC.ThietBiID 
	--							,SC.TrangThai 
	--							,SC.Loai
	--							,SC.CommandAction
	--						ORDER BY SC.ThoiGian DESC
	--					) AS RN
	--	FROM DeviceStatusMess SC
	--)AS SC WHERE RN = 1
END





GO
