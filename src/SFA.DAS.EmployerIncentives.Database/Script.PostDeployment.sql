/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
:r .\Scripts\Data.CollectionCalendar.sql
:r .\Scripts\Update.ApprenticeshipIncentives.sql
:r .\Scripts\Update.SetPhaseForApplications.sql

/* TODO: remove this script and update Learner.sql once the release has deployed to PROD.
   The DACPAC deployment will not allow a column rename when the table is populated.
 */
:r .\Scripts\Rename.SuccessfulLearnerMatch.sql