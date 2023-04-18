#@database
#@api
#Feature: BlockAccountLegalEntityForPayments
#	In order to prevent payments being sent for an employer that is under investigation
#	As a compliance officer
#	I want to block an account legal entity for payments until investigations are complete
#
#Scenario: Block account legal entities for payment
#	Given there are accounts in employment incentives that have been validated to receive payments
#	When a request to block the account legal entities is received
#	Then the vendor block end dates are set as per the request
#
#Scenario: Block account with multiple legal entity for payment on a single legal entity
#	Given there is an account with multiple legal entities that has been validated to receive payments
#	When a request to block one of the account legal entities is received
#	Then the vendor block end date is set for the single legal entity requested

