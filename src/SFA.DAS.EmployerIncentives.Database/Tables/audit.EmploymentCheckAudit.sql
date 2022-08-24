CREATE TABLE [audit].[EmploymentCheckAudit]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[ServiceRequestTaskId] NVARCHAR(100) NULL,
	[ServiceRequestDecisionReference] NVARCHAR(100) NULL,
	[ServiceRequestCreatedDate] DATETIME2 NULL, 
	[CreatedDateTime] DATETIME2 NOT NULL, 
    CONSTRAINT FK_EmploymentCheckAudit_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES [incentives].[ApprenticeshipIncentive](Id),
	[CheckType] NVARCHAR(50) NULL
)
GO
