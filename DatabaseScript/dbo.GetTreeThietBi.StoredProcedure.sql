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
	FROM ThietBi TB WITH (NOLOCK)
	JOIN KhoCongTrinh KCT WITH (NOLOCK)
	ON TB.KhoId = KCT.KhoId
	JOIN CongTrinh CT WITH (NOLOCK)
	ON KCT.CongTrinhId = CT.CongTrinhId
	AND CT.Dept = 0
	AND CT.IsActive = 1
	AND (ISNULL(@StrCongTrinhID,'') = '' OR CT.CongTrinhId = @CongTrinhId)
	UNION ALL
	SELECT DISTINCT
		Id = CONVERT(NVARCHAR(50), CT.CongTrinhId)
		,Name = CT.TenCongTrinh
		,Father = ''
		,Cap = 0
	FROM CongTrinh CT WITH (NOLOCK)
	WHERE
		CT.Dept = 0
		AND CT.IsActive = 1
		AND (ISNULL(@StrCongTrinhID,'') = '' OR CT.CongTrinhId = @CongTrinhId)
END



GO
