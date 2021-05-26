CREATE TABLE [incentives].[ApprenticeshipBreakInLearning]
(	
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[StartDate] DATETIME2 NOT NULL,
	[EndDate] DATETIME2 NULL,
	PRIMARY KEY ([ApprenticeshipIncentiveId], [StartDate]),
	CONSTRAINT FK_ApprenticeshipBreakInLearning_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES [incentives].[ApprenticeshipIncentive](Id)
)
GO