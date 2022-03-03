@activeCalendarPeriod
Feature: ValidatePaymentsFails
	In order to make employer incentives payments
	As a employer incentives service
	I want to be validate and submit payment requests

Scenario: When at least one validation check fails
	Given there are pending payments
	And the '<ValidationStep>' will fail
	When the payment process is run
	Then the '<ValidationStep>' will have a failed validation result
	And no payment records are created
	And pending payments are not marked as paid


Examples:
	| ValidationStep                    |
	| HasBankDetails                    |
	| IsInLearning                      |
	| HasLearningRecord                 |
	| HasNoDataLocks                    |
	| HasIlrSubmission                  |
	| HasDaysInLearning                 |
	| PaymentsNotPaused                 |
	| HasSignedMinVersion               |
	| LearnerMatchSuccessful            |
	| EmployedAtStartOfApprenticeship   |
	| EmployedBeforeSchemeStarted       |