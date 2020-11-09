@database
@api
@accountApi

Feature: UpdateVrfCaseStatusForLegalEntity
	When Vendor Regisgtration Form Status for Legal Entity is changed
	Then Employer Incentives records are updated accordingly

Scenario: VRF case status updated with a general value for legal entity
	Given an existing submitted incentive application
	When VRF case status is changed to 'Some Update Status'
	Then Employer Incentives account legal entity record is updated


Scenario: VRF case status of 'Case request complete' updated for legal entity
	Given an existing submitted incentive application
	When VRF case status is changed to 'Case Request Completed'
	Then Employer Incentives account legal entity record is updated
	And a command to add an Employer Vendor Id Command is sent  