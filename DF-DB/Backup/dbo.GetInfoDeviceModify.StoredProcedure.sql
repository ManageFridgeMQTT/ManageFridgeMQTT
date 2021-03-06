/****** Object:  StoredProcedure [dbo].[GetInfoDeviceModify]    Script Date: 16/06/2016 11:45:07 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Thuan Tran
-- Create date: 2016-04-28
-- Description:	<Description,,>
-- =============================================
--EXEC [GetInfoDeviceModify] 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF','2016-05-29','2016-05-29'
CREATE PROCEDURE [dbo].[GetInfoDeviceModify]
	@strThietBiID NVARCHAR(Max)
	,@FromDate DATETIME
	,@ToDate DATETIME
AS
BEGIN
	DECLARE @ThietBiID uniqueidentifier
	SET @ThietBiID = CONVERT(uniqueidentifier, @strThietBiID)
	SELECT 
		NgayBaoSua = [dbo].[fnFormatDateTime](SC.NgaySua)
		,NgaySua = [dbo].[fnFormatDateTime](SC.NgaySua)
		,NgayHoanTat = [dbo].[fnFormatDateTime](SC.NgayHoanTat)
		,DonViSua = SC.DonViSuaChua
		,DiaDiemSua = SC.DiaDiemSuaChua
		,MoTa = SC.MoTa
		,ChiPhi = [dbo].[FormatNumber](SC.ChiPhiSuaChua)--CONVERT(NVARCHAR(200), SC.ChiPhiSuaChua)
	FROM [TrangThaiThietBi] SC
	WHERE 
		--SC.ThietBiID = CONVERT(uniqueidentifier, @ThietBiID)
		--AND (@FromDate = '' OR SC.NgaySua >= @FromDate)
		--AND (@ToDate = '' OR SC.NgaySua <= @ToDate)
		--AND 
		SC.NgaySua IS NOT NULL
END


GO
