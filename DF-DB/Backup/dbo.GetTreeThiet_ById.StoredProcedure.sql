/****** Object:  StoredProcedure [dbo].[GetTreeThiet_ById]    Script Date: 16/06/2016 11:45:07 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetTreeThiet_ById]
(
@EquiqmentId varchar(200),
@IsContruction bit
)
AS
BEGIN
	DECLARE @ThietBiID uniqueidentifier
	SET @ThietBiID = CONVERT(uniqueidentifier, @EquiqmentId)
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF(@IsContruction = 'false')
	BEGIN
		SELECT * FROM
		(
		SELECT DISTINCT 
			Id = CONVERT(NVARCHAR(50), TB.ThietBiId)
			,Name = TB.TenThietBi
			,Father = CONVERT(NVARCHAR(50), CT.CongTrinhId)
			,Cap = 1
			,TB.LatitudeHienTai
			,TB.LongtitudeHienTai
		FROM ThietBi TB
		JOIN KhoCongTrinh KCT
		ON TB.KhoId = KCT.KhoId
		JOIN CongTrinh CT
		ON KCT.CongTrinhId = CT.CongTrinhId
		AND CT.Dept = 0
		UNION ALL
		SELECT DISTINCT
			Id = CONVERT(NVARCHAR(50), CT.CongTrinhId)
			,Name = CT.TenCongTrinh
			,Father = ''
			,Cap = 0
			,null
			,null
		FROM CongTrinh CT
		WHERE CT.Dept = 0) AS TBL
		WHERE Id = @ThietBiID 
	END
	ELSE
	BEGIN
			SELECT * FROM
		(
		SELECT DISTINCT 
			Id = CONVERT(NVARCHAR(50), TB.ThietBiId)
			,Name = TB.TenThietBi
			,Father = CONVERT(NVARCHAR(50), CT.CongTrinhId)
			,Cap = 1
			,TB.LatitudeHienTai
			,TB.LongtitudeHienTai
		FROM ThietBi TB
		JOIN KhoCongTrinh KCT
		ON TB.KhoId = KCT.KhoId
		JOIN CongTrinh CT
		ON KCT.CongTrinhId = CT.CongTrinhId
		AND CT.Dept = 0
		UNION ALL
		SELECT DISTINCT
			Id = CONVERT(NVARCHAR(50), CT.CongTrinhId)
			,Name = CT.TenCongTrinh
			,Father = ''
			,Cap = 0
			,null
			,null
		FROM CongTrinh CT
		WHERE CT.Dept = 0) AS TBL
		WHERE Father = @ThietBiID 
	END
	
END

GO
