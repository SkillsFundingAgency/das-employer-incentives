@database
@api
Feature: ReinstatePayments
	As a employer incentives service owner
	I want to be able to manually reinstate archived pending payments
	So that employers can receive their payments

Scenario: When the payment is archived then it is reinstated
Given a pending payment has been archived for an apprenticeship incentive
When a reinstate request is received
Then the pending payment is reinstated
And a log is written for the reinstate action

Scenario: When the payment cannot be found then an error is returned
Given a pending payment has been archived for an apprenticeship incentive
When a reinstate request is received for a pending payment id that does not match
Then an error is returned


