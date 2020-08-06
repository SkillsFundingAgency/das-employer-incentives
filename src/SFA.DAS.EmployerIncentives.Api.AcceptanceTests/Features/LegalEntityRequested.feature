@database
@api
Feature: LegalEntityRequested
	In order to verify the legal entity selected can apply for an incentive
	As an employer
	I want to be able to retrieve a legal entity

Scenario: A legal entity is requested
	Given a legal entity that is in employer incentives
	When a client requests the legal entity
	Then the legal entity is returned