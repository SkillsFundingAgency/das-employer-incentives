#@database
#@api
#@activeCalendarPeriod
#Feature: PausePayments
#	In order to stop future payments for an apprenticeship
#	As a employer incentives service
#	I want to be able to pause incentives payments for an apprenticeship
#
#Scenario: When an apprenticeship incentive does not exist then we should inform the requester
#	Given apprenticeship incentive does not exist
#	When the pause payments request is sent
#	Then the requester is informed no apprenticeship incentive is found
#
#Scenario: When an apprenticeship incentive exists then future payments should be paused
#	Given an apprenticeship incentive exists
#	When the pause payments request is sent
#	Then the requester is informed the apprenticeship incentive is paused
#	And the PausePayment status is set to true
#	And an Audit record has been added to record this pause request
#
#Scenario: When an apprenticeship incentive exists, but it's already paused
#	Given a paused apprenticeship incentive exists
#	When the pause payments request is sent
#	Then the requester is informed the apprenticeship incentive is already paused
#
#Scenario: When multiple pause payment requests are made
#	Given multiple apprenticeship incentives exist
#	When the multiple pause payments request is sent
#	Then the requester is informed the apprenticeship incentives are paused
#	And the PausePayment status for all incentives is set to true
#	And an Audit record has been added to record all incentives in the pause request
#
#Scenario: When multiple pause payment requests are made and one fails
#	Given multiple apprenticeship incentives exist
#	When the multiple pause payments request is sent with one that is invalid
#	Then the requester is informed the request has failed
#	And the PausePayment status for all incentives remains as false
#	And Audit records are not created
#
#Scenario: When period end is in progess, pause payment requests are delayed
#	Given an apprenticeship incentive exists
#	And the active period month end processing is in progress
#	When the pause payments request is sent
#	Then the requester is informed the apprenticeship incentive pause request has been queued
#	And The PausePayment status for the incentive remains as false
#	And Audit records are not created