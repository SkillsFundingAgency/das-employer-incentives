Feature: PausePayments
	In order to stop future payments for this apprenticeship
	As a employer incentives service
	I want to be able to pause incentives payments for a apprenticeship

Scenario: When an apprenticeship incentive does not exists then we should inform the requester
	Given apprenticeship incentive does not exist
	When the pause payments request is sent
	Then the requester is informed no apprenticeship incentive is found

Scenario: When an apprenticeship incentive exists then future payments should be paused
	Given an apprenticeship incentive exists
	When the pause payments request is sent
	Then the requester is informed the apprenticeship incentive is paused
	And the PausePayment status is set to true
	And an Audit record has been added to record this pause request

Scenario: When an apprenticeship incentive exists, but it's already paused
	Given a paused apprenticeship incentive exists
	When the pause payments request is sent
	Then the requester is informed the apprenticeship incentive is already paused
