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
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(12,12,'2021-07-07 00:00:00',7,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(11,11,'2021-06-05 00:00:00',6,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(10,10,'2021-05-08 00:00:00',5,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(9,9,'2021-04-09 00:00:00',4,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(8,8,'2021-03-05 00:00:00',3,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(7,7,'2021-02-05 00:00:00',2,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(6,6,'2021-01-08 00:00:00',1,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(5,5,'2020-12-05 00:00:00',12,2020)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(4,4,'2020-11-06 00:00:00',11,2020)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(3,3,'2020-10-07 00:00:00',10,2020)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(2,2,'2020-09-05 00:00:00',9,2020)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(1,1,'2020-08-07 00:00:00',8,2020)

INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(24,12,'2022-07-07 00:00:00',7,2022)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(23,11,'2022-06-07 00:00:00',6,2022)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(22,10,'2022-05-07 00:00:00',5,2022)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(21,9,'2022-04-07 00:00:00',4,2022)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(20,8,'2022-03-05 00:00:00',3,2022)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(19,7,'2022-02-05 00:00:00',2,2022)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(18,6,'2022-01-08 00:00:00',1,2022)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(17,5,'2021-12-07 00:00:00',12,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(16,4,'2021-11-05 00:00:00',11,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(15,3,'2021-10-07 00:00:00',10,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(14,2,'2021-09-07 00:00:00',9,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(13,1,'2021-08-06 00:00:00',8,2021)
GO