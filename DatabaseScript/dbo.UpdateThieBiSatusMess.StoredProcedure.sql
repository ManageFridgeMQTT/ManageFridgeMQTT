DROP PROCEDURE [UpdateThieBiSatusMess]
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
--EXEC [dbo].[UpdateThieBiSatusMess] '7000000009', '', '', '', 3, 1, '', 4, 21.0563052000, 105.7814181000
CREATE PROCEDURE [UpdateThieBiSatusMess]
	@strThietBiID NVARCHAR(max)
	,@CommandType NVARCHAR(200)
	,@CommandId NVARCHAR(200)
	,@CommandAction NVARCHAR(200)
	,@Loai INT
	,@StatusMay INT
	,@Time FLOAT
	,@TrangThaiHienTai INT
	,@LatitudeHienTai DECIMAL(18, 10)
	,@LongtitudeHienTai DECIMAL(18, 10)
AS
BEGIN

IF EXISTS (SELECT 1 FROM [dbo].[DeviceThietBi] WHERE DeviceId = @strThietBiID)
BEGIN
	DECLARE @ThietBiID uniqueidentifier
	SELECT TOP 1 @ThietBiID = CONVERT(uniqueidentifier, ThietBiId)
		FROM [dbo].[DeviceThietBi] 
		WHERE DeviceId = @strThietBiID
	
	If(@Loai != 1)
	BEGIN
		If(@StatusMay = 0)
		BEGIN
			UPDATE [dbo].[ThietBi] 
				SET TrangThaiHienTai = @TrangThaiHienTai
				WHERE ThietBi.ThietBiId = @ThietBiID
		END
		ELSE
		BEGIN
			DECLARE @ThoiGianNoMay INT

			SELECT TOP 1 @ThoiGianNoMay = DATEDIFF(SS, [ThoiGian], GETDATE())
			  FROM [DF_RELEASE].[dbo].[DeviceStatusMess]
			  WHERE ThietBiID = @strThietBiID
			  ORDER BY ThoiGian DESC


			UPDATE [dbo].[ThietBi] 
			SET LatitudeHienTai = @LatitudeHienTai
				,LongtitudeHienTai = @LongtitudeHienTai
				,TrangThaiHienTai = @TrangThaiHienTai
				,ThoiGianHoatDong = @ThoiGianNoMay
			WHERE ThietBi.ThietBiId = @ThietBiID
		END
	END

END

	INSERT INTO [dbo].[DeviceStatusMess] 
	VALUES (
		GETDATE()
		,@strThietBiID
		,@CommandType
		,@CommandId
		,@CommandAction
		,@Loai
		,@TrangThaiHienTai
		,@StatusMay
		,@Time
		,@LatitudeHienTai
		,@LongtitudeHienTai
		);
END



GO
