# Onion Architecture Visual Diagrams

This document provides visual representations of the project's Onion Architecture implementation.

---

## 🧅 Complete Onion Architecture Diagram

```
╔═══════════════════════════════════════════════════════════════════╗
║                          PHPT.Api                                 ║
║                     (Presentation Layer)                          ║
║  ┌─────────────────────────────────────────────────────────────┐  ║
║  │ Controllers:                                                │  ║
║  │ - PatientsController                                        │  ║
║  │ - AppointmentsController                                    │  ║
║  │ - InvoicesController                                        │  ║
║  │ - ClinicsController                                         │  ║
║  │ - AuthController                                            │  ║
║  │                                                             │  ║
║  │ Configuration:                                              │  ║
║  │ - Program.cs (DI, Middleware)                               │  ║
║  │ - JWT Authentication                                        │  ║
║  │ - Swagger/OpenAPI                                           │  ║
║  └─────────────────────────────────────────────────────────────┘  ║
╚═══════════════════════════════════╤═══════════════════════════════╝
                                    │ Depends On
                                    ▼
╔═══════════════════════════════════════════════════════════════════╗
║                       PHPT.Business                               ║
║                    (Application Layer)                            ║
║  ┌─────────────────────────────────────────────────────────────┐  ║
║  │ Service Interfaces:              Service Implementations:   │  ║
║  │ - IPatientService                - PatientService           │  ║
║  │ - IAppointmentService            - AppointmentService       │  ║
║  │ - IInvoiceService                - InvoiceService           │  ║
║  │ - IClinicService                 - ClinicService            │  ║
║  │ - IAuthService                   - AuthService              │  ║
║  │                                                             │  ║
║  │ Business Logic:                                             │  ║
║  │ - Appointment workflow enforcement                          │  ║
║  │ - Payment processing logic                                  │  ║
║  │ - Invoice generation rules                                  │  ║
║  │ - Business validation                                       │  ║
║  └─────────────────────────────────────────────────────────────┘  ║
╚═══════════════════════════════════╤═══════════════════════════════╝
                                    │ Depends On
                                    ▼
╔═══════════════════════════════════════════════════════════════════╗
║                        PHPT.Data                                  ║
║                   (Infrastructure Layer)                          ║
║  ┌─────────────────────────────────────────────────────────────┐  ║
║  │ DbContext:                       Repositories:              │  ║
║  │ - ApplicationDbContext           - IPatientRepository       │  ║
║  │                                  - IAppointmentRepository   │  ║
║  │ Entities:                        - IInvoiceRepository       │  ║
║  │ - Patient                        - IWalletRepository        │  ║
║  │ - Appointment                    - IClinicRepository        │  ║
║  │ - Invoice                        - IFacilityRepository      │  ║
║  │ - Wallet                                                    │  ║
║  │ - WalletTransaction              Unit of Work:              │  ║
║  │ - ApplicationUser                - IUnitOfWork              │  ║
║  │                                                             │  ║
║  │ Database Concerns:                                          │  ║
║  │ - Migrations                                                │  ║
║  │ - Seeders                                                   │  ║
║  │ - EF Core Configurations                                    │  ║
║  └─────────────────────────────────────────────────────────────┘  ║
╚═══════════════════════════════════╤═══════════════════════════════╝
                                    │ Depends On
                                    ▼
╔═══════════════════════════════════════════════════════════════════╗
║                       PHPT.Common                                 ║
║                   (Core/Domain Layer)                             ║
║  ┌─────────────────────────────────────────────────────────────┐  ║
║  │ Enums:                          Models:                     │  ║
║  │ - UserRole                       - PagedResult<T>           │  ║
║  │ - AppointmentStatus              - ApiResponse<T>           │  ║
║  │ - InvoiceStatus                  - JwtSettings              │  ║
║  │                                                             │  ║
║  │ Constants:                       DTOs:                      │  ║
║  │ - AppConstants                   - CreateClinicDto          │  ║
║  │                                  - PatientDto               │  ║
║  │ ⭐ NO EXTERNAL DEPENDENCIES      - CreateAppointmentDto     │  ║
║  │ ⭐ PURE C# CODE                  - And many more...         │  ║
║  └─────────────────────────────────────────────────────────────┘  ║
╚═══════════════════════════════════════════════════════════════════╝
                              CORE (Independent)
```

---

