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
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(14,2121,'2021-09-22 08:00:00.000',9,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(13,2120,'2021-08-13 08:00:00.000',8,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(12,1833,'2021-07-14 08:00:00.000',7,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(11,1832,'2021-06-14 08:00:00.000',6,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(10,1831,'2021-05-17 08:00:00.000',5,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(9,1830,'2021-04-16 08:00:00.000',4,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(8,1829,'2021-03-12 09:00:00.000',3,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(7,1828,'2021-02-12 09:00:00.000',2,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(6,1827,'2021-01-15 09:00:00.000',1,2021)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(5,1826,'2020-12-14 09:00:00.000',12,2020)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(4,1825,'2020-11-13 09:00:00.000',11,2020)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(3,1824,'2020-10-14 08:00:00.000',10,2020)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(2,1823,'2020-09-14 08:00:00.000',9,2020)
INSERT INTO incentives.CollectionCalendar (Id, PeriodNumber, EIScheduledOpenDateUTC, CalendarMonth, CalendarYear) VALUES(1,1822,'2020-08-19 08:00:00.000',8,2020)
GO