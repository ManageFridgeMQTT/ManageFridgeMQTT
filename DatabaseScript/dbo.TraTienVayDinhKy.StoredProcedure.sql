DROP PROCEDURE [TraTienVayDinhKy]
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
CREATE PROCEDURE [TraTienVayDinhKy]

AS
BEGIN
	DECLARE @i INT =1
DECLARE @numrows INT =0
DECLARE @SoTienDaTra float =0
DECLARE @SoTienConLai float =0
DECLARE @LaiVayID uniqueidentifier
DECLARE @SoNgayTinhLai float=0
DECLARE @SoTienLai float=0
DECLARE @SoTienTraGoc float=0
DECLARE @ThoiGian DateTime=0
DECLARE @NgayChotVay DateTime=0
DECLARE @LaiVayTAm TABLE (
    idx smallint Primary Key IDENTITY(1,1)
    , LaiVayID uniqueidentifier,
    NgayChotVay DateTime,
	SoTienTraGoc float,
	TuDongTraGoc float,
	SoTienDaTra float,
	SoTienConLai float
)
SET @ThoiGian= GETDATE()
--tạo bảng thêm mới
INSERT @LaiVayTAm
SELECT  a.LaiVayID,a.NgayChotVay,a.SoTienTraGoc,a.TuDongTraGoc,a.SoTienDaTra,a.SoTienConLai FROM LaiVay a
where a.TuDongTraGoc!=0 and DAY(a.NgayVay)=DAY(@ThoiGian) and ((YEAR(@ThoiGian)-YEAR(a.NgayVay)=0 and (MONTH(@ThoiGian)-MONTH(a.NgayVay))%CONVERT(int,a.TuDongTraGoc)=0)
or ((YEAR(@ThoiGian)-YEAR(a.NgayVay))*12+MONTH(@ThoiGian)-MONTH(a.NgayVay))%CONVERT(int,a.TuDongTraGoc)=0) and a.NgayDaoHan>=@ThoiGian
--Đếm số bản ghi trong bảng
SET @numrows= (SELECT COUNT(*) FROM @LaiVayTAm)
IF @numrows > 0
    WHILE (@i <= (SELECT MAX(idx) FROM @LaiVayTAm))
    BEGIN
 
    SET @LaiVayID =(select LaiVayID from @LaiVayTAm where idx=@i)
     SET @NgayChotVay =(select NgayChotVay from @LaiVayTAm where idx=@i)
     SET @SoTienTraGoc=(select SoTienTraGoc from @LaiVayTAm where idx=@i)
      SET @SoNgayTinhLai =(select TuDongTraGoc from @LaiVayTAm where idx=@i) 
      SET @SoTienDaTra =(select SoTienDaTra from @LaiVayTAm where idx=@i)
      SET @SoTienConLai=(select SoTienConLai from @LaiVayTAm where idx=@i)
      if @SoTienTraGoc>@SoTienConLai
       SET @i = @i + 1
       ELSE
       BEGIN
      -- Tính toán số ngày
		SET @SoNgayTinhLai =(SELECT DATEDIFF(day, @NgayChotVay, @ThoiGian))
      --Tính số lãi dựa vào số ngày
		SET @SoTienLai=@SoTienConLai*@SoNgayTinhLai/36500
      -- SET lại số tiền đã trả và số tiền còn lại
		SET @SoTienDaTra=@SoTienDaTra+@SoTienTraGoc
		  SET @SoTienConLai=@SoTienConLai-@SoTienTraGoc
		Update LaiVay set SoTienConLai=@SoTienConLai,SoTienDaTra=@SoTienDaTra,NgayChotVay=@ThoiGian where LaiVayID=@LaiVayID
		InSert into LichSuTraTien (LaiVayID,SoTienTra,NgayTra,Mota,TienLai,SoNgayTinhLai)VALUES
		(@LaiVayID,@SoTienTraGoc,@ThoiGian,N'Tự động',@SoTienLai,@SoNgayTinhLai )
    
        SET @i = @i + 1
        END
    END
END

GO
