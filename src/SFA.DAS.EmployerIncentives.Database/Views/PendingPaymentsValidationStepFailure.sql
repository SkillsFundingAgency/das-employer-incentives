CREATE VIEW [incentives].[PendingPaymentsValidationStepFailure]
AS
SELECT 1 AS TEST
--SELECT 
--	VR.ValidationStep,
--	SUM(CASE WHEN ValidationResult = 0 THEN 1 ELSE 0 END) AS Failures
--FROM incentives.PendingPaymentValidationResult VR
--JOIN incentives.GetNextCollectionPeriod(GETDATE()) CP 
--ON VR.CollectionPeriodMonth = CP.CalendarMonth AND VR.CollectionPeriodYear = CP.CalendarYear
--GROUP BY VR.ValidationStep
GO