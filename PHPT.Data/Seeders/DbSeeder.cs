using PHPT.Common.Constants;
using PHPT.Common.Enums;
using PHPT.Data.Context;
using PHPT.Data.Entities;

namespace PHPT.Data.Seeders;

public static class DbSeeder
{
    public static void SeedData(ApplicationDbContext context)
    {
        if (context.Facilities.Any())
        {
            return;
        }

        var facility = new Facility
        {
            Name = "Plural Health Medical Center",
            Code = "PHMC001",
            Address = "123 Health Street, Lagos",
            Phone = "+234-800-123-4567",
            Email = "info@pluralhealth.com",
            IsActive = true
        };
        context.Facilities.Add(facility);
        context.SaveChanges();

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
        context.SaveChanges();

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
        context.SaveChanges();

        var wallets = patients.Select(p => new PatientWallet
        {
            PatientId = p.Id,
            Balance = 50000.00m,
            Currency = AppConstants.DefaultCurrency,
            LastTransactionDate = DateTime.UtcNow
        }).ToList();
        context.PatientWallets.AddRange(wallets);
        context.SaveChanges();

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
        context.SaveChanges();
    }
}
