@database
@api
@activeCalendarPeriod

Feature: ApplicationsForAccountRequested
	In order to manage incentive applications
	As an account holder
	I want to be able to retrieve a list of apprenticeships applied for from my account

Scenario: A list of apprenticeships is requested
	Given an account that is in employer incentives
	When a client requests the apprenticeships for the account
	Then the apprenticeships are returned