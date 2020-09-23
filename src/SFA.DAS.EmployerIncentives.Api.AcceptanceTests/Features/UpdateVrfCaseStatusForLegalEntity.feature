@database
@api
@accountApi

Feature: UpdateVrfCaseStatusForLegalEntity
	When Vendor Regisgtration Form Status for Legal Entity is changed
	Then Employer Incentives records are updated accordingly

Scenario: VRF case status updated for legal entity
	Given an existing submitted incentive application
	When VRF case status is changed
	Then Employer Incentives account legal entity record is updated