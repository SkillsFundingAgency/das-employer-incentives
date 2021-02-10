CREATE VIEW [incentives].[PendingPaymentsValidationStepFailure]
AS
SELECT 
	VR.Step,
	SUM(CASE WHEN VR.Result = 0 THEN 1 ELSE 0 END) AS Failures
FROM incentives.PendingPaymentValidationResult VR
JOIN incentives.GetNextCollectionPeriod(GETDATE()) CP 
ON VR.PeriodNumber = CP.PeriodNumber AND VR.PaymentYear = CP.AcademicYear
GROUP BY VR.Step
GO