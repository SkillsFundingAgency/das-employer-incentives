CREATE TABLE [incentives].[ApprenticeshipDaysInLearning]
(	
	[LearnerId] UNIQUEIDENTIFIER NOT NULL,
	[NumberOfDaysInLearning] INT NOT NULL,
	[CollectionPeriodNumber] TINYINT NOT NULL,
	[CollectionPeriodYear] SMALLINT NOT NULL,
	[CreatedDate] DATETIME2 NOT NULL, 
    [UpdatedDate] DATETIME2 NULL, 
	PRIMARY KEY ([LearnerId], [CollectionPeriodNumber], [CollectionPeriodYear]),
	CONSTRAINT FK_ApprenticeshipDaysInLearning_Learner FOREIGN KEY (LearnerId) REFERENCES [incentives].[Learner](Id)
)
GO