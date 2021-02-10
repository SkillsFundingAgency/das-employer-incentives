Feature: ValidatePayments
	In order to make employer incentives payments
	As a employer incentives service
	I want to be validate and submit payment requests

Scenario: When all validation checks are successful
	Given there are pending payments
	And no validations steps will fail
	When the payment process is run
	Then successful validation results are recorded
	And payment records are created
	And pending payments are marked as paid
	And future payments are not marked as paid

Scenario: When no ILR submission found
	Given there are pending payments
	And the ILR submission validation step will fail
	When the payment process is run
	Then the ILR Submission check will have a failed validation result
	And no further ILR validation is performed
	And no payment records are created
	And pending payments are not marked as paid

Scenario: When there is an unsent payment clawback
	Given there are pending payments
	And there are payments with unsent clawbacks
	When the payment process is run
	Then the 'HasNoUnsentClawbacks' will have a failed validation result

Scenario: When there is a sent payment clawback
	Given there are pending payments
	Given there are payments with sent clawbacks
	When the payment process is run
	Then successful validation results are recorded

Scenario: When at least one validation check fails
	Given there are pending payments
	And the '<ValidationStep>' will fail
	When the payment process is run
	Then the '<ValidationStep>' will have a failed validation result
	And no payment records are created
	And pending payments are not marked as paid


Examples:
	| ValidationStep    |
	| HasBankDetails    |
	| IsInLearning      |
	| HasLearningRecord |
	| HasNoDataLocks    |
	| HasIlrSubmission  |
	| HasDaysInLearning |
	| PaymentsNotPaused |
