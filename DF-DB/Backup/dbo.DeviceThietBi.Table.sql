USE [DF_RELEASE]
GO
/****** Object:  Table [dbo].[DeviceThietBi]    Script Date: 21/05/2016 6:30:43 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeviceThietBi](
	[ThietBiId] [uniqueidentifier] NOT NULL,
	[DeviceId] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DeviceThietBi_1] PRIMARY KEY CLUSTERED 
(
	[ThietBiId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[DeviceThietBi] ADD  CONSTRAINT [DF_DeviceThietBi_ThietBiId]  DEFAULT (newid()) FOR [ThietBiId]
GO
