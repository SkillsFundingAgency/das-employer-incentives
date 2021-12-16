@database
@api
@activeCalendarPeriod

Feature: EmploymentCheckRefresh
	In order to validate apprenticeship incentives
	As the system
	I want to be able to refresh the employment checks for all apprentices that have a learning record

Scenario Outline: Employment check is refreshed
	Given an apprenticeship incentive has been submitted
	And the employment checks feature toggle is set to True
	And a learner match has been performed for the incentive with a learning found result of True
	When the employment checks are refreshed
	Then a request is made to refresh the employment checks for the incentive

Scenario Outline: Employment check is not refreshed as no learner match run
	Given an apprenticeship incentive has been submitted
	And the employment checks feature toggle is set to True
	And a learner match has not been performed
	When the employment checks are refreshed
	Then a request is not made to refresh the employment checks for the incentive

Scenario Outline: Employment check is not refreshed as no learning record found
	Given an apprenticeship incentive has been submitted
	And the employment checks feature toggle is set to True
	And a learner match has been performed for the incentive with a learning found result of False
	When the employment checks are refreshed
	Then a request is not made to refresh the employment checks for the incentive

Scenario Outline: Employment check is not refreshed when month end in progress
	Given an apprenticeship incentive has been submitted
	And the employment checks feature toggle is set to True
	And the active period month end processing is in progress
	When the employment checks are refreshed
	Then a request is not made to refresh the employment checks for the incentive

Scenario Outline: Employment check is not refreshed when incentive is withdrawn
	Given an apprenticeship incentive has been submitted and subsequently withdrawn
	And the employment checks feature toggle is set to True
	And a learner match has been performed for the incentive with a learning found result of True
	When the employment checks are refreshed
	Then a request is not made to refresh the employment checks for the incentive

Scenario Outline: Employment check is not refreshed when feature toggle is off
	Given an apprenticeship incentive has been submitted and subsequently withdrawn
	And the employment checks feature toggle is set to False
	And a learner match has been performed for the incentive with a learning found result of True
	When the employment checks are refreshed
	Then a request is not made to refresh the employment checks for the incentive
