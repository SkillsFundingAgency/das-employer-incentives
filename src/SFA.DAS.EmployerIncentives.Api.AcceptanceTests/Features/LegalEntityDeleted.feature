@database
@api
@activeCalendarPeriod

Feature: LegalEntityDeleted
	When a legal entity has been removed from an account
	Then is is no longer available in Employer Incentives

Scenario: A legal entity has been removed from an account
	Given a legal entity that is in employer incentives
	When the legal entity is removed from an account
	Then the legal entity should no longer be available in employer incentives

Scenario: A legal entity is not removed from an account
	Given a legal entity that is in employer incentives
	And has submitted one or more applications
	When the legal entity is removed from an account
	Then the legal entity should still have an account

Scenario: A legal entity that has is not available  in employer incentives
	Given the legal entity is in not available in Employer Incentives
	When the legal entity is removed from an account
	Then the legal entity is still not available in employer incentives

Scenario: A legal entity with applications has been removed from an account
	Given a legal entity has submitted one or more applications
	When the legal entity is removed from an account
	Then the applications for that legal entity should be withdrawn
