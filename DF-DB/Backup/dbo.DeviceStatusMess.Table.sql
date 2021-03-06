USE [DF_RELEASE]
GO
/****** Object:  Table [dbo].[DeviceStatusMess]    Script Date: 21/05/2016 6:30:43 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeviceStatusMess](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[ThietBiID] [nvarchar](50) NOT NULL,
	[CommandType] [nvarchar](200) NULL,
	[CommandId] [nvarchar](200) NULL,
	[CommandAction] [nvarchar](200) NULL,
	[Loai] [int] NULL,
	[TrangThai] [int] NULL,
	[StatusMay] [int] NULL,
	[Time] [float] NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
 CONSTRAINT [PK_DeviceStatusMess] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
