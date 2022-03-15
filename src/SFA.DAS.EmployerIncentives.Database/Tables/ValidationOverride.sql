CREATE TABLE [incentives].[ValidationOverride]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[Step] NVARCHAR(50) NOT NULL, 
	[ExpiryDate] DATETIME2 NOT NULL, 
	[CreatedDateTime] DATETIME2 NOT NULL,	
    CONSTRAINT FK_ValidationOverride_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES [incentives].[ApprenticeshipIncentive](Id)
)
GO
