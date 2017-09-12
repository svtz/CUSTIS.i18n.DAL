-- Run under priviliged user

USE [master]
GO

CREATE DATABASE [TestEfI18N]
GO

ALTER DATABASE [TestEfI18N] SET ALLOW_SNAPSHOT_ISOLATION ON  
GO

ALTER DATABASE [TestEfI18N] SET READ_COMMITTED_SNAPSHOT ON 
GO

USE [TestEfI18N]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_product](
	[id_product] [bigint] IDENTITY(1,1) NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[name] [xml] NULL,
 CONSTRAINT [PK_product] PRIMARY KEY CLUSTERED ([id_product])
) 

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_product_code] ON [dbo].[t_product] ([code] ASC)
GO

CREATE PRIMARY XML INDEX [XML_IX_product_name] ON [dbo].[t_product] ([name])
GO

USE [master]
GO

CREATE LOGIN [TEST_EF_USER] WITH PASSWORD=N'test', DEFAULT_DATABASE=[TestEfI18N], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

USE [TestEfI18N]
GO

CREATE USER [TEST_EF_USER] FOR LOGIN [TEST_EF_USER] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember N'db_datareader', N'TEST_EF_USER'
GO

EXEC sp_addrolemember N'db_datawriter', N'TEST_EF_USER'
GO
