using HealthManager.Models;
using HealthManager.Services.Appointments;
using HealthManager.Services.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthManagerIntegrationTests.Helpers
{
    internal class SeedMethods
    {
        public static async Task SeedAsync(HealthManagerContext db)
        {
            if (!db.Specialties.Any())
            {
                db.Specialties.Add(new Specialty { SpecialtyName = "Neurología" });
                await db.SaveChangesAsync();
            }

            if (!db.Doctors.Any())
            {
                var doctor = new Doctor
                {
                    Name = "Mariano",
                    Surname = "Benitez",
                    Specialty = 1,
                    Email = "marianobenitez@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("DefaultPass0213++"),
                    LicenseNumber = 44444,
                    PhoneNumber =45678901,
                    Role = "Doctor"
                };
                db.Doctors.Add(doctor);

                var wd = new WorkingDay
                {
                    DoctorId = 1,
                    Monday = true,
                    Tuesday = true,
                    Wednesday = true,
                    Thursday = true,
                    Friday = true,
                    Saturday = true,
                    Sunday = true
                };
                await db.WorkingDays.AddAsync(wd);
                await db.SaveChangesAsync();

                var doctorShift = new DoctorShift
                {
                    DoctorId = 1,
                    ShiftStart = new TimeOnly(10, 00),
                    ShiftEnd = new TimeOnly(18, 30),
                    ConsultDuration = new TimeOnly(00, 20)
                };
                await db.DoctorShifts.AddAsync(doctorShift);

                
                await db.SaveChangesAsync();
            }

            if (!db.Patients.Any())
            {
                db.Patients.Add(new Patient
                {
                    Name = "Alicia",
                    Surname = "Rodriguez",
                    BirthDate = new DateOnly(1995, 12, 09),
                    Email = "aliciarodriguez@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Rodriguez02!!"),
                    Dni = 1234567,
                    PhoneNumber = 12345678,
                    Gender = "F",
                    Sex = "F",
                    Role = "Patient"
                });
                await db.SaveChangesAsync();
            }
        }
    }
}
