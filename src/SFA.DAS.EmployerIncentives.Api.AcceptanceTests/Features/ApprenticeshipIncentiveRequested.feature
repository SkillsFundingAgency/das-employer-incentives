@database
@api
Feature: ApprenticeshipIncentiveRequested
	In order to select the apprenticeship incentives to be withdrawn
	As an employer
	I want to retrieve apprenticeship incentives which aren't withdrawn

Scenario: Employer is selecting apprenticeship incentives to withdraw
	Given An employer has submitted 
	When They retrieve apprenticeship incentives
	Then the selected apprenticeships are returned