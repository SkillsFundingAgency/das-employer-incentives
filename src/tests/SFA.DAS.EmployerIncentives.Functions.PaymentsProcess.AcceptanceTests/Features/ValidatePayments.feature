@database
@learnerMatchApi
@functions
Feature: ValidatePayments
	In order to make employer incentives payments
	As a employer incentives service
	I want to be validate and submit payment requests

Scenario: When bank details have been validated successfully
	Given a legal entity has pending payments with valid bank details
	And the apprentice 'is in learning' is true 
	When the payment process is run
	Then successful validation results are recorded
	And payment records are created
	And pending payments are marked as paid
	And future payments are not marked as paid

Scenario: When at least one validation check fails
	Given a legal entity has pending payments with <BankDetailStatus> bank details
	And the apprentice 'is in learning' is <IsInLearning> 
	And the apprenticeship datalock status is '<DataLockStatus>'
	When the payment process is run
	Then bank details validation check is <BankDetailsValidationResult>
	Then apprentice is in learning check is <IsInLearningValidationResult>
	And no payment records are created
	And pending payments are not marked as paid

Examples:
	| BankDetailStatus | IsInLearning | DataLockStatus | BankDetailsValidationResult | IsInLearningValidationResult | DataLockValidationResult |
	| Valid            | false        | no datalocks   | true                        | false                        | true                     |
	| Valid            |              | no datalocks   | true                        | false                        | true                     |
	| Notvalid         | true         | no datalocks   | false                       | true                         | true                     |
	| Valid            | true         | datalocks      | true                        | true                         | false                    |
