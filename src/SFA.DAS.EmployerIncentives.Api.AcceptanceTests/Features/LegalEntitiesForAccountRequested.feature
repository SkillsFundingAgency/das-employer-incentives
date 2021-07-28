@database
@api
Feature: LegalEntitiesForAccountRequested
	In order to manage legal entities
	As an account holder
	I want to be able to retrieve a list of legal entities

Scenario: A list of legal entities is requested
	Given an existing Employer Incentives account 
	And a legal entity who signed version 5 of the agreement
	And a legal entity who signed version 6 of the agreement
	When a client requests the legal entities for the account
	Then the legal entities are returned
	And a property is set indicating whether the minimum required agreement version is signed
