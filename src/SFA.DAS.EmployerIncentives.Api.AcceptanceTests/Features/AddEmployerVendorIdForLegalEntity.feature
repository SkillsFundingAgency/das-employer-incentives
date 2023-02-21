@database
@api

Feature: AddEmployerVendorIdForLegalEntity
	When a new Employer Vendor Id is to be assigned to the Legal Entity
	Then the legal entities without an existing vendor are updated

Scenario: A new Employer Vendor Id will be assigned to legal entities who do not already have a vedor assigned
	Given a legal entity exists with a vendor assigned within an account
	And a the same legal entity exists without a vendor assigned for a seperate account    
	When we add the employer vendor for this legal entity  
	Then the vendor remains the same for first legal entity
	And the vendor is updated for the second legal entity