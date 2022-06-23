CREATE TABLE [Audit].[VendorBlockAudit]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[VrfVendorId] NVARCHAR(100) NOT NULL,
	[VendorBlockEndDate] DATETIME2 NOT NULL,
	[ServiceRequestTaskId] NVARCHAR(100) NULL,
	[ServiceRequestDecisionReference] NVARCHAR(100) NULL,
	[ServiceRequestCreatedDate] DATETIME2 NULL,
	[CreatedDateTime] DATETIME2 NOT NULL
)
