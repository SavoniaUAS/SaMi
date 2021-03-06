USE [SavoniaMeasurementsV2]
GO
/****** Object:  Table [dbo].[AccessKey]    Script Date: 30.8.2016 17.11.09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccessKey](
	[ProviderID] [int] NOT NULL,
	[Key] [nvarchar](448) NOT NULL,
	[AccessControl] [int] NOT NULL,
	[ValidFrom] [datetimeoffset](7) NULL,
	[ValidTo] [datetimeoffset](7) NULL,
	[KeyId] [smallint] NULL,
	[KeyEncrypt] [nvarchar](448) NULL,
	[Info] [nvarchar](max) NULL,
 CONSTRAINT [PK_Keys] PRIMARY KEY CLUSTERED 
(
	[ProviderID] ASC,
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Data]    Script Date: 30.8.2016 17.11.09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data](
	[MeasurementID] [bigint] NOT NULL,
	[Tag] [nvarchar](50) NOT NULL,
	[Value] [float] NULL,
	[LongValue] [bigint] NULL,
	[TextValue] [nvarchar](max) NULL,
	[BinaryValue] [varbinary](max) NULL,
	[XmlValue] [xml] NULL,
 CONSTRAINT [PK_Data] PRIMARY KEY CLUSTERED 
(
	[MeasurementID] ASC,
	[Tag] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Measurement]    Script Date: 30.8.2016 17.11.09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Measurement](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ProviderID] [int] NOT NULL,
	[Object] [nvarchar](50) NULL,
	[Tag] [nvarchar](50) NULL,
	[Timestamp] [datetimeoffset](7) NOT NULL,
	[Note] [nvarchar](max) NULL,
	[Location] [geography] NULL,
	[RowCreatedTimestamp] [datetimeoffset](7) NULL,
	[KeyId] [smallint] NULL,
 CONSTRAINT [PK_Measurement] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Meta]    Script Date: 30.8.2016 17.11.09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Meta](
	[Context] [nvarchar](50) NOT NULL,
	[Object] [nvarchar](100) NOT NULL,
	[Tag] [nvarchar](50) NOT NULL,
	[Version] [int] NOT NULL,
	[Data] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_Meta] PRIMARY KEY CLUSTERED 
(
	[Context] ASC,
	[Object] ASC,
	[Tag] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Provider]    Script Date: 30.8.2016 17.11.09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Provider](
	[ID] [int] IDENTITY(256,1) NOT NULL,
	[Key] [nvarchar](448) NOT NULL,
	[Info] [nvarchar](max) NULL,
	[Name] [nvarchar](250) NULL,
	[Owner] [nvarchar](250) NULL,
	[IsPublicDomain] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Tag] [nvarchar](50) NULL,
	[Location] [geography] NULL,
	[ContactEmail] [nvarchar](250) NULL,
	[ActiveFrom] [datetime] NULL,
	[ActiveTo] [datetime] NULL,
	[DataStorageUntil] [datetime] NULL,
	[CreatedBy] [nvarchar](250) NULL,
 CONSTRAINT [PK_Provider] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Provider_Key] UNIQUE NONCLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Query]    Script Date: 30.8.2016 17.11.09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Query](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProviderID] [int] NOT NULL,
	[Key] [nvarchar](448) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Object] [nvarchar](50) NULL,
	[Tag] [nvarchar](50) NULL,
	[Take] [int] NULL,
	[From] [datetimeoffset](7) NULL,
	[To] [datetimeoffset](7) NULL,
	[Sensors] [nvarchar](max) NULL,
 CONSTRAINT [PK_Query] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Sensor]    Script Date: 30.8.2016 17.11.09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sensor](
	[ProviderID] [int] NOT NULL,
	[Tag] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](250) NULL,
	[Description] [nvarchar](max) NULL,
	[Rounding] [int] NULL,
	[Unit] [nvarchar](50) NULL,
	[Context] [nvarchar](250) NULL,
	[Location] [geography] NULL,
 CONSTRAINT [PK_Sensor] PRIMARY KEY CLUSTERED 
(
	[ProviderID] ASC,
	[Tag] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[AccessKey]  WITH CHECK ADD  CONSTRAINT [FK_Keys_Provider] FOREIGN KEY([ProviderID])
REFERENCES [dbo].[Provider] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AccessKey] CHECK CONSTRAINT [FK_Keys_Provider]
GO
ALTER TABLE [dbo].[Data]  WITH CHECK ADD  CONSTRAINT [FK_Data_Measurement] FOREIGN KEY([MeasurementID])
REFERENCES [dbo].[Measurement] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Data] CHECK CONSTRAINT [FK_Data_Measurement]
GO
ALTER TABLE [dbo].[Measurement]  WITH NOCHECK ADD  CONSTRAINT [FK_Measurement_Provider] FOREIGN KEY([ProviderID])
REFERENCES [dbo].[Provider] ([ID])
GO
ALTER TABLE [dbo].[Measurement] CHECK CONSTRAINT [FK_Measurement_Provider]
GO
ALTER TABLE [dbo].[Query]  WITH CHECK ADD  CONSTRAINT [FK_Query_AccessKey] FOREIGN KEY([ProviderID], [Key])
REFERENCES [dbo].[AccessKey] ([ProviderID], [Key])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Query] CHECK CONSTRAINT [FK_Query_AccessKey]
GO
ALTER TABLE [dbo].[Sensor]  WITH CHECK ADD  CONSTRAINT [FK_Sensor_Provider] FOREIGN KEY([ProviderID])
REFERENCES [dbo].[Provider] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Sensor] CHECK CONSTRAINT [FK_Sensor_Provider]
GO
