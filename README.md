# Invoice System

A clean architecture ASP.NET Core Web API for managing invoices, tracking payments, sending payment reminders, and applying late fees.

## Project Structure

The solution follows Clean Architecture principles and is organized into the following projects:

- **InvoiceSystem.Domain**: Contains all entities, enums, exceptions, interfaces, types and logic specific to the domain layer.
- **InvoiceSystem.Application**: Contains all application logic. It is dependent on the domain layer, but has no dependencies on any other layer or project.
- **InvoiceSystem.Infrastructure**: Contains all infrastructure concerns, including database, external APIs, file systems, etc.
- **InvoiceSystem.API**: Contains all API endpoints, controllers, and related configurations.
- **InvoiceSystem.Tests**: Contains all unit tests for the application.

## Features

- **Invoice Management**: Create, read, update, and delete invoices
- **Payment Tracking**: Record full or partial payments for invoices
- **Payment Reminders**: Automatically generate reminders for upcoming and overdue invoices
- **Late Fee Calculation**: Apply late fees to overdue invoices based on configurable rules

## Technology Stack

- **.NET 9.0**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQLite** (for development/testing purposes)
- **xUnit** (for unit testing)
- **Moq** (for mocking in tests)
- **Swagger/OpenAPI** (for API documentation)

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later

### Setup and Running

1. Clone the repository
2. Navigate to the solution folder
3. Run the following commands:

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the tests
dotnet test

# Run the API
cd InvoiceSystem.API
dotnet run
```

The API will be available at `https://localhost:5001` and the Swagger UI at `https://localhost:5001/swagger`.

## API Endpoints

### Invoices

- `GET /api/invoices`: Get all invoices
- `GET /api/invoices/{id}`: Get invoice by ID
- `GET /api/invoices/{id}/details`: Get invoice with payment details
- `POST /api/invoices`: Create a new invoice
- `PUT /api/invoices/{id}`: Update an existing invoice
- `DELETE /api/invoices/{id}`: Delete an invoice

### Payments

- `GET /api/payments/invoice/{invoiceId}`: Get all payments for an invoice
- `GET /api/payments/{id}`: Get payment by ID
- `POST /api/payments`: Record a new payment
- `PUT /api/payments/{id}`: Update an existing payment
- `DELETE /api/payments/{id}`: Delete a payment

### Reminders

- `GET /api/reminders/invoice/{invoiceId}`: Get all reminders for an invoice
- `GET /api/reminders/{id}`: Get reminder by ID
- `POST /api/reminders`: Create a new reminder
- `POST /api/reminders/send`: Send a reminder
- `GET /api/reminders/invoices-needing-reminders`: Get invoices needing reminders

### Late Fees

- `GET /api/latefees/invoice/{invoiceId}`: Get all late fees for an invoice
- `GET /api/latefees/{id}`: Get late fee by ID
- `POST /api/latefees`: Apply a new late fee
- `GET /api/latefees/calculate/{invoiceId}`: Calculate late fee for an invoice
- `GET /api/latefees/invoices-needing-fees`: Get invoices that need late fees applied

## Testing

The project includes unit tests for the domain entities and application services. Run the tests with:

```bash
dotnet test
```

## License

This project is licensed under the MIT License. 