@ignore
@database
@api
@accountApi
Feature: UpdateVrfCaseDetailsForNewApplications
	When case details need to be populated in Employer Incentives from VRF
	Then current Legal entities in Managed Apprenticeships are available in Employer Incentives

Scenario: VRF case details are requested for a submitted application
	Given an application has been submitted
	When an UpdateVrfCaseDetailsForNewApplications job is requested
	Then the case details are requested for the legal entity
