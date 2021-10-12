CREATE PROCEDURE [support].[CreateRetrospectiveApplication]
	@accountId BIGINT,
	@accountLegalEntityId BIGINT,
	@submittedByEmail NVARCHAR(255),
	@submittedByName NVARCHAR(MAX),
	@apprenticeshipId BIGINT,
	@firstName NVARCHAR(100),
	@LastName NVARCHAR(100),
	@dateOfBirth DATETIME2,
	@uln BIGINT,
	@plannedStartDate DATETIME2,
	@apprenticeshipEmployerTypeOnApproval INT,
    @ukprn BIGINT, 
	@courseName NVARCHAR(126),
	@phase NVARCHAR(50)
AS
BEGIN
	DECLARE @applicationId AS UNIQUEIDENTIFIER
	SET @applicationId = NEWID()

	IF EXISTS(SELECT TOP 1 * FROM IncentiveApplicationApprenticeship iaa INNER JOIN IncentiveApplication ia ON ia.Id = iaa.IncentiveApplicationId WHERE ULN = @uln AND [Status] = 'Submitted' AND WithdrawnByCompliance = 0 AND WithdrawnByEmployer = 0)
		THROW 50001, 'Application already exists', 1

	BEGIN TRAN

	INSERT INTO dbo.IncentiveApplication
		(Id, AccountId, AccountLegalEntityId, DateCreated, Status, DateSubmitted, SubmittedByEmail, SubmittedByName)
	VALUES
		(@applicationId, @accountId, @accountLegalEntityId, GETDATE(), 'Submitted', GETDATE(), @submittedByEmail, @submittedByName)

	INSERT INTO dbo.IncentiveApplicationApprenticeship
		(Id, IncentiveApplicationId, ApprenticeshipId, FirstName, LastName, DateOfBirth, ULN, PlannedStartDate, ApprenticeshipEmployerTypeOnApproval, UKPRN, EarningsCalculated, WithdrawnByEmployer, WithdrawnByCompliance, CourseName, EmploymentStartDate, Phase, HasEligibleEmploymentStartDate)
	VALUES
		(NEWID(), @applicationId, @apprenticeshipId, @firstName, @LastName, @dateOfBirth, @uln, @plannedStartDate, @apprenticeshipEmployerTypeOnApproval, @ukprn, 0, 0, 0, @courseName, @plannedStartDate, @phase, 1)

	COMMIT TRAN
END