@database
@api
@accountApi
Feature: UpdateVrfCaseStatusForIncompleteCases
	When a vendor registration form case is in progress
	Then a case status update is requested

Scenario: VRF case statuses are requested for in progress cases
	Given a legal entity has submitted vendor registration form details
	When an UpdateVrfCaseStatusForIncompleteCases job is requested
	Then the case statuses are requested for the legal entities
