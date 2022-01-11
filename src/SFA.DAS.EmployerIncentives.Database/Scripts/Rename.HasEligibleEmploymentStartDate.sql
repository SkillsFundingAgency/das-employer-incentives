IF EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE TABLE_SCHEMA = 'dbo'
		  AND TABLE_NAME = 'IncentiveApplicationApprenticeship'
          AND COLUMN_NAME = 'HasEligibleEmploymentStartDate') 
BEGIN
	EXEC sp_rename 'dbo.IncentiveApplicationApprenticeship.HasEligibleEmploymentStartDate', 'StartDatesAreEligible', 'COLUMN';
END 