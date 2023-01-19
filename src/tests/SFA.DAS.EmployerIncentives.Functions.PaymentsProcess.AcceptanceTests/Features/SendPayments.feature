@activeCalendarPeriod
Feature: SendPayments
	In order to make employer incentives payments
	As a employer incentives service
	I want to be send payments to business central once they are validated

Scenario: When payments are approved they are sent to Business Central
	Given payments exist for a legal entity
	When the payments have been approved
	Then the payments are sent to Business Central

Scenario: When clawbacks are approved they are sent to Business Central
	Given clawbacks exist for a legal entity
	When the payments have been approved
	Then the clawbacks are sent to Business Central
