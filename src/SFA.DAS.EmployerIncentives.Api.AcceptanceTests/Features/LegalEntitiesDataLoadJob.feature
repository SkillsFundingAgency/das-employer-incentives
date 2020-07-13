@database
@api
@accountApi
Feature: LegalEntitiesDataLoad
	When legal entities need to be populated in Employer Incentives from Managed Apprenticeships
	Then current Legal entities in Managed Apprenticeships are available in Employer Incentives

Scenario: An initial data load is requested
	Given legal entities exist in Managed Apprenticeships
	When a RefreshLegalEntities job is requested
	Then the legal entities are available in employer incentives
