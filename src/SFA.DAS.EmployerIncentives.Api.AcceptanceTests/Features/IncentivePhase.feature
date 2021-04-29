@database
@api
@domainMessageHandlers
@messageBus
@activeCalendarPeriod
Feature: IncentivePhase
	When an application is submitted then the phase of the application is stored against the application and the created incentive

Scenario: Incentive Application is submitted
	Given an employer is applying for an apprenticeship with a start date of '<StartDate>'
	When they submit the application on '<SubmissionDate>'
	Then the apprenticeship incentive phase for the the application is '<Phase>'
	And the apprenticeship incentive is created with an incentive phase of '<Phase>'

Examples:
	| StartDate  | SubmissionDate | Phase    |
	| 2020-08-01 | 2021-06-01     | NotSet   |
	| 2020-08-01 | 2021-05-31     | Phase1_0 |
	| 2020-08-02 | 2021-05-31     | Phase1_0 |
	| 2021-01-31 | 2021-05-31     | Phase1_0 |
	| 2021-02-01 | 2021-05-31     | Phase1_1 |
	| 2021-02-02 | 2021-05-31     | Phase1_1 |
	| 2021-03-31 | 2021-05-31     | Phase1_1 |
	| 2021-04-01 | 2021-05-31     | NotSet   |