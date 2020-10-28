CREATE VIEW [incentives].[PendingPaymentsValidationStatus]
AS
SELECT 
	VR.PendingPaymentId,
	CASE WHEN COUNT(*) = SUM(CASE WHEN VR.Result = 1 THEN 1 ELSE 0 END) THEN 1
	ELSE 0
	END AS ValidStatus
FROM incentives.PendingPaymentValidationResult VR
JOIN incentives.GetNextCollectionPeriod(GETDATE()) CP 
ON VR.CollectionPeriodMonth = CP.CalendarMonth AND VR.CollectionPeriodYear = CP.CalendarYear
GROUP BY VR.PendingPaymentId
GO