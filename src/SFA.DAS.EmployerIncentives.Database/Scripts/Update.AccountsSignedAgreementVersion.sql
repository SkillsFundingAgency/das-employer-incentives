UPDATE 
	dbo.Accounts 
SET 
	SignedAgreementVersion = 4
FROM 
	dbo.Accounts 
WHERE 
	HasSignedIncentivesTerms = 1
AND 
	SignedAgreementVersion IS NULL
