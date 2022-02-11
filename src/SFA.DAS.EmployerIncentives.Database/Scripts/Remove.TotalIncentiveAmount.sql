IF EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE TABLE_SCHEMA = 'dbo'
		  AND TABLE_NAME = 'IncentiveApplicationApprenticeship'
          AND COLUMN_NAME = 'TotalIncentiveAmount') 
BEGIN
	ALTER TABLE [dbo].[IncentiveApplicationApprenticeship] DROP COLUMN [TotalIncentiveAmount]
END