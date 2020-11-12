Feature: ValidatePayments
	In order to make employer incentives payments
	As a employer incentives service
	I want to be validate and submit payment requests

Scenario: When bank details have been validated successfully
	Given a legal entity has pending payments with valid bank details
	When the payment process is run
	Then successful validation results are recorded
	And payment records are created
	And pending payments are marked as paid
	And future payments are not marked as paid

Scenario: When bank details have failed validation
	Given a legal entity has pending payments without bank details
	When the payment process is run
	Then failed validation results are recorded
	And no payment records are created
	And pending payments are not marked as paid
