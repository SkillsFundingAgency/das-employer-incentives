#@messageBus
@database
Feature: ValidatePayments
	In order to make employer incentives payments
	As a employer incentives service
	I want to be validate and submit payment requests

Scenario: When bank details have not been validated
	Given a legal entity has pending payments without bank details
	When the payment process is run
	Then the validation fails
	And validation results are recorded
	And pending payments are marked as not payable