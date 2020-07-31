@database
@api
Feature: IncentiveApplicationSubmitted
	In order to claim the incentive payment
	As an employer 
	I want to submit an incentive claim application

Scenario: Incentive Application is submitted
	Given an employer has entered incentive claim application details
	When the application is submitted
	Then the application status is updated to reflect completion

Scenario: Incentive Application is submitted with invalid application id
	Given an employer has entered incentive claim application details
	When the invalid application id is submittted
	Then the application status is not updated
	And the service responds with an error

Scenario: Incentive Application is submitted with invalid account id
	Given an employer has entered incentive claim application details
	When the invalid account id is submittted
	Then the application status is not updated
	And the service responds with an error
