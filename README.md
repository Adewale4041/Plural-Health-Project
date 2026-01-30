# Plural Health Project - Front Desk API

A comprehensive ASP.NET Core Web API for managing clinic appointments, patient billing, and wallet-based payments.

## ??? Architecture

The solution follows **Clean Architecture** principles with clear separation of concerns:

### Project Structure

```
PHPT.Common       - Shared models, enums, and constants
PHPT.Data         - Data access layer (Entities, DbContext, Repositories)
PHPT.Business     - Business logic layer (Services, DTOs, Validation)
PHPT.Api          - Presentation layer (Controllers, API configuration)
```

## ?? Features

### Core Functionality

- ? **Patient Appointment Management**
  - List appointments with pagination, filtering, and search
  - Create and schedule appointments
  - Track appointment status transitions
  - Prevent overlapping appointments

- ? **Invoice Management**
  - Create itemized invoices with automatic discount calculation
  - Link invoices to appointments
  - Track invoice status (Unpaid/Paid)
  - Generate unique invoice numbers

- ? **Wallet-Based Payment System**
  - Patient digital wallets with balance tracking
  - Payment processing with transaction history
  - Balance validation before payment
  - Automatic status updates on payment

- ? **Business Rules**
  - Enforce appointment status workflow: Scheduled ? Invoiced ? Paid ? AwaitingVitals
  - Prevent duplicate invoices for appointments
  - Validate sufficient wallet balance
  - Facility-scoped data access

### Technical Features

- ?? Role-based access control ready
- ?? Structured logging with Microsoft.Extensions.Logging
- ?? Repository pattern with Unit of Work
- ?? Entity Framework Core with SQL Server
- ? Async/await throughout
- ??? Comprehensive error handling
- ?? API response standardization
- ?? Full-text search capabilities

## ?? Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code