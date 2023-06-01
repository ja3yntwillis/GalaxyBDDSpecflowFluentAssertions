Feature: Galaxy

To verify on implementation of AT 

@DB_Connection
@Test
@Business_cases
Scenario: Validate the active campaigns and all the campaign segments of partner Ford Motor Company are updated correctly in the database
	Given I have connected to database with the necessary connection details
	Then I check the existance of the "Ford Motor Company" in the Partner table below
	| partnerid |
	| 13000021  |
	When I find the partner is existing in the system
	Then I validate the following campaigns are available for the partner
	| campaignId | campaignName                                        |
	| 13102661   | Ford IMM                                            |
	| 13107365   | Ford Medicare hired after 6/1/01                    |
	| 13000021   | Ford Medicare Salaried Retirees hired before 6/1/01 |
	| 13107368   | Ford Pre-Medicare hired after 6/1/01                |
	| 13107363   | Ford Pre-Medicare hired before 6/1/01               |
	And I validate the following campaignsegments of the campaign "Ford Medicare Salaried Retirees hired before 6/1/01"
	| CampaignSegmentName                                                                                          |
	| Ford Medicare - Hourly Non-VEBA (Losing Group Medical and Prescription Drug Coverage)                        |
	| Ford Medicare - Salary (Losing Group Medical and Prescription Drug Coverage)                                 |
	| Ford Medicare Age-Ins: No Current Group Coverage (turning age 65)                                            |
	And I validate the following campaignsegments of the campaign "Ford Pre-Medicare hired before 6/1/01"
	| CampaignSegmentName                                                                                          |
	| Ford Individual and Family Plan Eligible (Losing Group Medical and Prescription Drug Coverage)               |
	| Ford Individual and Family Plan Eligible (Waived, No Current Group Coverage)                                 |
	| Ford Pre-Medicare hired before 6/1/01                                                                        |
	And I validate the following campaignsegments of the campaign "Ford Medicare hired after 6/1/01"
	| CampaignSegmentName                                                                                          |
	| Ford Medicare post 6/1/01 (Losing Group Medical and Prescription Drug Coverage)                              |
	| Ford Medicare post 6/1/01 (Losing Vision Access Only)                                                        |
	| Ford Medicare post 6/1/01 (Waived/Deffered, No Current Group Coverage)                                       |
	And I validate the following campaignsegments of the campaign "Ford Pre-Medicare hired after 6/1/01"
	| CampaignSegmentName                                                                                          |
	| Ford Individual and Family Plan Eligible — post 6/1/01 (Losing Group Medical and Prescription Drug Coverage) |
	| Ford Individual and Family Plan Eligible — post 6/1/01 (Losing Vision)                                       |
	| Ford Individual and Family Plan Eligible — post 6/1/01 (Waived/Deffered, No Current Group Coverage)          |
	And I validate there are no campaignsegments for the campaign "Ford IMM"