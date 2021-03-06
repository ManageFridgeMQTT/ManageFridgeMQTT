DROP PROCEDURE [sp_BLKhauHaoCongCu]
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
CREATE PROCEDURE [sp_BLKhauHaoCongCu]
AS
BEGIN
	SET NOCOUNT ON;
--KhauHaoCongCuID	Uniqueidentifier
--ThoiGian	Date
--CongTrinhID	Uniqueidentifier
--ChiPhi	Float
DECLARE @BaselineDate DATE = GETDATE()

DELETE FROM [KhauHaoCongCu] WHERE [ThoiGian] = @BaselineDate

INSERT INTO [dbo].[KhauHaoCongCu]
           ([KhauHaoCongCuID]
           ,[ThoiGian]
           ,[CongTrinhID]
           ,[ChiPhi])
SELECT DISTINCT
	[KhauHaoCongCuID] = NEWID()
	,ThoiGian = @BaselineDate
	,[CongTrinhID] = CT.CongTrinhId
	,ChiPhi = SUM(
						CONVERT(DECIMAL(18,2), 
								CASE WHEN ISNUMERIC(C.KhauHao) = 1 --AND C.KhauHao != 0.00
									THEN CONVERT(DECIMAL(18,2), C.SoLuong) * CONVERT(DECIMAL(18,2), C.GiaTien) / CONVERT(DECIMAL(18,2), C.KhauHao)
									ELSE 0.00
									END
						)
			)
FROM 
	CongTrinh CT
	JOIN KhoCongTrinh K
		ON CT.CongTrinhId = K.CongTrinhId
	JOIN CongCu C
		ON K.KhoId = C.KhoID
WHERE
	CT.Dept = 0
	AND ISNUMERIC(C.KhauHao) = 1
GROUP BY CT.CongTrinhId
END



GO
