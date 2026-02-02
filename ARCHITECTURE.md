# Architecture Documentation

## Onion (Clean) Architecture Implementation

This project implements **Onion Architecture** (also known as Clean Architecture) with a custom naming convention that better reflects the project's domain and purpose.

---

## 📊 Project Structure Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         PHPT.Api                            │
│                   (Presentation Layer)                      │
│  Controllers, API Configuration, Dependency Registration    │
└──────────────────────┬──────────────────────────────────────┘
                       │ depends on
┌──────────────────────▼──────────────────────────────────────┐
│                      PHPT.Business                          │
│                   (Application Layer)                       │
│        Services, Business Logic, DTOs, Validation           │
└──────────────────────┬──────────────────────────────────────┘
                       │ depends on
┌──────────────────────▼──────────────────────────────────────┐
│                       PHPT.Data                             │
│                  (Infrastructure Layer)                     │
│  DbContext, Entities, Repositories, Unit of Work, Seeders   │
└──────────────────────┬──────────────────────────────────────┘
                       │ depends on
┌──────────────────────▼──────────────────────────────────────┐
│                      PHPT.Common                            │
│                      (Core/Domain)                          │
│     Models, Enums, Constants, Shared DTOs, Interfaces       │
└─────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Layer Mapping: Custom Names vs Traditional Names

| **Our Project** | **Traditional Name** | **Purpose** |
|-----------------|----------------------|-------------|
| **PHPT.Common** | **Core/Domain** | Contains enterprise business rules, domain entities, enums, and shared contracts |
| **PHPT.Data** | **Infrastructure** | Implements data access, external services, and framework-specific concerns |
| **PHPT.Business** | **Application** | Contains application-specific business logic and use cases |
| **PHPT.Api** | **Presentation/API** | Handles HTTP requests, routing, and presentation concerns |

---

## 🎯 Why This Still Follows Onion Architecture

### Core Principle: Dependency Inversion

The fundamental principle of Onion Architecture is that **dependencies point inward** - outer layers depend on inner layers, never the reverse.

#### Our Implementation:
```
PHPT.Api → PHPT.Business → PHPT.Data → PHPT.Common
(Outer)                                    (Inner)
```

✅ **Dependency Flow Analysis:**
- ✅ PHPT.Api references only PHPT.Business
- ✅ PHPT.Business references PHPT.Data and PHPT.Common
- ✅ PHPT.Data references only PHPT.Common
- ✅ PHPT.Common has NO external dependencies (pure core)

This creates the characteristic "onion" shape where the core (PHPT.Common) is independent and stable.

---

## 📦 Detailed Layer Breakdown

### 1️⃣ PHPT.Common (Core/Domain Layer)

**Purpose:** The innermost layer containing the most stable, business-critical code.

**Contents:**
- **Enums:** `UserRole`, `AppointmentStatus`, `InvoiceStatus`
- **Constants:** `AppConstants` for application-wide constants
- **Models:** `PagedResult`, `ApiResponse`, `JwtSettings`
- **DTOs:** Shared data transfer objects used across layers
- **No Dependencies:** This layer is completely self-contained

**Why it's the Core:**
- Contains domain models and business rules that rarely change
- No dependencies on other projects or external frameworks
- Can be reused in any context (Web API, Console App, etc.)
- Represents the heart of the business domain

**Separation of Concerns:**
- ✅ Pure C# code, no framework dependencies
- ✅ Defines contracts (DTOs, enums) without implementation details
- ✅ Shared across all layers without coupling

---

### 2️⃣ PHPT.Data (Infrastructure Layer)

**Purpose:** Handles all external concerns and data persistence.

**Contents:**
- **Context:** `ApplicationDbContext` (EF Core DbContext)
- **Entities:** Domain entities with EF Core configurations
- **Repositories:** Data access implementations
  - Interfaces: `IPatientRepository`, `IAppointmentRepository`, etc.
  - Implementations: Concrete repository classes
