CREATE TABLE [incentives].[LearningPeriod]
(
    [LearnerId] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME2 NOT NULL, 
    [EndDate] DATETIME2 NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL, 
    PRIMARY KEY ([LearnerId], [StartDate]),
    CONSTRAINT FK_LearningPeriod_Learner FOREIGN KEY (LearnerId) REFERENCES [incentives].[Learner](Id)
)
GO

