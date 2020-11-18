Feature: RefreshLearnerData
	When the request to refresh learner data for an apprenticeship incentive is received
	Then the learner data is either created or updated

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where no learner data exists
	Given an apprenticeship incentive exists and without a corresponding learner match record
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is created for the application without any submission data

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where learner data exists
	Given an apprenticeship incentive exists and with a corresponding learner match record
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is created for the application with submission data

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where learner data previously refreshed
	Given an apprenticeship incentive exists and has previously been refreshed
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is updated for the application with submission data