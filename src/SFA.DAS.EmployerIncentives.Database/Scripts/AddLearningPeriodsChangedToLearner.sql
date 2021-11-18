IF NOT EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE TABLE_SCHEMA = 'incentives'
		  AND TABLE_NAME = 'Learner'
          AND COLUMN_NAME = 'LearningPeriodsChanged') 
BEGIN
	ALTER TABLE incentives.Learner
        ADD [LearningPeriodsChanged] BIT NOT NULL DEFAULT(0)
END