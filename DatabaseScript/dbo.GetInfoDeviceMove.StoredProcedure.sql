DROP PROCEDURE [GetInfoDeviceMove]
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
--EXEC [GetInfoDeviceMove] 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF','2016-05-29','2016-05-29'
CREATE PROCEDURE [GetInfoDeviceMove]
	@strThietBiID NVARCHAR(Max)
	,@FromDate DATETIME
	,@ToDate DATETIME
AS
BEGIN
	DECLARE @ThietBiID uniqueidentifier
	SET @ThietBiID = CONVERT(uniqueidentifier, @strThietBiID)
	SELECT DISTINCT TOP 100
		Ngay = [dbo].[fnFormatDateTime](TT.Thoigian)
		,TuViTri = TT.MoTa
		,DenViTri = TT.MoTa
		,TT.Thoigian
	FROM [dbo].[ThietBi] SC WITH (NOLOCK)
		JOIN dbo.TrangThaiThietBi TT WITH (NOLOCK)
			ON SC.ThietBiID = TT.ThietBiID
	WHERE 
		SC.ThietBiID = CONVERT(uniqueidentifier, @ThietBiID)
		AND (@FromDate IS NULL OR SC.NgayNhap >= @FromDate)
		AND (@ToDate IS NULL OR SC.NgayNhap <= @ToDate)
		AND 
		(TT.LoaiHinhBC = 5)
	ORDER BY TT.Thoigian DESC
END





GO
