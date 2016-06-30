/****** Object:  StoredProcedure [dbo].[GetInfoDeviceActivity]    Script Date: 30/06/2016 10:19:53 CH ******/
DROP PROCEDURE [dbo].[GetInfoDeviceActivity]
GO
/****** Object:  StoredProcedure [dbo].[GetInfoDeviceActivity]    Script Date: 30/06/2016 10:19:53 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Thuan Tran
-- Create date: 2016-04-28
-- Description:	<Description,,>
-- =============================================
--EXEC [GetInfoDeviceActivity] 'C18CEB2F-EF65-4034-B145-123DFA232204', '20160530', '20160630'
CREATE PROCEDURE [dbo].[GetInfoDeviceActivity]
	@strThietBiID NVARCHAR(Max)
	,@FromDate DATETIME
	,@ToDate DATETIME
AS
BEGIN
	DECLARE @ThietBiID uniqueidentifier
	SET @ThietBiID = CONVERT(uniqueidentifier, @strThietBiID)
	SELECT TOP 200
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
	FROM
		[dbo].[DeviceStatusMess] SC
		LEFT JOIN dbo.DeviceThietBi DT
		ON SC.ThietBiID = DT.DeviceId
		LEFT JOIN dbo.ThietBi TB 
		ON DT.ThietBiId = TB.ThietBiId
	WHERE 
		TB.ThietBiID = CONVERT(uniqueidentifier, @ThietBiID)
		AND (ISNULL(@FromDate, '') = '' OR CONVERT(Date, SC.ThoiGian) >= CONVERT(Date, @FromDate))
		AND (ISNULL(@ToDate, '') = '' OR CONVERT(Date, SC.ThoiGian) <= CONVERT(Date, @ToDate))
	ORDER BY SC.ThoiGian DESC
END





GO
