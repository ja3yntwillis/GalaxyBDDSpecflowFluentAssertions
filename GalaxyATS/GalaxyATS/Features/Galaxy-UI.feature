Feature: Galaxy-UI

To verify on implementation of AT 


@UITest
Scenario: Validate login flow
Given I have navigated to "http://App.viabenefits.com" portal
Then I have entered "jayanta.dutta@wtwco.com" in the "wtw-email" field
Then I have entered "Willis@123" in the "wtw-password" field
Then I click on the "//button[contains(text(),'Sign In')]" button
Then I close the browser	
