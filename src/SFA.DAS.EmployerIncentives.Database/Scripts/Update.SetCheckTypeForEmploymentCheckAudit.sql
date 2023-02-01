--------------------------------------------------------
-- EI-1781 Script to default CheckType in 
-- 		   EmploymentCheckAudit table
--------------------------------------------------------

--------------------------------------------------------
--------------- Initialise CheckType to Unknown --------
--------------------------------------------------------
UPDATE [audit].[EmploymentCheckAudit]
SET CheckType = 'Unknown'
WHERE CheckType IS NULL
