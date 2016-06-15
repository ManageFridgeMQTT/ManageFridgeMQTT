DROP PROCEDURE [GetTreeThietBi]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [GetTreeThietBi]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT DISTINCT 
		Id = CONVERT(NVARCHAR(50), TB.ThietBiId)
		,Name = TB.TenThietBi
		,Father = CONVERT(NVARCHAR(50), CT.CongTrinhId)
		,Cap = 1
	FROM ThietBi TB
	JOIN KhoCongTrinh KCT
	ON TB.KhoId = KCT.KhoId
	JOIN CongTrinh CT
	ON KCT.CongTrinhId = CT.CongTrinhId
	UNION ALL
	SELECT DISTINCT
		Id = CONVERT(NVARCHAR(50), CT.CongTrinhId)
		,Name = CT.TenCongTrinh
		,Father = ''
		,Cap = 0
	FROM CongTrinh CT
END

GO
