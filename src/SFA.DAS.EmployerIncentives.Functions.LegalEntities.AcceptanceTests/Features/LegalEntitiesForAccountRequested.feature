@database
@api
Feature: LegalEntitiesForAccountRequested
	In order to manage legal entities
	As an account holder
	I want to be able to retrieve a list of legal entities


Scenario: A list of legal entities is requested
	Given an account with legal entities is in employer incentives
	When a client requests the legal entities for the account
	Then the legal entities are returned
