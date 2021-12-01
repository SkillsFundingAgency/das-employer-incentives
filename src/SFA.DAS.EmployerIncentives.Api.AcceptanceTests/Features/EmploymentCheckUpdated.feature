@database
@api
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
