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
DELETE FROM incentives.CollectionCalendar
GO
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(12,12,'2021-07-08 00:00:00',7,2021, '2021-07-31 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(11,11,'2021-06-06 00:00:00',6,2021, '2021-06-30 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(10,10,'2021-05-09 00:00:00',5,2021, '2021-05-31 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(9,9,'2021-04-10 00:00:00',4,2021, '2021-04-30 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(8,8,'2021-03-06 00:00:00',3,2021, '2021-03-31 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(7,7,'2021-02-06 00:00:00',2,2021, '2021-02-28 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(6,6,'2021-01-09 00:00:00',1,2021, '2021-01-31 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(5,5,'2020-12-06 00:00:00',12,2020, '2020-12-31 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(4,4,'2020-11-07 00:00:00',11,2020, '2020-11-30 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(3,3,'2020-10-08 00:00:00',10,2020, '2020-10-31 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(2,2,'2020-09-06 00:00:00',9,2020, '2020-09-30 00:00:00', '2021', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(1,1,'2020-08-08 00:00:00',8,2020, '2020-08-31 00:00:00', '2021', 0)

INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(24,12,'2022-07-08 00:00:00',7,2022, '2022-07-31 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(23,11,'2022-06-08 00:00:00',6,2022, '2022-06-30 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(22,10,'2022-05-08 00:00:00',5,2022, '2022-05-31 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(21,9,'2022-04-08 00:00:00',4,2022, '2022-04-30 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(20,8,'2022-03-06 00:00:00',3,2022, '2022-03-31 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(19,7,'2022-02-06 00:00:00',2,2022, '2022-02-28 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(18,6,'2022-01-09 00:00:00',1,2022, '2022-01-31 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(17,5,'2021-12-08 00:00:00',12,2021, '2021-12-31 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(16,4,'2021-11-06 00:00:00',11,2021, '2021-11-30 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(15,3,'2021-10-08 00:00:00',10,2021, '2021-10-31 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(14,2,'2021-09-08 00:00:00',9,2021, '2021-09-30 00:00:00', '2122', 0)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(13,1,'2021-08-07 00:00:00',8,2021, '2021-08-31 00:00:00', '2122', 0)

INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear, CensusDate, AcademicYear, Active) VALUES(25,8,'2022-08-06 00:00:00',8,2022, '2022-08-31 00:00:00', '2223', 0)

GO