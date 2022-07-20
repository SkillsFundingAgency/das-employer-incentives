--------------------------------------------------------
-- EI-1858 Script to update reinstated audit records
--------------------------------------------------------

--------------------------------------------------------
--------------- Updated Submitted to reinstated --------
--------------------------------------------------------
UPDATE [dbo].[IncentiveApplicationStatusAudit]
SET Process = 'Reinstated'
WHERE Process = 'Submitted'

