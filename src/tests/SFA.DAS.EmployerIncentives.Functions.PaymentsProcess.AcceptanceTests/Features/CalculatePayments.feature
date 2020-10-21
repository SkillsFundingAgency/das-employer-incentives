@messageBus
Feature: CalculatePayments
	In order to make employer incentives payments
	As a employer incentives service
	I want to be validate and submit payment requests

Scenario: When bank details have not been validated
	Given a legal entity does not have a valid vendor Id
	When pending payments for the legal entity are validated
	Then the validation fails
	And validation results are recorded
	And pending payment is marked as not payable