- **Unit of Work:** Transaction management
- **Migrations:** Database schema versioning
- **Seeders:** Database initialization and test data

**Why it's Infrastructure:**
- Depends on external frameworks (Entity Framework Core, SQL Server)
- Implements data access patterns (Repository, Unit of Work)
- Can be swapped out (e.g., switch from SQL Server to PostgreSQL) without affecting business logic
- Contains framework-specific code

**Separation of Concerns:**
- ✅ Isolates all database and ORM concerns
- ✅ Implements repository interfaces consumed by upper layers
- ✅ Handles migrations and data seeding
- ✅ Only depends on PHPT.Common for domain models and DTOs

---

### 3️⃣ PHPT.Business (Application Layer)

**Purpose:** Orchestrates application workflows and business logic.

**Contents:**
- **Services:**
  - Interfaces: `IPatientService`, `IAppointmentService`, `IInvoiceService`, etc.
  - Implementations: Business logic, validation, orchestration
- **Business Rules:**
  - Appointment workflow enforcement
  - Payment processing logic
  - Invoice generation rules
- **Logging:** Business operation logging

**Why it's Application Layer:**
- Contains use cases specific to this application
- Orchestrates data flow between presentation and data layers
- Implements business processes and validations
- Coordinates repository calls through Unit of Work

**Separation of Concerns:**
- ✅ No HTTP or API-specific code
- ✅ No direct database access (uses repositories)
- ✅ Pure business logic and orchestration
- ✅ Testable without UI or database

**Key Business Rules Implemented:**
```csharp
// Example: Appointment status workflow
Scheduled → Invoiced → Paid → AwaitingVitals

// Wallet-based payment validation
if (wallet.Balance < invoice.TotalAmount)
    throw new InvalidOperationException("Insufficient funds");

// Prevent duplicate invoices
if (appointment.InvoiceId != null)
    throw new InvalidOperationException("Invoice already exists");
```

---

### 4️⃣ PHPT.Api (Presentation Layer)

**Purpose:** The outermost layer handling HTTP requests and responses.

**Contents:**
- **Controllers:** API endpoints
  - `PatientsController`
  - `AppointmentsController`
  - `InvoicesController`
  - `ClinicsController`
  - `AuthController`
- **Program.cs:** Dependency injection, middleware, configuration
- **Configuration:** `appsettings.json`, JWT setup, CORS
- **API-specific concerns:** Routing, model binding, authentication

**Why it's Presentation:**
- Handles HTTP protocol concerns
- Converts DTOs to HTTP responses
- Manages authentication/authorization
- Entry point for external clients

**Separation of Concerns:**
- ✅ No business logic in controllers
- ✅ No database access (delegates to services)
- ✅ Only depends on PHPT.Business
- ✅ Can be replaced with different UI (Blazor, MVC, etc.)

