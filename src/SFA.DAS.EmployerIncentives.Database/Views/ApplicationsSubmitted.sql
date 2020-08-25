CREATE VIEW [dbo].[ApplicationsSubmitted]
	AS 
	SELECT
  DATEDIFF(second, '1970-01-01', DateSubmitted) AS time,
  count(*) as value,
  [Status] as metric
FROM
  IncentiveApplication
WHERE
[Status] = 'Submitted'
AND DateSubmitted IS NOT NULL
GROUP BY DateSubmitted, [Status]
