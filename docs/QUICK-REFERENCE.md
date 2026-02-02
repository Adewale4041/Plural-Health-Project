# Quick Reference: Where Does My Code Go?

A practical guide for developers working with this Onion Architecture implementation.

---

## 🎯 Quick Decision Tree

```
I need to add...                    →  Put it in...
────────────────────────────────────────────────────────────────
A new API endpoint                  →  PHPT.Api/Controllers
A new business rule                 →  PHPT.Business/Services
A new database entity               →  PHPT.Data/Entities
A new shared model/DTO              →  PHPT.Common/DTOs
A new enum                          →  PHPT.Common/Enums
A new constant                      →  PHPT.Common/Constants
A new repository                    →  PHPT.Data/Repositories
A new service                       →  PHPT.Business/Services
Authentication/Authorization logic  →  PHPT.Api/Program.cs or PHPT.Business/Services
Database migration                  →  PHPT.Data/Migrations (auto-generated)
```

---

## 📋 Common Scenarios

### Scenario 1: Adding a New Feature (e.g., "Lab Results")

**Step-by-step:**

1. **PHPT.Common** - Define shared contracts
   ```csharp
   // PHPT.Common/Enums/LabResultStatus.cs
   public enum LabResultStatus { Pending, Ready, Delivered }
   
   // PHPT.Common/DTOs/LabResultDto.cs
   public class LabResultDto { /* properties */ }
   ```

2. **PHPT.Data** - Create entity and repository
   ```csharp
   // PHPT.Data/Entities/LabResult.cs
   public class LabResult : BaseEntity { /* properties */ }
   
   // PHPT.Data/Repositories/Interfaces/ILabResultRepository.cs
   public interface ILabResultRepository : IRepository<LabResult> { }
   
   // PHPT.Data/Repositories/Implementations/LabResultRepository.cs
   public class LabResultRepository : Repository<LabResult>, ILabResultRepository { }
   ```

3. **PHPT.Business** - Implement business logic
   ```csharp
   // PHPT.Business/Services/Interfaces/ILabResultService.cs
   public interface ILabResultService { /* methods */ }
   
   // PHPT.Business/Services/Implementations/LabResultService.cs
   public class LabResultService : ILabResultService
   {
       private readonly IUnitOfWork _unitOfWork;
       // Implement business logic here
   }
   ```

4. **PHPT.Api** - Create controller
   ```csharp
   // PHPT.Api/Controllers/LabResultsController.cs
   [ApiController]
   [Route("api/[controller]")]
   public class LabResultsController : ControllerBase
   {
       private readonly ILabResultService _service;
       // HTTP endpoints here
   }
   ```

5. **PHPT.Api** - Register services
   ```csharp
   // PHPT.Api/Program.cs
   builder.Services.AddScoped<ILabResultService, LabResultService>();
   builder.Services.AddScoped<ILabResultRepository, LabResultRepository>();
   ```

---

### Scenario 2: Modifying Existing Business Logic

**Where to look:**
- PHPT.Business/Services/Implementations/[Feature]Service.cs

**Example:** Change appointment validation rules
```csharp
// File: PHPT.Business/Services/Implementations/AppointmentService.cs
public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto)
{
    // ✅ Add your business logic here
    // ❌ DO NOT add database queries here - use repositories
    // ❌ DO NOT add HTTP logic here - keep it in controllers
}
```

---

### Scenario 3: Adding a New API Endpoint

**Where to add:**
- PHPT.Api/Controllers/[Existing]Controller.cs (if related to existing feature)
- PHPT.Api/Controllers/[New]Controller.cs (if new feature)

**Example:**
```csharp
// File: PHPT.Api/Controllers/PatientsController.cs
[HttpGet("{id}/appointments")]
[ProducesResponseType(typeof(ApiResponse<List<AppointmentDto>>), 200)]
public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetPatientAppointments(Guid id)
{
    // ✅ Call service layer
    var appointments = await _patientService.GetPatientAppointmentsAsync(id);
    
    // ✅ Return wrapped response
    return Ok(new ApiResponse<List<AppointmentDto>>(appointments));
    
    // ❌ DO NOT implement business logic here
}
```

---

### Scenario 4: Adding Database Columns to Existing Entity

**Steps:**

1. **Update Entity** (PHPT.Data/Entities/)
   ```csharp
   // PHPT.Data/Entities/Patient.cs
   public class Patient : BaseEntity
   {
       // ... existing properties
       public string? NewProperty { get; set; }  // ✅ Add here
   }
   ```

2. **Create Migration**
   ```bash
   cd PHPT.Data
   dotnet ef migrations add AddNewPropertyToPatient --startup-project ../PHPT.Api
   ```

3. **Update DTO if needed** (PHPT.Common/DTOs/)
   ```csharp
   // PHPT.Common/DTOs/PatientDto.cs
   public class PatientDto
   {
       // ... existing properties
       public string? NewProperty { get; set; }  // ✅ Add here
   }
   ```

---

## 🚫 Common Mistakes to Avoid

### ❌ DON'T Do This:

1. **Don't reference PHPT.Data from PHPT.Api directly**
   ```csharp
   // ❌ BAD - in a controller
   public class BadController : ControllerBase
   {
       private readonly ApplicationDbContext _context;  // WRONG!
       
       public async Task<IActionResult> GetPatient(Guid id)
       {
           var patient = await _context.Patients.FindAsync(id);  // WRONG!
       }
   }
   ```
   **Why?** This bypasses the business layer and couples the API to database implementation.

