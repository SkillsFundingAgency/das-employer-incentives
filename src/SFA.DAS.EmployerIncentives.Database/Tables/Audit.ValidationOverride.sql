CREATE TABLE [audit].[ValidationOverride]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[Step] NVARCHAR(50) NOT NULL, 
	[ExpiryDate] DATETIME2 NOT NULL, 
	[ServiceRequestTaskId] NVARCHAR(100) NULL,
	[ServiceRequestDecisionReference] NVARCHAR(100) NULL,
	[ServiceRequestCreatedDate] DATETIME2 NULL, 
	[CreatedDateTime] DATETIME2 NOT NULL, 
	[DeletedDateTime] DATETIME2 NULL,
    CONSTRAINT FK_AuditValidationOverride_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES [incentives].[ApprenticeshipIncentive](Id)
)
GO
