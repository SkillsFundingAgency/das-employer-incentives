CREATE TABLE [dbo].[IncentiveApplicationStatusAudit]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[IncentiveApplicationApprenticeshipId] UNIQUEIDENTIFIER NOT NULL,
	[Process] NVARCHAR(20) NOT NULL,
	[ServiceRequestTaskId] NVARCHAR(100) NULL,
	[ServiceRequestDecisionReference] NVARCHAR(100) NULL,
	[ServiceRequestCreatedDate] DATETIME2 NULL, 
	[CreatedDateTime] DATETIME2 NOT NULL, 
    CONSTRAINT FK_IncentiveApplicationStatusAudit_IncentiveApplicationApprenticeship FOREIGN KEY (IncentiveApplicationApprenticeshipId) REFERENCES IncentiveApplicationApprenticeship(Id)
)
GO


