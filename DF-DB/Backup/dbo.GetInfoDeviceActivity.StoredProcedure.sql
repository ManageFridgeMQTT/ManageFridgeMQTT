/****** Object:  StoredProcedure [dbo].[GetInfoDeviceActivity]    Script Date: 16/06/2016 11:45:07 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Thuan Tran
-- Create date: 2016-04-28
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetInfoDeviceActivity]
	@strThietBiID NVARCHAR(Max)
	,@FromDate DATETIME
	,@ToDate DATETIME
AS
BEGIN
	DECLARE @ThietBiID uniqueidentifier
	SET @ThietBiID = CONVERT(uniqueidentifier, @strThietBiID)
	SELECT 
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
		INNER JOIN DeviceThietBi TB 
			ON SC.ThietBiID = TB.DeviceId
	--WHERE 
	--	TB.ThietBiID = CONVERT(uniqueidentifier, @ThietBiID)
	--	AND 
	--	(@FromDate is null OR SC.ThoiGian >= @FromDate)
	--	AND (@ToDate is null OR SC.ThoiGian <= @ToDate)
END



GO
