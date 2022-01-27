﻿@database
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

Scenario Outline: A payment validation is not created when no matching apprenticeship incentive exists 
    Given an apprenticeship incentive has a validation
    When the validation override request is received for a non matching apprenticeship incentive
    Then the validation override is not stored against the apprenticeship incentive

Scenario: An existing validation override is replaced by a new one
    Given an apprenticeship incentive has a validation override
    When the validation override request is received
    Then the validation override is stored against the apprenticeship incentive
    And the exising validation override is archived