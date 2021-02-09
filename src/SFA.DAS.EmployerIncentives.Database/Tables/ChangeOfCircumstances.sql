CREATE TABLE [incentives].[ChangeOfCircumstance]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[ChangeType] VARCHAR(20) NOT NULL,
	[PreviousValue] VARCHAR(100) NOT NULL,
	[PreviousPeriodNumber] TINYINT NULL,
	[PreviousPaymentYear] SMALLINT NULL, 
	[NewValue] VARCHAR(100) NOT  NULL,	
	[ChangedDate] DATETIME2 NOT NULL
)
GO


