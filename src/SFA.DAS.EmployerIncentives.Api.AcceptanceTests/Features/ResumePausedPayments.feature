@database
@api
@activeCalendarPeriod
Feature: ResumePausedPayments
	In order to resume payments for a apprenticeship
	As a employer incentives service
	I want to be able to resume a previously paused incentives payments for a apprenticeship

Scenario: When an apprenticeship incentive exists and is already paused
	Given a paused apprenticeship incentive exists
	When the resume payments request is sent
	Then the requester is informed the apprenticeship incentive has resumed
	And the PausePayment status is set to false
	And an Audit record has been added to record this resume request

Scenario: When an apprenticeship incentive exists, but it's not paused
	Given a non paused apprenticeship incentive exists
	When the resume payments request is sent
	Then the requester is informed the apprenticeship incentive is not paused

Scenario: When an invalid request is sent
	Given a paused apprenticeship incentive exists
	When an invalid request is sent
	Then the requester is informed the request is invalid

Scenario: When multiple resume payment requests are made to existing paused incentives
	Given multiple paused apprenticeship incentives exist
	When the multiple resume payments request is sent
	Then the requester is informed the apprenticeship incentives have resumed
	And the PausePayment status for all incentives is set to false
	And an Audit record has been added to record all incentives the resume request

Scenario: When period end is in progess, resume paused payment requests are delayed
	Given a paused apprenticeship incentive exists
	And the active period month end processing is in progress
	When the resume payments request is sent
	Then the requester is informed the apprenticeship incentive pause request has been queued
	And the PausePayment status is set to true
	And Audit records are not created