-- Run under priviliged user
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

USE [master]
GO

CREATE DATABASE [TestEfI18N]
GO

ALTER DATABASE [TestEfI18N] SET ALLOW_SNAPSHOT_ISOLATION ON  
GO

ALTER DATABASE [TestEfI18N] SET READ_COMMITTED_SNAPSHOT ON 
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

IF OBJECT_ID (N'[dbo].[McsGetString]', N'FN') IS NOT NULL  
    DROP FUNCTION [dbo].[McsGetString]
GO

-- =============================================
-- Description:	MultiCulturalString.GetString analogue for db
-- =============================================
CREATE FUNCTION [dbo].[McsGetString]
(
	@aMcs xml,
	@aLocale nvarchar(100),
	@aFallbackChain nvarchar(100) = NULL
)
-- Functions must be created with schema binding to be deterministic
RETURNS nvarchar(255)
WITH SCHEMABINDING 
AS
BEGIN
	IF @aMcs IS NULL 
		RETURN NULL
	IF (@aLocale IS NULL) 
		RETURN NULL
		
	DECLARE @result nvarchar(max)
	DECLARE @listSeparator nvarchar(1) = N','
	DECLARE @separatorPosition int = CHARINDEX(@listSeparator, @aFallbackChain)

	IF (@aFallbackChain IS NOT NULL AND @aLocale <> CASE WHEN @separatorPosition > 0 THEN SUBSTRING(@aFallbackChain, 1, @separatorPosition - 1) ELSE @aFallbackChain END) 
		RETURN NULL

	DECLARE	@nextFallbackChain nvarchar(100) = CASE WHEN @aFallbackChain IS NULL THEN @aLocale ELSE @aFallbackChain END
	DECLARE @nextLocale nvarchar(100) = @aLocale
	
	WHILE @nextFallbackChain IS NOT NULL AND @result IS NULL AND @nextLocale IS NOT NULL BEGIN
	
		SET @result = @aMcs
			.value('declare namespace i18n="http://custis.ru/i18n";
					data((/i18n:MultiCulturalString/*[namespace-uri()="http://custis.ru/i18n" and local-name()=sql:variable("@nextLocale")])[1])', 
				   'nvarchar(max)')

		SET @nextFallbackChain = CASE WHEN @separatorPosition > 0 THEN SUBSTRING(@nextFallbackChain, @separatorPosition + 1, LEN(@nextFallbackChain)) ELSE NULL END
		SET @separatorPosition = CHARINDEX (@listSeparator, @nextFallbackChain)
		SET @nextLocale = CASE WHEN @separatorPosition > 0 THEN SUBSTRING(@nextFallbackChain, 1, @separatorPosition - 1) ELSE @nextFallbackChain END;
	END
	
	RETURN @result
END
GO

GRANT EXECUTE ON [dbo].[McsGetString] TO [TEST_EF_USER]
GO

CREATE TABLE [dbo].[t_product](
	[id_product] [bigint] IDENTITY(1,1) NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[name] [xml] NULL,
	[name_ru_nofallback]	as [dbo].[McsGetString]([name], 'ru', null),
	[name_ru_stdfallback]	as [dbo].[McsGetString]([name], 'ru', 'ru,en'),
	[name_ruru_stdfallback]	as [dbo].[McsGetString]([name], 'ru-RU', 'ru-RU,ru,en'),
	[name_en_nofallback]	as [dbo].[McsGetString]([name], 'en', null),
	[name_en_stdfallback]	as [dbo].[McsGetString]([name], 'en', 'en'),
	[name_enus_stdfallback]	as [dbo].[McsGetString]([name], 'ru', 'en-US,en'),
	CONSTRAINT [PK_product] PRIMARY KEY CLUSTERED ([id_product])
) 

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_product_code] ON [dbo].[t_product] ([code] ASC)
GO

CREATE PRIMARY XML INDEX [XML_IX_product_name] ON [dbo].[t_product] ([name])
GO
CREATE INDEX IX_prodname_ru_nofallback
  ON [dbo].[t_product] ([name_ru_nofallback]);
  
CREATE INDEX IX_prodname_ru_stdfallback
  ON [dbo].[t_product] ([name_ru_stdfallback]);

CREATE INDEX IX_prodname_ruru_stdfallback
  ON [dbo].[t_product] ([name_ruru_stdfallback]);

CREATE INDEX IX_prodname_en_nofallback
  ON [dbo].[t_product] ([name_en_nofallback]);
  
CREATE INDEX IX_prodname_en_stdfallback
  ON [dbo].[t_product] ([name_en_stdfallback]);

CREATE INDEX IX_prodname_enus_stdfallback
  ON [dbo].[t_product] ([name_enus_stdfallback]);


