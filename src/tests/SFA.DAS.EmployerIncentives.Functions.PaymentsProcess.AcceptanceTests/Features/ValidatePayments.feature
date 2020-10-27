@database
@functions
Feature: ValidatePayments
	In order to make employer incentives payments
	As a employer incentives service
	I want to be validate and submit payment requests

Scenario: When bank details have not been validated
	Given a legal entity has pending payments without bank details
	When the payment process is run
	Then the validation fails and validation results are recorded

Scenario: When bank details have been validated successfully
	Given a legal entity has pending payments with valid bank details
	When the payment process is run
	Then the validation succeeds and validation results are recorded
