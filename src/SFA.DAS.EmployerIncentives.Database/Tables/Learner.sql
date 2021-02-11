CREATE TABLE [incentives].[Learner]
(
	[Id] UNIQUEIDENTIFIER  NOT NULL PRIMARY KEY NONCLUSTERED,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[ApprenticeshipId] BIGINT NOT NULL,
	[Ukprn] BIGINT NOT NULL,
	[ULN] BIGINT NOT NULL,
	[SubmissionFound] BIT NOT NULL,
	[SubmissionDate] DATETIME2 NULL,
	[LearningFound] BIT NULL,
	[HasDataLock] BIT NULL,
	[StartDate] DATETIME2 NULL,
	[InLearning] BIT NULL,
	[RawJSON] NVARCHAR(MAX) NULL,
	[CreatedDate] DATETIME2 NOT NULL,
	[UpdatedDate] DATETIME2 NULL
)
GO
CREATE CLUSTERED INDEX IX_Learner_ApprenticeshipIncentiveId ON [incentives].Learner (ApprenticeshipIncentiveId)
GO