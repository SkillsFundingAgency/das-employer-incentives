@database
@api
Feature: EarningsResilienceCheck
	In order to ensure that all employers are paid the incentive payment
	As a service owner
	I want to run a resilience check to identify and update any apprenticeships without earnings calculated

Scenario: Earnings Resilience Check is run
	Given there are apprenticeships that do not have earnings calculations
	When the earnings resilience check is requested
	Then the earnings recalculation is triggered

