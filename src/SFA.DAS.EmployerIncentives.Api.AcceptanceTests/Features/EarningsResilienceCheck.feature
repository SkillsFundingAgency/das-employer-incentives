@database
@api
@activeCalendarPeriod
Feature: EarningsResilienceCheck
	In order to ensure that all employers are paid the incentive payment
	As a service owner
	I want to run a resilience check to identify and update any apprenticeships without earnings calculated

Scenario: Earnings Resilience Check is run
	Given there are apprenticeships that do not have earnings calculations
	When the earnings resilience check is requested
	Then the earnings recalculation is triggered

Scenario: Earnings Resilience Check is run for applications with apprenticeships withdrawn by employer
	Given apprenticeships have been withdrawn by employer
	When the earnings resilience check is requested
	Then the earnings recalculation is not triggered

Scenario: Earnings Resilience Check is run for applications with apprenticeships withdrawn by compliance
	Given apprenticeships have been withdrawn by compliance
	When the earnings resilience check is requested
	Then the earnings recalculation is not triggered

Scenario: Earnings Resilience Check is not run when the active period month end processing is in progress
	Given there are apprenticeships that do not have earnings calculations
	And the active period month end processing is in progress
	When the earnings resilience check is requested
	Then the earnings recalculation is not triggered

