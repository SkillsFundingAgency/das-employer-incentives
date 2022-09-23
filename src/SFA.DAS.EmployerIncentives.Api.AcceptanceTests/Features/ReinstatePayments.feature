@database
@api
Feature: RevertPayments
	In order to address payment errors that occurred during a payment run
	As a employer incentives service owner
	I want to be able to revert incentives payments that have been marked as failed so that they can be processed again

Scenario: When the payment exists and has previously been paid then the payment is reverted
	Given an apprenticeship exists with a payment marked as paid
	When the revert payments request is sent for a single payment
	Then the payment is reverted

Scenario: When a payment does not exist then an error is returned
	Given an apprenticeship exists with a payment marked as paid
	When the revert payments request is sent with an unmatching payment ID
	Then the payment is not reverted
	And the requester is informed no payment is found

Scenario: When multiple payments exist then the payments are reverted
	Given apprentice incentives exist with payments marked as paid
	When the revert payments request is sent for multiple payments
	Then the payments are reverted
