DROP PROCEDURE [CongTrinhGetInfoQuanLyThuChi]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [CongTrinhGetInfoQuanLyThuChi] '', '2011-01-01', '2017-01-01'
CREATE PROCEDURE [CongTrinhGetInfoQuanLyThuChi]
	@CongTrinhId NVARCHAR(500)
	,@FromDate DATE
	,@ToDate DATE
AS
BEGIN
	--DECLARE @_CongTrinhId uniqueidentifier
	--SET @_CongTrinhId = CONVERT(uniqueidentifier, @CongTrinhId)
	SET NOCOUNT ON;

    SELECT TOP 3
	  [CongTrinhId] = '1C6E5DD8-E3F9-4054-BD22-0B07A99900A3'
	  ,ThuChiName = N'Thu Sản Lượng'
	  ,ThuChiType = CONVERT(BIT,1)--1 là thu, 0 là chi
	  ,ThuChiValue = dbo.FormatNumberDecimal(1000000)
	  ,Tien = 1000000

   --   ,[TenCongTrinh]
   --   ,[TrangThai]
   --   ,[Latitude]
	  --,[Longitude]
	FROM [dbo].[CongTrinh]
	--WHERE CongTrinhID = @_CongTrinhId

	UNION ALL

	 SELECT TOP 5
	  [CongTrinhId] = '1C6E5DD8-E3F9-4054-BD22-0B07A99900A3'
	  ,ThuChiName = N'Chi Vật Tư'
	  ,ThuChiType = CONVERT(BIT,0)--1 là thu, 0 là chi
	  ,ThuChiValue = dbo.FormatNumberDecimal(2000000)
	  ,Tien = 1000000
   --   ,[TenCongTrinh]
   --   ,[TrangThai]
   --   ,[Latitude]
	  --,[Longitude]
	FROM [dbo].[CongTrinh]
	--WHERE CongTrinhID = @_CongTrinhId

END





GO
