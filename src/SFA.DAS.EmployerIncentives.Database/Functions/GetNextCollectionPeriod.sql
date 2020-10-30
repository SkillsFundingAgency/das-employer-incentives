CREATE FUNCTION [incentives].[GetNextCollectionPeriod]
(
	@today DATETIME2
)
RETURNS TABLE
AS
RETURN SELECT Top 1 
	   [Id]
      ,[PeriodNumber]
      ,[CalendarMonth]
      ,[CalendarYear]
      ,[EIScheduledOpenDateUTC]
  FROM [incentives].[CollectionCalendar]
  WHERE @today <= EIScheduledOpenDateUTC  
  ORDER BY EIScheduledOpenDateUTC

GO 