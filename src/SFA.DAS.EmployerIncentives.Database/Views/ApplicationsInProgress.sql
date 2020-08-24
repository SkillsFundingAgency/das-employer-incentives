CREATE VIEW [dbo].[ApplicationsInProgress]
	AS 
SELECT
  DATEDIFF(second, '1970-01-01', DateCreated) AS time,
  count(*) as value,
  [Status] as metric
FROM
  IncentiveApplication
WHERE
[Status] = 'InProgress'
GROUP BY DateCreated, [Status]

