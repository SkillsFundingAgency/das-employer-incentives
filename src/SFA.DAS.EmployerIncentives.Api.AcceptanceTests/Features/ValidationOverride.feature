@database
@api
@domainMessageHandlers
@messageBus
@activeCalendarPeriod
Feature: ValidationOverride
	In order to prevent incorrect payment validation results
	As a support user
	I want to be able to override payment validations

Scenario Outline: A payment validation override is created
	Given an apprenticeship incentive has a <validationType> validation
	When the validation override request is received
	Then the validation override <validationType> is stored against the apprenticeship incentive

Examples:
    | validationType                  |
    | HasBankDetails                  |
    | IsInLearning                    |
    | HasLearningRecord               |
    | HasNoDataLocks                  |
    | HasIlrSubmission                |
    | HasDaysInLearning               |
    | PaymentsNotPaused               |
    | HasSignedMinVersion             |
    | LearnerMatchSuccessful          |
    | EmployedBeforeSchemeStarted     |
    | EmployedAtStartOfApprenticeship |

Scenario: A payment validation is not created when no matching apprenticeship incentive exists 

Scenario: A payment validation is not created when the expiry date of the validation is in the past

Scenario: A payment validation is not created when the validation type date of the validation is not valid
