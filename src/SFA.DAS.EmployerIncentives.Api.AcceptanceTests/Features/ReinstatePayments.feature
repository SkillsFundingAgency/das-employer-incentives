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
Then a pending payment not found error is returned

Scenario: When the payment has already been paid then the pending payment is restored
Given a pending payment has been archived for an apprenticeship incentive
And the pending payment has already been paid
When a reinstate request is received
Then the pending payment is restored using the payment details

Scenario: When the pending payment already exists then an error is returned
Given a pending payment has been archived for an apprenticeship incentive
And the pending payment already exists
When a reinstate request is received
Then a pending payment already exists error is returned




