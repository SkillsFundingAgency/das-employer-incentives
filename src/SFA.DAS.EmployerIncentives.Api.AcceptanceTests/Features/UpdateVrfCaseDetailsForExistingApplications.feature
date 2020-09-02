@database
@api
@accountApi

Feature: UpdateVrfCaseDetailsForExistingApplications
	When Case Id, Vendor Id or Status are changed in Vendor Registration Form (VRF)
	Then Employer Incentives records are updated accordingly

Scenario: VRF case, vendor and status updated for legal entity
	Given an existing submitted application
	When VRF case, vendor and status are changed
	Then Employer Incentives legal entity record is updated