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
	And the employment checks feature toggle is set to True
	When an 'InitialEmploymentChecks' employment check refresh is requested
	Then a request is made to refresh the employment checks for the incentive

Scenario: Employment check is requested by support for a incentive that does not exist
	Given an apprenticeship incentive has not been submitted	
	And the employment checks feature toggle is set to True
	When an 'InitialEmploymentChecks' employment check refresh is requested
	Then a request is not made to refresh the employment checks for the incentive

Scenario: Employment check is requested by support during month end processing
	Given an apprenticeship incentive has been submitted
	And the employment checks feature toggle is set to True
	And the active period month end processing is in progress
	When an 'InitialEmploymentChecks' employment check refresh is requested
	Then the request to refresh the employment checks for the incentive is delayed

Scenario: Employment check result procesing is resumed after month end processing delay
	Given an employment check refresh has been requested by support
	And the employment checks feature toggle is set to True
	And the employment check refresh processing has been delayed
	When the employment check refresh processing resumes
	Then a request is made to refresh the employment checks for the incentive

Scenario: Employment check result processing is disabled when the feature toggle is switched off
	Given an apprenticeship incentive has been submitted	
	And the employment checks feature toggle is set to False
	When an 'InitialEmploymentChecks' employment check refresh is requested
	Then a request is not made to the employment checks API

Scenario: Employed at 365 days check is not processed if initial employment checks not run
	Given an apprenticeship incentive has been submitted	
	And the employment checks feature toggle is set to True
	And the apprenticeship incentive has a second earning due for payment
	And the initial employment checks have not been run
	When an 'EmployedAt365DaysCheck' employment check refresh is requested
	Then a request is not made to refresh the employment checks for the incentive

Scenario: Employed at 365 days check is not processed if initial employment checks not passed
	Given an apprenticeship incentive has been submitted	
	And the employment checks feature toggle is set to True
	And the apprenticeship incentive has a second earning due for payment
	And the '<EmploymentCheckType>' employment check has a result of '<EmploymentCheckResult>'
	When an 'EmployedAt365DaysCheck' employment check refresh is requested
	Then a request is not made to refresh the employment checks for the incentive
Examples: 
| EmploymentCheckType             | EmploymentCheckResult |
| EmployedBeforeSchemeStarted     | true                  |
| EmployedAtStartOfApprenticeship | false                 |

Scenario: Employed at 365 days check is not processed if initial checks passed and 365 day check not previously run
	Given an apprenticeship incentive has been submitted	
	And the employment checks feature toggle is set to True
	And the apprenticeship incentive has a second earning due for payment
	And the 'EmployedBeforeSchemeStarted' employment check has a result of 'false'
	And the 'EmployedAtStartOfApprenticeship' employment check has a result of 'true'
	And the 365 day checks have not previously been processed
	When an 'EmployedAt365DaysCheck' employment check refresh is requested
	Then a request is not made to refresh the employment checks for the incentive

Scenario: Employed at 365 days check is processed if initial checks passed and 365 day check previously run
	Given an apprenticeship incentive has been submitted	
	And the employment checks feature toggle is set to True
	And the apprenticeship incentive has a second earning due for payment
	And the 'EmployedBeforeSchemeStarted' employment check has a result of 'false'
	And the 'EmployedAtStartOfApprenticeship' employment check has a result of 'true'
	And the 'EmployedAt365PaymentDueDateFirstCheck' employment check has a result of 'false'
	And the 'EmployedAt365PaymentDueDateSecondCheck' employment check has a result of 'false'
	When an 'EmployedAt365DaysCheck' employment check refresh is requested
	Then a request is made to refresh the 365 day employment check for the incentive