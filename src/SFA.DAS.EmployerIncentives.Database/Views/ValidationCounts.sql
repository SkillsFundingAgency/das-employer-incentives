CREATE VIEW [incentives].[ValidationCounts]
AS
SELECT 
	'Valid' AS Metric,
	COUNT(*) AS [Value]
FROM [incentives].[PendingPaymentsValidationStatus]
WHERE ValidStatus = 1
UNION
SELECT 
	'Invalid' AS Metric,
	COUNT(*) AS [Value]
FROM [incentives].[PendingPaymentsValidationStatus]
WHERE ValidStatus = 0
GO
