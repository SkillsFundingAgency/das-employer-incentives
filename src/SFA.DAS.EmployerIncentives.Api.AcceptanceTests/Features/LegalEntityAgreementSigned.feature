@database
@api
Feature: LegalEntityAgreementSigned
	In order to prevent applications when an agreement hasnt been signed
	As a policy stakeholder
	I want to know that an employer has signed the legal agreement

Scenario: Legal Agreement signed
	Given the legal entity is already available in Employer Incentives
	When the legal agreement is signed
	Then the employer can apply for incentives
