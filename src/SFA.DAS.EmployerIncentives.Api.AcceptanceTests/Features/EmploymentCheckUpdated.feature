@database
@api
@messageBus
@domainMessageHandlers
@activeCalendarPeriod
Feature: EmploymentChecks
	In order to validate an apprenticeship incentive
	As the system
	I want to be able to confirm that the apprentice met the employment eligibility at the time of application

Scenario Outline: Employment check is updated
	Given an apprenticeship incentive has submitted a new employment check
	When the employment check result is returned with a result of <checkResultType>
	Then the apprenticeship incentive employment check result is updated to <hasPassed>

Examples:
    | checkResultType | hasPassed |
    | Employed        | true      |
    | NotEmployed     | false     |
    | HMRCUnknown     | false     |
    | NoNINOFound     | false     |
    | NoAccountFound  | false     |

Scenario: Employment check result procesing is delayed during month end processing
	Given an apprenticeship incentive has submitted a new employment check
	When the employment check result is returned during month end payment process is running
	Then the apprenticeship incentive employment check result processing is delayed

Scenario: Employment check result procesing is resumed after month end processing delay
	Given an apprenticeship incentive has submitted a new employment check
	And the employment check result processing has been delayed
	When the employment check result processing resumes
	Then the apprenticeship incentive employment check result is processed