**Controller Example:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    // Controller only coordinates between HTTP and business layer
    [HttpPost]
    public async Task<IActionResult> CreateAppointment(CreateAppointmentDto dto)
    {
        var result = await _appointmentService.CreateAppointmentAsync(dto);
        return Ok(result);
    }
}
```

---

## 🔄 How Separation of Concerns is Maintained

### 1. **Dependency Direction (Inward)**
```
API depends on Business
Business depends on Data and Common
Data depends on Common
Common depends on NOTHING
```

### 2. **Single Responsibility per Layer**

| Layer | Responsibility | What It Does NOT Do |
|-------|----------------|---------------------|
| **PHPT.Common** | Define domain models | ❌ Database access, HTTP, Business logic |
| **PHPT.Data** | Data persistence | ❌ Business rules, HTTP handling |
| **PHPT.Business** | Business logic | ❌ HTTP concerns, direct DB access |
| **PHPT.Api** | HTTP handling | ❌ Business logic, direct DB access |

### 3. **Interface Segregation**

Interfaces are defined in the layer they're consumed in:
- Repository interfaces → Defined in PHPT.Data
- Service interfaces → Defined in PHPT.Business
- This allows implementations to change without affecting consumers

### 4. **Dependency Injection**

All dependencies are injected, enabling:
- ✅ Testability (mock services/repositories)
- ✅ Flexibility (swap implementations)
- ✅ Loose coupling

---

## 🆚 Comparison: Our Naming vs Traditional

### Why Our Naming Makes Sense

**Traditional:**
```
Core → Infrastructure → Application → API
```

**Our Approach:**
```
Common → Data → Business → Api
```

**Advantages of Our Naming:**

1. **More Intuitive for Healthcare Domain**
   - "Business" clearly indicates business logic
   - "Data" obviously handles data persistence
   - "Common" suggests shared utilities

2. **Clearer Purpose**
   - "PHPT.Business" is more descriptive than "Application"
   - "PHPT.Data" is more specific than "Infrastructure"

3. **Better Developer Communication**
   - New developers immediately understand layer purposes
   - Reduces cognitive load ("Where does patient logic go?" → "PHPT.Business")

4. **Still Architecturally Sound**
   - Follows all Onion Architecture principles
   - Maintains proper dependency direction
   - Achieves complete separation of concerns

---

## ✅ Onion Architecture Principles Checklist

Our implementation satisfies all core Onion Architecture principles:

- ✅ **Dependency Inversion:** Dependencies point inward toward the core
- ✅ **Independent Core:** PHPT.Common has no external dependencies
- ✅ **Testability:** Each layer can be tested in isolation
- ✅ **Framework Independence:** Business logic doesn't depend on EF Core or ASP.NET
- ✅ **Database Independence:** Can swap SQL Server for another DB without changing business logic
- ✅ **UI Independence:** Can add Blazor, MVC, or mobile app without changing business/data layers
- ✅ **Single Responsibility:** Each layer has one clear purpose
- ✅ **Open/Closed Principle:** Can extend behavior without modifying existing code

---

## 🎓 Key Takeaways

1. **Naming is Flexible:** Onion Architecture is about structure and dependencies, not specific names.

2. **Your Implementation is Valid:** PHPT.Common/Data/Business/Api follows the same principles as Core/Infrastructure/Application/API.

3. **Dependency Flow Matters Most:** As long as dependencies point inward and layers are properly separated, you're implementing Clean Architecture correctly.

4. **Custom Naming Can Improve Clarity:** Your naming convention is more intuitive for your domain.

5. **Separation of Concerns is Maintained:** Each layer has distinct responsibilities with no overlap.

---

## 📚 Real-World Benefits

### For New Developers:
- Clear understanding of where to put code
- Self-documenting project structure
- Reduced onboarding time

### For Maintenance:
- Changes are isolated to specific layers
- Reduced risk of breaking changes
- Easier to locate and fix bugs

### For Testing:
- Each layer is independently testable
- Mock dependencies easily
- Business logic tests don't require database

### For Scalability:
- Can add new features without restructuring
- Can replace entire layers (e.g., switch from SQL to NoSQL)
- Can add new presentation layers (mobile app, desktop)

---

## 🔍 Verification: Project References

**Project Dependencies (from .csproj files):**

```xml
PHPT.Api → PHPT.Business

PHPT.Business → PHPT.Data
               → PHPT.Common

PHPT.Data → PHPT.Common

PHPT.Common → (No dependencies)
```

This proves the onion structure with PHPT.Common at the core and PHPT.Api at the periphery.

---

## Conclusion

Your project successfully implements Onion (Clean) Architecture with a custom, domain-appropriate naming convention. The structure maintains all the benefits of Clean Architecture:

- ✅ Separation of concerns across layers
- ✅ Proper dependency direction (inward)
- ✅ Testability and maintainability
- ✅ Framework and database independence
- ✅ Clear, intuitive layer responsibilities

**The architecture is sound, and the naming convention enhances clarity rather than diminishing it.**