## 🔄 Request Flow Through Layers

```
┌────────────┐
│   Client   │ (Mobile/Web App)
└──────┬─────┘
       │ HTTP Request (POST /api/appointments)
       ▼
┌──────────────────────────────────────────────┐
│  PHPT.Api - Presentation Layer               │
│  ┌────────────────────────────────────────┐  │
│  │ AppointmentsController                 │  │
│  │ - Receives HTTP request                │  │
│  │ - Validates model binding              │  │
│  │ - Calls IAppointmentService            │  │
│  └────────────────┬───────────────────────┘  │
└─────────────────────┼────────────────────────┘
                     │
                     ▼
┌──────────────────────────────────────────────┐
│  PHPT.Business - Application Layer           │
│  ┌────────────────────────────────────────┐  │
│  │ AppointmentService                     │  │
│  │ - Executes business logic              │  │
│  │ - Validates business rules             │  │
│  │ - Calls repositories via UnitOfWork    │  │
│  │ - Logs operations                      │  │
│  └────────────────┬───────────────────────┘  │
└─────────────────────┼────────────────────────┘
                     │
                     ▼
┌──────────────────────────────────────────────┐
│  PHPT.Data - Infrastructure Layer            │
│  ┌────────────────────────────────────────┐  │
│  │ AppointmentRepository                  │  │
│  │ - Queries database via EF Core         │  │
│  │ - Maps entities                        │  │
│  │ - Returns data                         │  │
│  └────────────────┬───────────────────────┘  │
└─────────────────────┼────────────────────────┘
                     │
                     ▼
┌──────────────────────────────────────────────┐
│  PHPT.Common - Core Layer                    │
│  ┌────────────────────────────────────────┐  │
│  │ Used Throughout:                       │  │
│  │ - AppointmentStatus enum               │  │
│  │ - CreateAppointmentDto                 │  │
│  │ - ApiResponse<T>                       │  │
│  └────────────────────────────────────────┘  │
└──────────────────────────────────────────────┘
       │
       │ Response flows back up
       ▼
┌────────────┐
│   Client   │ Receives ApiResponse<AppointmentDto>
└────────────┘
```

---

## 📦 Project Dependencies Graph

```
                    ┌──────────────┐
                    │  PHPT.Api    │
                    │  (Web API)   │
                    └──────┬───────┘
                           │
                    references
                           │
                           ▼
                    ┌──────────────┐
                    │PHPT.Business │◄────────┐
                    │ (Services)   │         │
                    └──────┬───────┘         │
                           │              references
                    references              │
                           │                 │
                 ┌─────────┴─────────┐       │
                 ▼                   ▼       │
          ┌─────────────┐     ┌─────────────┴┐
          │ PHPT.Data   │────►│ PHPT.Common  │
          │(Repository) │     │    (Core)    │
          └─────────────┘     └──────────────┘
               references
```

**Key Points:**
- PHPT.Common has **zero outgoing dependencies** (core principle of Onion Architecture)
- PHPT.Api only knows about PHPT.Business (not Data or Common directly)
- Each layer only depends on layers closer to the core

---

## 🎯 Separation of Concerns Visualization

```
┌─────────────────────────────────────────────────────────────────┐
│                    CONCERNS SEPARATION                          │
└─────────────────────────────────────────────────────────────────┘

HTTP/REST Concerns        │  PHPT.Api
- Routing                 │  - Controllers
- Model Binding           │  - Program.cs
- Authentication          │  - Middleware
- API Documentation       │  - Swagger Config
──────────────────────────┼────────────────────────────────────────
Business Logic Concerns   │  PHPT.Business
- Use Cases               │  - Services
- Validation              │  - Business Rules
- Workflow Orchestration  │  - Logging
- Business Rules          │  - Workflow Logic
──────────────────────────┼────────────────────────────────────────
Data Access Concerns      │  PHPT.Data
- Database Queries        │  - Repositories
- ORM Configuration       │  - DbContext
- Migrations              │  - Entities
- Transaction Management  │  - Unit of Work
- Data Seeding            │  - Seeders
──────────────────────────┼────────────────────────────────────────
Domain Concerns           │  PHPT.Common
- Business Models         │  - DTOs
- Shared Types            │  - Enums
- Constants               │  - Constants
- Cross-cutting Models    │  - Shared Models
──────────────────────────┴────────────────────────────────────────
```

---

