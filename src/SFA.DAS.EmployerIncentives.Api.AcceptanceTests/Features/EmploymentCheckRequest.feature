@database
@api
@activeCalendarPeriod
@messageBus
@domainMessageHandlers
@employmentCheckApi
Feature: EmploymentCheckRequest
	In order to allow support to request a new employment check for an apprenticeship
	As the system
	I want to be able to refresh the employment checks for a single apprenticeship

Scenario: Employment check is requested by support
	Given an apprenticeship incentive has been submitted	
	When an employment check refresh is requested
	Then a request is made to refresh the employment checks for the incentive

Scenario: Employment check is requested by support for a incentive that does not exist
	Given an apprenticeship incentive has not been submitted	
	When an employment check refresh is requested
	Then a request is not made to refresh the employment checks for the incentive

Scenario: Employment check is requested by support during month end processing
	Given an apprenticeship incentive has been submitted
	And the active period month end processing is in progress
	When an employment check refresh is requested
	Then the request to refresh the employment checks for the incentive is delayed

Scenario: Employment check result procesing is resumed after month end processing delay
	Given an employment check refresh has been requested by support
	And the employment check refresh processing has been delayed
	When the employment check refresh processing resumes
	Then a request is made to refresh the employment checks for the incentive