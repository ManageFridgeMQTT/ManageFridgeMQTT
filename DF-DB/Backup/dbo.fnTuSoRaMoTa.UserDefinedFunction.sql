/****** Object:  UserDefinedFunction [dbo].[fnTuSoRaMoTa]    Script Date: 16/06/2016 11:45:07 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[fnTuSoRaMoTa]
(
	@TrangThaiID INT
	,@LoaiTrangThai NVARCHAR(50)
)
RETURNS NVARCHAR(500)
AS
BEGIN
	DECLARE @Desc NVARCHAR(500) = ''
	IF(@LoaiTrangThai = 'TrangThai')
		--0: Không Hoạt Động
		--1: Đang Hoạt Động
		---1: Chưa Báo Cáo
		--Còn lại: Không Xác Định
		SET @Desc = CASE WHEN @TrangThaiID = 0 THEN N'Không Hoạt Động'
					WHEN @TrangThaiID = 1 THEN N'Đang Hoạt Động'
					WHEN @TrangThaiID = -1 THEN N'Chưa Báo Cáo'
					ELSE N'Không Xác Định' END
	ELSE IF(@LoaiTrangThai = 'TinhTrang')
		--1: Hư Hỏng
		--Còn lại: Bình Thường
		SET @Desc = CASE WHEN @TrangThaiID = 1 THEN N'Hư Hỏng'
					ELSE N'Bình Thườngh' END
	ELSE IF(@LoaiTrangThai = 'LoaiHinhBC')
		--0: Báo cáo tình trạng:  TieuHaoNhienLieu = ThoiGianSuDung = null
		--1: Báo cáo sử dụng:  TrangThai =  TinhTrang = MoTa = null
		--2: Báo cáo tình trạng và sử dụng
		--3: Thêm mới
		--4: Sửa chữa
		--5: Di chuyển thiết bị
		SET @Desc = CASE WHEN @TrangThaiID = 0 THEN N'Báo cáo tình trạng'
					WHEN @TrangThaiID = 1 THEN N'Báo cáo sử dụng'
					WHEN @TrangThaiID = 2 THEN N'Báo cáo tình trạng và sử dụng'
					WHEN @TrangThaiID = 3 THEN N'Thêm mới'
					WHEN @TrangThaiID = 4 THEN N'Sửa chữa'
					WHEN @TrangThaiID = 5 THEN N'Di chuyển thiết bị'
					ELSE N'' END
		
	ELSE IF(@LoaiTrangThai = 'TrangThaiDiChuyen')
		--0: Thiết bị đang được chuyển đến
		--1: Nhận được thiết bị
		--2: Thiết bị đang được chuyển đi
		SET @Desc = CASE WHEN @TrangThaiID = 0 THEN N'Thiết bị đang được chuyển đến'
					WHEN @TrangThaiID = 1 THEN N'Nhận được thiết bị'
					WHEN @TrangThaiID = 2 THEN N'Thiết bị đang được chuyển đi'
					ELSE N'' END
	ELSE IF(@LoaiTrangThai = 'TrangThaiHienTai')
		SET @Desc = CASE WHEN @TrangThaiID = 3 THEN N'Đang bảo dưỡng'
						WHEN @TrangThaiID = 4 THEN N'Tắt máy bảo dưỡng'
						WHEN @TrangThaiID = 5 THEN N'Đang di chuyển'
						WHEN @TrangThaiID = 6 THEN N'Tắt máy di chuyển'
						WHEN @TrangThaiID = 7 THEN N'Đang khoan'
						WHEN @TrangThaiID = 8 THEN N'Tắt máy khoan'
						WHEN @TrangThaiID = 9 THEN N'Đang cẩu'
						WHEN @TrangThaiID = 10 THEN N'Tắt máy cẩu'
						ELSE N'Không xác định' END
	
	RETURN @Desc

END


GO