2. **Don't put business logic in controllers**
   ```csharp
   // ❌ BAD
   [HttpPost]
   public async Task<IActionResult> CreateAppointment(CreateAppointmentDto dto)
   {
       // ❌ WRONG - business logic in controller
       if (dto.StartTime < DateTime.Now)
           return BadRequest("Appointment cannot be in the past");
           
       // ❌ WRONG - direct database access
       _context.Appointments.Add(new Appointment { ... });
       await _context.SaveChangesAsync();
   }
   ```

3. **Don't add framework dependencies to PHPT.Common**
   ```csharp
   // ❌ BAD - in PHPT.Common.csproj
   <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.2" />
   ```
   **Why?** PHPT.Common must remain framework-independent.

---

### ✅ DO This Instead:

1. **Always go through the service layer**
   ```csharp
   // ✅ GOOD - in a controller
   public class GoodController : ControllerBase
   {
       private readonly IPatientService _patientService;  // CORRECT!
       
       public async Task<IActionResult> GetPatient(Guid id)
       {
           var patient = await _patientService.GetPatientByIdAsync(id);  // CORRECT!
           return Ok(new ApiResponse<PatientDto>(patient));
       }
   }
   ```

2. **Put business logic in services**
   ```csharp
   // ✅ GOOD - in PHPT.Business/Services
   public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto)
   {
       // ✅ CORRECT - business validation
       if (dto.StartTime < DateTime.Now)
           throw new InvalidOperationException("Appointment cannot be in the past");
       
       // ✅ CORRECT - use repositories
       var appointment = new Appointment { /* map from dto */ };
       await _unitOfWork.Appointments.AddAsync(appointment);
       await _unitOfWork.SaveChangesAsync();
   }
   ```

3. **Keep PHPT.Common pure**
   ```xml
   <!-- ✅ GOOD - PHPT.Common.csproj -->
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
     </PropertyGroup>
     <!-- No package references - pure C# -->
   </Project>
   ```

---

## 🔍 Debugging Tips

### "Where is this code being called from?"

**Typical call chain:**
```
HTTP Request
    ↓
Controller (PHPT.Api)
    ↓
Service Interface (PHPT.Business)
    ↓
Service Implementation (PHPT.Business)
    ↓
Repository Interface (PHPT.Data)
    ↓
Repository Implementation (PHPT.Data)
    ↓
DbContext (PHPT.Data)
    ↓
Database
```

### "I need to find where [Feature] is implemented"

1. **Controllers:** PHPT.Api/Controllers/[Feature]Controller.cs
2. **Business Logic:** PHPT.Business/Services/Implementations/[Feature]Service.cs
3. **Data Access:** PHPT.Data/Repositories/Implementations/[Feature]Repository.cs
4. **Database Entity:** PHPT.Data/Entities/[Feature].cs
5. **DTOs:** PHPT.Common/DTOs/[Feature]Dto.cs

---

## 📝 Code Review Checklist

Before submitting a PR, verify:

- [ ] Controllers only handle HTTP concerns (no business logic)
- [ ] Business logic is in service layer (PHPT.Business)
- [ ] Database queries only in repositories (PHPT.Data)
- [ ] No circular dependencies between projects
- [ ] PHPT.Common has no external package dependencies
- [ ] DTOs are in PHPT.Common (if shared) or with the service
- [ ] Proper error handling at each layer
- [ ] Logging added for important operations
- [ ] Interfaces defined before implementations
- [ ] Dependency injection configured in Program.cs

---

## 💡 Pro Tips

1. **When in doubt, look at existing code**
   - See how PatientService is implemented? Do the same for your feature
   - See how AppointmentsController works? Follow the same pattern

2. **Use the Repository Pattern consistently**
   - Don't bypass repositories and query DbContext directly
   - If you need a custom query, add a method to the repository

3. **Keep DTOs separate from Entities**
   - Entities (PHPT.Data) = Database representation
   - DTOs (PHPT.Common) = API/Business representation
   - Map between them in the service layer

4. **Follow naming conventions**
   - Services: `[Feature]Service` (e.g., PatientService)
   - Controllers: `[Feature]Controller` (e.g., PatientsController)
   - Repositories: `[Entity]Repository` (e.g., PatientRepository)
   - DTOs: `[Purpose][Feature]Dto` (e.g., CreatePatientDto)

5. **Test at the right layer**
   - Unit test services with mocked repositories
   - Integration test repositories with in-memory database
   - Test controllers with mocked services

---

## 🎓 Learning Resources

**To understand this project better:**
1. Read [ARCHITECTURE.md](../ARCHITECTURE.md) for detailed explanation
2. Check [ONION-ARCHITECTURE-DIAGRAM.md](./ONION-ARCHITECTURE-DIAGRAM.md) for visual guides
3. Explore existing code in this order:
   - PHPT.Common (simplest, no dependencies)
   - PHPT.Data (entities and repositories)
   - PHPT.Business (services with business logic)
   - PHPT.Api (controllers and setup)

**External Resources:**
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/)

---

## 🤝 Need Help?

If you're still unsure where to put your code:
1. Ask: "What is this code's primary responsibility?"
2. Match it to the layer responsibilities in [ARCHITECTURE.md](../ARCHITECTURE.md)
3. Look for similar existing code in the project
4. When in doubt, ask the team!

---

**Remember:** The goal of this architecture is to make the codebase maintainable, testable, and flexible. Follow the patterns, and you'll write clean, organized code! 🚀
