DROP PROCEDURE [GetInfoCongTrinh]
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
--EXEC [GetInfoCongTrinh] ''
CREATE PROCEDURE [GetInfoCongTrinh]
	@NameCongTring NVARCHAR(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT [CongTrinhId]
      ,[TenCongTrinh]
      ,[NgayKhoiCong]
      ,[NgayKetThuc]
      ,[TrangThai]
      ,[DiaDiem]
      ,Latitude = CASE WHEN CONVERT(float,[Latitude]) IS NOT NULL THEN [Latitude] ELSE CONVERT(float,0) END
      ,Longitude = CASE WHEN CONVERT(float,[Longitude]) IS NOT NULL THEN [Longitude] ELSE CONVERT(float, 0) END
  FROM [dbo].[CongTrinh] WITH (NOLOCK)
  WHERE 
	Dept = 0
	AND IsActive = 1
	AND (ISNULL(@NameCongTring, '') = ''  OR  [TenCongTrinh] like @NameCongTring)
END



GO
