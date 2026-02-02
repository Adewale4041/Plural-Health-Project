# Plural Health Project - Front Desk API

A comprehensive ASP.NET Core Web API for managing clinic appointments, patient billing, and wallet-based payments.

##  Architecture

The solution follows **Onion (Clean) Architecture** principles with clear separation of concerns using a custom naming convention.

### Project Structure

```
PHPT.Common       - Core/Domain Layer (Shared models, enums, constants)
PHPT.Data         - Infrastructure Layer (Entities, DbContext, Repositories)
PHPT.Business     - Application Layer (Services, Business Logic, Validation)
PHPT.Api          - Presentation Layer (Controllers, API configuration)
```

### Layer Dependencies (Inward Direction)
```
PHPT.Api → PHPT.Business → PHPT.Data → PHPT.Common
(Outer)                                   (Core)
```

> 📖 **For detailed architecture explanation:** See [ARCHITECTURE.md](./ARCHITECTURE.md) for comprehensive documentation on how this project implements Onion Architecture, layer responsibilities, separation of concerns, and why the custom naming convention (Common/Data/Business/Api) is equivalent to traditional names (Core/Infrastructure/Application/Api).

##  Features

### Core Functionality

-  **Patient Appointment Management**
  - List appointments with pagination, filtering, and search
  - Create and schedule appointments
  - Track appointment status transitions
  - Prevent overlapping appointments

-  **Invoice Management**
  - Create itemized invoices with automatic discount calculation
  - Link invoices to appointments
  - Track invoice status (Unpaid/Paid)
  - Generate unique invoice numbers

-  **Wallet-Based Payment System**
  - Patient digital wallets with balance tracking
  - Payment processing with transaction history
  - Balance validation before payment
  - Automatic status updates on payment

-  **Business Rules**
  - Enforce appointment status workflow: Scheduled  Invoiced  Paid  AwaitingVitals
  - Prevent duplicate invoices for appointments
  - Validate sufficient wallet balance
  - Facility-scoped data access

### Technical Features

-  Role-based access control ready
-  Structured logging with Microsoft.Extensions.Logging
-  Repository pattern with Unit of Work
-  Entity Framework Core with SQL Server
-  Async/await throughout
-  Comprehensive error handling
-  API response standardization
-  Full-text search capabilities

##  Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
