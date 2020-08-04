@database
@api
Feature: IncentiveApplicationCreated
	In order to select apprenticeships to apply for an incentive for
	As an employer
	I want to know if an apprenticeship is eligible

Scenario: Incentive Application is created
	Given An employer is applying for the New Apprenticeship Incentive
	When They have selected the apprenticeships for the application
	Then the application is saved
