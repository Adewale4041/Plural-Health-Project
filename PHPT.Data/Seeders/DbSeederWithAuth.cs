using PHPT.Common.Constants;
using PHPT.Common.Enums;
using PHPT.Data.Context;
using PHPT.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace PHPT.Data.Seeders;

public static class DbSeederWithAuth
{
    public static async Task SeedDataAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        // Seed Roles
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await SeedRolesAsync(roleManager);

        // Seed Facilities and Users
        if (!context.Facilities.Any())
        {
            await SeedFacilitiesAndUsersAsync(context, serviceProvider);
        }

        // Seed rest of the data
        if (!context.Clinics.Any())
        {
            await SeedClinicsAsync(context);
        }

        if (!context.Patients.Any())
        {
            await SeedPatientsAndWalletsAsync(context);
        }

        if (!context.Appointments.Any())
        {
            await SeedAppointmentsAsync(context);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles = { UserRoles.Admin, UserRoles.FrontDeskStaff };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private static async Task SeedFacilitiesAndUsersAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var facility = new Facility
        {
            Id = Guid.NewGuid(),
            Name = "Plural Health Medical Center",
            Code = "PHMC001",
            Address = "123 Health Street, Lagos",
            Phone = "+234-800-123-4567",
            Email = "info@pluralhealth.com",
            IsActive = true
        };
        context.Facilities.Add(facility);
        await context.SaveChangesAsync();

        // Create Admin User
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin@pluralhealth.com",
            Email = "admin@pluralhealth.com",
            FirstName = "System",
            LastName = "Administrator",
            FacilityId = facility.Id,
            PhoneNumber = "+234-800-000-0001",
            EmailConfirmed = true,
            IsActive = true
        };

        var adminResult = await userManager.CreateAsync(admin, "Admin123!");
        if (adminResult.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, UserRoles.Admin);
        }

        // Create Front Desk Staff
        var staff = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "staff@pluralhealth.com",
            Email = "staff@pluralhealth.com",
            FirstName = "Sarah",
            LastName = "Johnson",
            FacilityId = facility.Id,
            PhoneNumber = "+234-800-000-0002",
            EmailConfirmed = true,
            IsActive = true,
            CreatedBy = admin.Id
        };

        var staffResult = await userManager.CreateAsync(staff, "Staff123!");
        if (staffResult.Succeeded)
        {
            await userManager.AddToRoleAsync(staff, UserRoles.FrontDeskStaff);
        }
    }

    private static async Task SeedClinicsAsync(ApplicationDbContext context)
    {
        var facility = await context.Facilities.FirstOrDefaultAsync();

        var clinics = new List<Clinic>
        {
            new Clinic
            {
                Name = "General Medicine",
                Code = "GEN",
                Description = "General medical consultations",
                FacilityId = facility.Id,
                IsActive = true
            },
            new Clinic
            {
                Name = "Pediatrics",
                Code = "PED",
                Description = "Children's healthcare",
                FacilityId = facility.Id,
                IsActive = true
            },
            new Clinic
            {
                Name = "Cardiology",
                Code = "CARD",
                Description = "Heart and cardiovascular care",
                FacilityId = facility.Id,
                IsActive = true
            }
        };
        context.Clinics.AddRange(clinics);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPatientsAndWalletsAsync(ApplicationDbContext context)
    {
        var facility = await context.Facilities.FirstAsync();

        var patients = new List<Patient>
        {
            new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                PatientCode = "PAT001",
                Phone = "+234-801-234-5678",
                Email = "john.doe@email.com",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = "Male",
                Address = "45 Patient Street, Lagos",
                FacilityId = facility.Id
            },
            new Patient
            {
                FirstName = "Jane",
                LastName = "Smith",
                PatientCode = "PAT002",
                Phone = "+234-802-345-6789",
                Email = "jane.smith@email.com",
                DateOfBirth = new DateTime(1990, 8, 22),
                Gender = "Female",
                Address = "67 Health Avenue, Lagos",
                FacilityId = facility.Id
            },
            new Patient
            {
                FirstName = "Michael",
                LastName = "Johnson",
                PatientCode = "PAT003",
                Phone = "+234-803-456-7890",
                Email = "michael.j@email.com",
                DateOfBirth = new DateTime(1978, 12, 10),
                Gender = "Male",
                Address = "89 Wellness Road, Lagos",
                FacilityId = facility.Id
            }
        };
        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();

        var wallets = patients.Select(p => new PatientWallet
        {
            PatientId = p.Id,
            Balance = 50000.00m,
            Currency = AppConstants.DefaultCurrency,
            LastTransactionDate = DateTime.UtcNow
        }).ToList();
        context.PatientWallets.AddRange(wallets);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAppointmentsAsync(ApplicationDbContext context)
    {
        var patients = await context.Patients.ToListAsync();
        var clinics = await context.Clinics.ToListAsync();
        var facility = await context.Facilities.FirstAsync();

        var appointments = new List<Appointment>
        {
            new Appointment
            {
                PatientId = patients[0].Id,
                ClinicId = clinics[0].Id,
                AppointmentDate = DateTime.Today,
                AppointmentTime = new TimeSpan(9, 0, 0),
                AppointmentType = "General Consultation",
                Status = AppointmentStatus.Scheduled,
                Notes = "Regular checkup",
                FacilityId = facility.Id
            },
            new Appointment
            {
                PatientId = patients[1].Id,
                ClinicId = clinics[1].Id,
                AppointmentDate = DateTime.Today,
                AppointmentTime = new TimeSpan(10, 30, 0),
                AppointmentType = "Pediatric Checkup",
                Status = AppointmentStatus.Scheduled,
                Notes = "Child vaccination",
                FacilityId = facility.Id
            },
            new Appointment
            {
                PatientId = patients[2].Id,
                ClinicId = clinics[2].Id,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(14, 0, 0),
                AppointmentType = "Cardiology Consultation",
                Status = AppointmentStatus.Scheduled,
                Notes = "Follow-up on heart condition",
                FacilityId = facility.Id
            }
        };
        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();
    }

    // Keep the old synchronous method for backward compatibility
    public static void SeedData(ApplicationDbContext context)
    {
        // This method is now deprecated but kept for backward compatibility
        // Use SeedDataAsync instead
    }
}
