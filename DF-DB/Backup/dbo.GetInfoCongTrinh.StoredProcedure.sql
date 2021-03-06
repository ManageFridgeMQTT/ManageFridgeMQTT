/****** Object:  StoredProcedure [dbo].[GetInfoCongTrinh]    Script Date: 16/06/2016 11:45:07 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetInfoCongTrinh]
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
  FROM [dbo].[CongTrinh]
  WHERE (ISNULL(@NameCongTring, '') = ''  OR  [TenCongTrinh] like @NameCongTring)
END

GO
