InvoiceSystem - Invoicing & Payment Reminder System

Don't make a frontend / GUI all logic should be made only for testing purporses.
Do NOT use Try/catch methods


Console application for small businesses that lets users: 
Create and manage invoices
Track payments
Automatically send payment reminders
Apply late fees according to business rules.

Business Logic to Implement and test:
1. Create Invoice
    a. Be able to create a new invoice
2. Invoice Due Date Calculation
	a. 
3. Reminder logic 
	a. Send reminders X days before due, on due date and every Y days after due until paid.
4. Late Fee Calculation
	a. Apply daily/weekly percentage or flat fee after due date.
5. Partial Payments & Balancing
	a. Handle and allocate partial payments toward invoices.


System Architecture:
Backend - ASP.NET Web API
Database: Entity Framework Core with SQLite for testing
No Authentication



Designing Unit Tests:

These design principles are:
1. Tests should be fast
2. Tests should be cohesive, independent, and isolated
3. Tests should have a reason to exist
4. Tests should be repeatable and not flaky
5. Tests should have strong assertions
6. Tests should break if the behavior changes
7. Tests should have a single and clear reason to fail
8. Tests should be easy to write
9. Tests should be easy to read
10. Tests should be easy to change and evolve


Mocking Frameworks 
Use Moq .net mocking framework

Properties of good unit tests:
Maintainability:
1. Remove redundant code in tests
2. Enforce test isolation
	2.1 It should be possible to run the test methods in any order
	2.2 A test method should not call another test method
	2.3 A test method which shares state with other test methods should roll back any 	changes it may make to the shared state. 
Readability:
1. Use the Arrange-Act-Assert Pattern
2. Use good naming conventions 
	2.1 Name of the method being tested
	2.2 Scenario under which the method is being tested
	2.3 Expected behavior when the scenario is invoked
3. Keep the code in tests simple.


