@database
@api
Feature: ActivateCollectionCalendarPeriod
	In order to ensure that payments are calculated correctly
	As a service owner
	I want to set the active collection period for the payment process

Scenario: Change of active collection period is requested
	Given there is an active collection period
	When the change of active collection period is requested
	Then the active collection period is changed

