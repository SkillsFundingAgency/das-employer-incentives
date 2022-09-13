@activeCalendarPeriod
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

Scenario: When learner match failed
	Given there are pending payments
	And the learner match was unsuccessful
	When the payment process is run
	Then the Learner Match Successful check will have a failed validation result
	And no further ILR validation is performed
	And no payment records are created
	And pending payments are not marked as paid

Scenario: When there is a sent payment clawback
	Given there are pending payments
	And there are payments with sent clawbacks
	When the payment process is run
	Then successful validation results are recorded

Scenario Outline: When at least one validation check fails
	Given there are pending payments
	And the '<ValidationStep>' will fail
	When the payment process is run
	Then the '<ValidationStep>' will have a failed validation result
	And no payment records are created
	And pending payments are not marked as paid

Examples:
	| ValidationStep                  |
	| HasBankDetails                  |
	| IsInLearning                    |
	| HasLearningRecord               |
	| HasNoDataLocks                  |
	| HasIlrSubmission                |
	| HasDaysInLearning               |
	| PaymentsNotPaused               |
	| HasSignedMinVersion             |
	| LearnerMatchSuccessful          |
	| EmployedAtStartOfApprenticeship |
	| EmployedBeforeSchemeStarted     |
	| BlockedForPayments              |

Scenario Outline: Expired validation overrides are removed
	Given there are pending payments	
	And an expired validation override exists for '<ValidationStep>' 
	When the payment process is run
	Then the expired validation override is removed

Examples:
	| ValidationStep                    |
	| IsInLearning                      |
	| HasNoDataLocks                    |
	| HasDaysInLearning                 |
	| EmployedAtStartOfApprenticeship   |
	| EmployedBeforeSchemeStarted       |	

Scenario Outline: When at least one validation check fails and is overriden
	Given there are pending payments
	And there is a validation override for '<ValidationStep>'
	And the '<ValidationStep>' will fail
	When the payment process is run
	Then the validation result override for '<ValidationStep>' is recorded
	And pending payments are marked as paid
	And future payments are not marked as paid

Examples:
	| ValidationStep                    |
	| IsInLearning                      |
	| HasNoDataLocks                    |
	| HasDaysInLearning                 |
	| EmployedAtStartOfApprenticeship   |
	| EmployedBeforeSchemeStarted       |	

Scenario: Employed at 365 days validation check fails
	Given an apprentice has a pending 365 day payment
	And the employed at 365 days check will fail
	When the payment process is run
	Then the employed at 365 days check will have a failed validation result
	And no payment records are created
	And the 365 day pending payment is not marked as paid


Scenario: Employed at 365 days validation check fails and is overriden
	Given an apprentice has a pending 365 day payment
	And the employed at 365 days check will fail
	And there is a validation override for 'EmployedAt365Days'
	When the payment process is run
	Then the validation result override for the 365 days check is recorded
	And the 365 day pending payment is marked as paid