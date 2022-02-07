CREATE PROCEDURE [dbo].[ImportEmploymentCheckResult]
	@uln BIGINT,
	@checkType NVARCHAR(50),
	@minimumDate DATETIME,
	@maximumDate DATETIME,
	@result BIT
AS
	DECLARE @apprenticeshipIncentiveId UNIQUEIDENTIFIER

	IF EXISTS(SELECT TOP 1 * FROM incentives.ApprenticeshipIncentive WHERE ULN = @uln AND Status != 'Withdrawn')
	BEGIN
		SET @apprenticeshipIncentiveId = (SELECT TOP 1 Id FROM incentives.ApprenticeshipIncentive WHERE ULN = @uln AND Status != 'Withdrawn')

		IF EXISTS(SELECT TOP 1 * FROM incentives.EmploymentCheck WHERE ApprenticeshipIncentiveId = @apprenticeshipIncentiveId AND CheckType = @checkType)
		BEGIN
			UPDATE incentives.EmploymentCheck SET CorrelationId = NEWID(), Result = @result, ResultDateTime = GETDATE()
			WHERE ApprenticeshipIncentiveId = @apprenticeshipIncentiveId AND CheckType = @checkType
		END
		ELSE
		BEGIN
			INSERT INTO incentives.EmploymentCheck
			(Id, ApprenticeshipIncentiveId, CheckType, MinimumDate, MaximumDate, CorrelationId, Result, CreatedDateTime)
			VALUES
			(NEWID(), @apprenticeshipIncentiveId, @checkType, @minimumDate, @maximumDate, NEWID(), @result, GETDATE())
		END
	END
