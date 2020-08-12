@database
@api
Feature: EligibleApprenticeshipRequested
	In order to select apprenticeships to apply for an incentive for
	As an employer
	I want to know if an apprenticeship is eligible

Scenario: Apprenticeship eligibility is requested
	Given I am applying for the New Apprenticeship Incentive
	When I request the eligibility of an apprenticeship
	Then the status of the apprenticeship is returned

Scenario: Apprenticeship eligibility is requested for a previously used ULN
	Given I am applying for the New Apprenticeship Incentive
	And the ULN has been used on a previously submitted Incentive
	When I request the eligibility of an apprenticeship
	Then the status of the apprenticeship is returned as not eligible