## 🔀 Comparison: Traditional vs Our Naming

```
TRADITIONAL ONION          OUR IMPLEMENTATION        PURPOSE
     NAMES                      NAMES

┌──────────────┐           ┌──────────────┐
│     API      │           │   PHPT.Api   │      HTTP Endpoints
└──────────────┘           └──────────────┘
       │                          │
       ▼                          ▼
┌──────────────┐           ┌──────────────┐
│ Application  │           │PHPT.Business │      Business Logic
└──────────────┘           └──────────────┘
       │                          │
       ▼                          ▼
┌──────────────┐           ┌──────────────┐
│Infrastructure│           │  PHPT.Data   │      Data Access
└──────────────┘           └──────────────┘
       │                          │
       ▼                          ▼
┌──────────────┐           ┌──────────────┐
│  Core/Domain │           │ PHPT.Common  │      Domain Models
└──────────────┘           └──────────────┘
```

**Both are valid** - The structure and dependencies are identical!

---

## 🛡️ Dependency Inversion Principle in Action

```
┌─────────────────────────────────────────────────────────────────┐
│              HOW DEPENDENCIES ARE INVERTED                      │
└─────────────────────────────────────────────────────────────────┘

PHPT.Business (High-Level)
   │
   │ depends on abstraction (interface)
   │
   └──► IPatientRepository (Interface in PHPT.Data)
              ▲
              │ implements
              │
   PatientRepository (Concrete in PHPT.Data)
              │
              │ uses
              ▼
        DbContext (Infrastructure Detail)


Key Points:
✅ High-level modules (Business) don't depend on low-level modules (Data)
✅ Both depend on abstractions (Interfaces)
✅ Abstractions don't depend on details
✅ Details depend on abstractions
```

---

## 🧪 Testability Visualization

```
┌────────────────────────────────────────────────────────────────┐
│                  TESTING EACH LAYER                            │
└────────────────────────────────────────────────────────────────┘

PHPT.Api Tests
┌─────────────────────┐
│ Controller Tests    │──► Mock IAppointmentService
│ - Test routing      │──► No real database needed
│ - Test model binding│──► No business logic needed
└─────────────────────┘

PHPT.Business Tests
┌─────────────────────┐
│ Service Tests       │──► Mock IRepository
│ - Test business     │──► Mock IUnitOfWork
│   logic in isolation│──► No database needed
│ - Test validation   │──► No HTTP needed
└─────────────────────┘

PHPT.Data Tests
┌─────────────────────┐
│ Repository Tests    │──► In-Memory Database
│ - Test queries      │──► No business logic
│ - Test mappings     │──► No HTTP layer
└─────────────────────┘

PHPT.Common Tests
┌─────────────────────┐
│ Model/Enum Tests    │──► Pure C# testing
│ - Test DTOs         │──► No external deps
│ - Test enums        │──► Fast & simple
└─────────────────────┘
```

---

## 🚀 Why This Architecture Scales

```
┌────────────────────────────────────────────────────────────────┐
│              EASY TO EXTEND WITHOUT BREAKING                   │
└────────────────────────────────────────────────────────────────┘

Add New Presentation Layer (Mobile App):
   ┌──────────────┐
   │ PHPT.Mobile  │──► References PHPT.Business
   └──────────────┘    ✅ No changes to Business/Data/Common

Add New Database (Switch to PostgreSQL):
   ┌──────────────┐
   │  PHPT.Data   │──► Update DbContext & connection string
   └──────────────┘    ✅ No changes to Business/Api/Common

Add New Business Feature:
   ┌──────────────┐
   │PHPT.Business │──► Add new service
   └──────────────┘    ✅ No changes to Data/Common
                       ✅ Add new controller in Api

Add New Shared Model:
   ┌──────────────┐
   │ PHPT.Common  │──► Add new DTO/Enum
   └──────────────┘    ✅ Available to all layers
```

---

## 📝 Summary

This Onion Architecture implementation with custom naming:

✅ **Maintains all Clean Architecture principles**
✅ **Clear separation of concerns**
✅ **Testable in isolation**
✅ **Framework independent at the core**
✅ **Flexible and maintainable**
✅ **Intuitive naming for the healthcare domain**

The naming convention (Common/Data/Business/Api) is **semantically equivalent** to traditional names (Core/Infrastructure/Application/Api) and provides **better domain clarity**.
