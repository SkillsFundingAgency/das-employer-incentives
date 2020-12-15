/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

PRINT '********************* Prepping @COLLECTION_CALENDAR temp table *****************************'
DECLARE @COLLECTION_CALENDAR TABLE
(
    [Id] INT NOT NULL PRIMARY KEY NONCLUSTERED,
    [PeriodNumber] TINYINT NOT NULL,
    [CalendarMonth] TINYINT NOT NULL,
    [CalendarYear] SMALLINT NOT NULL,
    [EIScheduledOpenDateUTC] DATETIME2 NOT NULL, 
    [CensusDate] DATETIME NULL, 
    [AcademicYear] VARCHAR(10) NULL, 
    [Active] BIT NULL
)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (1, 1, 8, 2020, CAST(N'2020-08-08T00:00:00.0000000' AS DateTime2), CAST(N'2020-08-31T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (2, 2, 9, 2020, CAST(N'2020-09-06T00:00:00.0000000' AS DateTime2), CAST(N'2020-09-30T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (3, 3, 10, 2020, CAST(N'2020-10-08T00:00:00.0000000' AS DateTime2), CAST(N'2020-10-31T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (4, 4, 11, 2020, CAST(N'2020-11-07T00:00:00.0000000' AS DateTime2), CAST(N'2020-11-30T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (5, 5, 12, 2020, CAST(N'2020-12-06T00:00:00.0000000' AS DateTime2), CAST(N'2020-12-31T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (6, 6, 1, 2021, CAST(N'2021-01-09T00:00:00.0000000' AS DateTime2), CAST(N'2021-01-31T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (7, 7, 2, 2021, CAST(N'2021-02-06T00:00:00.0000000' AS DateTime2), CAST(N'2021-02-28T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (8, 8, 3, 2021, CAST(N'2021-03-06T00:00:00.0000000' AS DateTime2), CAST(N'2021-03-31T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (9, 9, 4, 2021, CAST(N'2021-04-10T00:00:00.0000000' AS DateTime2), CAST(N'2021-04-30T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (10, 10, 5, 2021, CAST(N'2021-05-09T00:00:00.0000000' AS DateTime2), CAST(N'2021-05-31T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (11, 11, 6, 2021, CAST(N'2021-06-06T00:00:00.0000000' AS DateTime2), CAST(N'2021-06-30T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (12, 12, 7, 2021, CAST(N'2021-07-08T00:00:00.0000000' AS DateTime2), CAST(N'2021-07-31T00:00:00.000' AS DateTime), N'2021', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (13, 1, 8, 2021, CAST(N'2021-08-07T00:00:00.0000000' AS DateTime2), CAST(N'2021-08-31T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (14, 2, 9, 2021, CAST(N'2021-09-08T00:00:00.0000000' AS DateTime2), CAST(N'2021-09-30T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (15, 3, 10, 2021, CAST(N'2021-10-08T00:00:00.0000000' AS DateTime2), CAST(N'2021-10-31T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (16, 4, 11, 2021, CAST(N'2021-11-06T00:00:00.0000000' AS DateTime2), CAST(N'2021-11-30T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (17, 5, 12, 2021, CAST(N'2021-12-08T00:00:00.0000000' AS DateTime2), CAST(N'2021-12-31T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (18, 6, 1, 2022, CAST(N'2022-01-09T00:00:00.0000000' AS DateTime2), CAST(N'2022-01-31T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (19, 7, 2, 2022, CAST(N'2022-02-06T00:00:00.0000000' AS DateTime2), CAST(N'2022-02-28T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (20, 8, 3, 2022, CAST(N'2022-03-06T00:00:00.0000000' AS DateTime2), CAST(N'2022-03-31T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (21, 9, 4, 2022, CAST(N'2022-04-08T00:00:00.0000000' AS DateTime2), CAST(N'2022-04-30T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (22, 10, 5, 2022, CAST(N'2022-05-08T00:00:00.0000000' AS DateTime2), CAST(N'2022-05-31T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (23, 11, 6, 2022, CAST(N'2022-06-08T00:00:00.0000000' AS DateTime2), CAST(N'2022-06-30T00:00:00.000' AS DateTime), N'2122', 0)
INSERT @COLLECTION_CALENDAR ([Id], [PeriodNumber], [CalendarMonth], [CalendarYear], [EIScheduledOpenDateUTC], [CensusDate], [AcademicYear], [Active]) VALUES (24, 12, 7, 2022, CAST(N'2022-07-08T00:00:00.0000000' AS DateTime2), CAST(N'2022-07-31T00:00:00.000' AS DateTime), N'2122', 0)

PRINT '********************* Updating table [incentives].[CollectionCalendar] *****************************'

MERGE [incentives].[CollectionCalendar] AS tgt
USING (
    SELECT
        [Id],
        [PeriodNumber],
        [CalendarMonth],
        [CalendarYear],
        [EIScheduledOpenDateUTC],
        [CensusDate],
        [AcademicYear],
        [Active]
    FROM @COLLECTION_CALENDAR
) AS src 
ON (tgt.[PeriodNumber] = src.[PeriodNumber] AND tgt.[CalendarMonth] = src.[CalendarMonth] AND tgt.[CalendarYear] = src.[CalendarYear]) 

WHEN NOT MATCHED BY TARGET THEN 
INSERT 
           ([Id]
           ,[PeriodNumber]
           ,[CalendarMonth]
           ,[CalendarYear]
           ,[EIScheduledOpenDateUTC]
           ,[CensusDate]
           ,[AcademicYear]
           ,[Active])
     VALUES
           (src.[Id]
           ,src.[PeriodNumber]
           ,src.[CalendarMonth]
           ,src.[CalendarYear]
           ,src.[EIScheduledOpenDateUTC]
           ,src.[CensusDate]
           ,src.[AcademicYear]
           ,src.[Active])

WHEN MATCHED AND
    src.[EIScheduledOpenDateUTC] <> tgt.[EIScheduledOpenDateUTC]
    OR ISNULL(src.[CensusDate], '') <> ISNULL(tgt.[CensusDate], '')
    OR ISNULL(src.[AcademicYear], '') <> ISNULL(tgt.[AcademicYear], '')
THEN UPDATE
   SET 
       [EIScheduledOpenDateUTC] = src.[EIScheduledOpenDateUTC]
      ,[CensusDate] = src.[CensusDate]
      ,[AcademicYear] = src.[AcademicYear]

WHEN NOT MATCHED BY SOURCE THEN
    DELETE;