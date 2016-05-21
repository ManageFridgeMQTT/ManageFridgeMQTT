USE [DF_RELEASE]
GO
/****** Object:  StoredProcedure [dbo].[GetInfoDeviceActivity]    Script Date: 21/05/2016 6:30:43 CH ******/
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
	DECLARE @DeviceID NVARCHAR(50)
	SET @ThietBiID = CONVERT(uniqueidentifier, @strThietBiID)
	SELECT TOP 1 @DeviceID = DeviceId
		FROM [dbo].[DeviceThietBi] 
		WHERE ThietBiId = @ThietBiID

	SELECT SC.ThietBiID
		,SC.ThoiGian
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
	FROM [dbo].[DeviceStatusMess] SC
	WHERE SC.ThietBiID = @DeviceID
		AND (@FromDate = '' OR SC.ThoiGian >= @FromDate)
		AND (@ToDate = '' OR SC.ThoiGian <= @ToDate)
END


GO
