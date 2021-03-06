/****** Object:  StoredProcedure [dbo].[GetTreeThietBi]    Script Date: 16/06/2016 11:45:07 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetTreeThietBi]
	@StrCongTrinhID NVARCHAR(250)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @CongTrinhId uniqueidentifier
	IF ISNULL(@StrCongTrinhID,'') = ''   
		SET @CongTrinhId = NULL
	ELSE
		SET @CongTrinhId = CONVERT(uniqueidentifier, @StrCongTrinhID)


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
	AND CT.Dept = 0
	AND (ISNULL(@StrCongTrinhID,'') = '' OR CT.CongTrinhId = @CongTrinhId)
	UNION ALL
	SELECT DISTINCT
		Id = CONVERT(NVARCHAR(50), CT.CongTrinhId)
		,Name = CT.TenCongTrinh
		,Father = ''
		,Cap = 0
	FROM CongTrinh CT
	WHERE
		CT.Dept = 0
		AND (ISNULL(@StrCongTrinhID,'') = '' OR CT.CongTrinhId = @CongTrinhId)
END

GO
