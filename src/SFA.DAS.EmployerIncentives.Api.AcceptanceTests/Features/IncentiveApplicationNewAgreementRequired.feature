@database
@api
Feature: IncentiveApplicationNewAgreementRequired
	In order to confirm the employment dates of the selected apprenticeships
	As an employer
	I want to retrieve the details of the saved application

Scenario: Employer is submitting employment dates - new agreement required
	Given an employer who has previously signed a Phase2 agreement version
	And the employer has selected the apprenticeships for their application
	And submitted employment dates for the apprenticeships
	And one of the apprenticeships employment date falls into Phase3 window
	When they retrieve the application
	Then the employer is asked to sign a new agreement version

Scenario: Employer is submitting employment dates - new agreement is not required
	Given an employer who has previously signed a Phase2 agreement version
	And the employer has selected the apprenticeships for their application
	And submitted employment dates for the apprenticeships
	And all of the apprenticeships employment dates fall into Phase2 window
	When they retrieve the application
	Then the employer is not asked to sign a new agreement version