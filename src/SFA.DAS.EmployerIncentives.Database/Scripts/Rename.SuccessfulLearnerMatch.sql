IF EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE TABLE_SCHEMA = 'incentives'
		  AND TABLE_NAME = 'Learner'
          AND COLUMN_NAME = 'SuccessfulLearnerMatch') 
BEGIN
	EXEC sp_rename 'incentives.Learner.SuccessfulLearnerMatch', 'SuccessfulLearnerMatchExecution', 'COLUMN';
END