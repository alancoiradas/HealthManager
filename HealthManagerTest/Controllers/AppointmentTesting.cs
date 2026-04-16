using HealthManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthManagerUnitTest.Controllers
{
    public class AppointmentTesting
    {
        IEnumerable<Appointment> appointmentList =
        [
            new Appointment
            {
                AppointmentId = Guid.Parse("c2f1bcb1-2e3e-4c5d-9b27-6a5a3b819f2b"),
                AppointmentDate = new DateOnly(2025, 10, 28),
                AppointmentHour = new TimeOnly(14, 30),
                PatientId = 1,
                DoctorId = 1,
                Attended = null,
                Status = "Reserved"
            },
            new Appointment
            {
                AppointmentId = Guid.Parse("b9a8f5b4-60b0-42d5-bc12-317e17f9a8d4"),
                AppointmentDate = new DateOnly(2025, 10, 15),
                AppointmentHour = new TimeOnly(11, 30),
                PatientId = 2,
                DoctorId = 3,
                Attended = false,
                Status = "Reserved"
            },
            new Appointment
            {
                AppointmentId = Guid.Parse("45de57a1-5cb4-40b1-a4ab-35b2d7ad2fa9"),
                AppointmentDate = new DateOnly(2025, 10, 30),
                AppointmentHour = new TimeOnly(09, 15),
                PatientId = null,
                DoctorId = 5,
                Attended = null,
                Status = "Available"
            },
            new Appointment
            {
                AppointmentId = Guid.Parse("9f2e67cb-4b2b-4ff6-bc89-1a3f04c7d6ee"),
                AppointmentDate = new DateOnly(2025, 10, 19),
                AppointmentHour = new TimeOnly(14, 30),
                PatientId = 1,
                DoctorId = 1,
                Attended = true,
                Status = "Reserved"
            },
            new Appointment
            {
                AppointmentId = Guid.Parse("7ab6532a-b39c-4d5f-b9c3-29c749b2d1a1"),
                AppointmentDate = new DateOnly(2025, 10, 20),
                AppointmentHour = new TimeOnly(16, 30),
                PatientId = null,
                DoctorId = 1,
                Attended = null,
                Status = "Available"
            },
            new Appointment
            {
                AppointmentId = Guid.Parse("f34d820b-57d4-42f1-9af9-c43a34a7dcb7"),
                AppointmentDate = new DateOnly(2025, 10, 20),
                AppointmentHour = new TimeOnly(10, 30),
                PatientId = null,
                DoctorId = 1,
                Attended = null,
                Status = "Available"
            },
            new Appointment
            {
                AppointmentId = Guid.Parse("e8a3b72e-9c77-43a9-8bda-09a5b824d4e3"),
                AppointmentDate = new DateOnly(2025, 10, 10),
                AppointmentHour = new TimeOnly(10, 45),
                PatientId = null,
                DoctorId = 8,
                Attended = null,
                Status = "Available"
            },
            new Appointment
            {
                AppointmentId = Guid.Parse("1b9f75c8-67db-45a9-bf13-4a8c372a7c6e"),
                AppointmentDate = new DateOnly(2025, 10, 25),
                AppointmentHour = new TimeOnly(15, 30),
                PatientId = null,
                DoctorId = 5,
                Attended = null,
                Status = "Available"
            },
        ];

        IEnumerable<Doctor> doctorList = 
        [
            new Doctor
            {
                DoctorId = 1,
                Specialty = 1,
            },
            new Doctor
            {
                DoctorId = 2,
                Specialty = 3,
            },
            new Doctor
            {
                DoctorId = 3,
                Specialty = 7,
            },
            new Doctor
            {
                DoctorId = 5,
                Specialty = 3,
            }
        ];

        [Fact]
        public void GetAvailableAppointmentsAfterASpecificDate()
        {

            //Arrange 

            DateOnly date = new DateOnly(2025, 10, 20);
            TimeOnly hour = new TimeOnly(14, 35);
            DateTime testDate = date.ToDateTime(hour);

            //Act

            IEnumerable<Appointment> filteredList = appointmentList.Where(x => x.Status == "Available" && (x.AppointmentDate.CompareTo(date) > 0 ||
                x.AppointmentDate.ToDateTime(x.AppointmentHour) > testDate))
                .OrderBy(x => x.AppointmentDate)
                .ThenBy(x => x.AppointmentHour);


            //Assert

            IEnumerable<Appointment> expectedList =
                [
                    new Appointment
                    {
                        AppointmentId = Guid.Parse("7ab6532a-b39c-4d5f-b9c3-29c749b2d1a1"),
                        AppointmentDate = new DateOnly(2025, 10, 20),
                        AppointmentHour = new TimeOnly(16, 30),
                        PatientId = null,
                        DoctorId = 1,
                        Attended = null,
                        Status = "Available"
                    },
                    new Appointment
                    {
                        AppointmentId = Guid.Parse("45de57a1-5cb4-40b1-a4ab-35b2d7ad2fa9"),
                        AppointmentDate = new DateOnly(2025, 10, 30),
                        AppointmentHour = new TimeOnly(09, 15),
                        PatientId = null,
                        DoctorId = 5,
                        Attended = null,
                        Status = "Available"
                    },
                    new Appointment
                    {
                        AppointmentId = Guid.Parse("1b9f75c8-67db-45a9-bf13-4a8c372a7c6e"),
                        AppointmentDate = new DateOnly(2025, 10, 25),
                        AppointmentHour = new TimeOnly(15, 30),
                        PatientId = null,
                        DoctorId = 5,
                        Attended = null,
                        Status = "Available"
                    },
                ];

            IEnumerable<Appointment> orderedExpextedList = expectedList.OrderBy(x => x.AppointmentDate).ThenBy(x => x.AppointmentHour);

            Assert.Equal(
                orderedExpextedList.Select(x => x.AppointmentId),
                filteredList.Select(x => x.AppointmentId)
                );
        }

        [Fact]
        public void GetReservedAppointments()
        {
            //Arrange

            //Act
            IEnumerable<Appointment> reservedAppointments = appointmentList.Where(x => x.Status == "Reserved");
            //Assert

            IEnumerable<Appointment> expectedList =
            [
               new Appointment
                {
                    AppointmentId = Guid.Parse("c2f1bcb1-2e3e-4c5d-9b27-6a5a3b819f2b"),
                    AppointmentDate = new DateOnly(2025, 10, 28),
                    AppointmentHour = new TimeOnly(14, 30),
                    PatientId = 1,
                    DoctorId = 1,
                    Attended = null,
                    Status = "Reserved"
                },

                new Appointment
                {
                    AppointmentId = Guid.Parse("b9a8f5b4-60b0-42d5-bc12-317e17f9a8d4"),
                    AppointmentDate = new DateOnly(2025, 10, 15),
                    AppointmentHour = new TimeOnly(11, 30),
                    PatientId = 2,
                    DoctorId = 3,
                    Attended = false,
                    Status = "Reserved"
                },

                new Appointment
                {
                    AppointmentId = Guid.Parse("9f2e67cb-4b2b-4ff6-bc89-1a3f04c7d6ee"),
                    AppointmentDate = new DateOnly(2025, 10, 19),
                    AppointmentHour = new TimeOnly(14, 30),
                    PatientId = 1,
                    DoctorId = 1,
                    Attended = true,
                    Status = "Reserved"
                },
            ];

            Assert.Equal(
                expectedList.Select(x => x.AppointmentId), 
                reservedAppointments.Select(x => x.AppointmentId));
        }

        [Fact]
        public void GetPatientReservedAppointments()
        {
            //Arrange

            int patientId = 1;
            DateOnly consultingDate = new DateOnly(2025, 10, 26);
            TimeOnly consultingTime = new TimeOnly(10, 00);
            DateTime consultingDateTime = consultingDate.ToDateTime(consultingTime);

            //Act

            IEnumerable<Appointment> patientAppointments = appointmentList.Where(x =>
            x.PatientId == patientId && 
            ( x.AppointmentDate.CompareTo(consultingDate) >0 || x.AppointmentDate.ToDateTime(x.AppointmentHour) > consultingDateTime));

            //Assert

            IEnumerable<Appointment> expectedList =
            [
                new Appointment
                {
                    AppointmentId = Guid.Parse("c2f1bcb1-2e3e-4c5d-9b27-6a5a3b819f2b"),
                    AppointmentDate = new DateOnly(2025, 10, 28),
                    AppointmentHour = new TimeOnly(14, 30),
                    PatientId = 1,
                    DoctorId = 1,
                    Attended = null,
                    Status = "Reserved"
                },
            ];

            Assert.Equal
                (
                    expectedList.Select(x => x.AppointmentId),
                    patientAppointments.Select(x => x.AppointmentId)
                );
        }

        [Fact]
        public void GetDoctorsFromASpecialty()
        {
            //Arrange
            int specialtyId = 3;

            //Act
            IEnumerable<Doctor> doctorFromSpecialty = doctorList.Where(x => x.Specialty == specialtyId);

            //Assert
            IEnumerable<Doctor> doctorsExpected = 
            [
                new Doctor
                {
                    DoctorId = 2,
                    Specialty = 3,
                },
                new Doctor
                {
                    DoctorId = 5,
                    Specialty = 3,
                }
            ];

            Assert.Equal(doctorsExpected.Select(x => x.DoctorId), doctorFromSpecialty.Select(x => x.DoctorId));
        }

        [Fact]
        public void GetAvailableAppointmentsFromADoctor()
        {
            //Arrange

            int doctorId = 1;

            //Act

            IEnumerable<Appointment> doctorAppointments = appointmentList.Where(x => x.DoctorId == doctorId && x.Status == "Available");

            //Assert

            IEnumerable<Appointment> expectedAppointments = 
            [
                new Appointment
                {
                    AppointmentId = Guid.Parse("7ab6532a-b39c-4d5f-b9c3-29c749b2d1a1"),
                    AppointmentDate = new DateOnly(2025, 10, 20),
                    AppointmentHour = new TimeOnly(16, 30),
                    PatientId = null,
                    DoctorId = 1,
                    Attended = null,
                    Status = "Available"
                },
                new Appointment
                {
                    AppointmentId = Guid.Parse("f34d820b-57d4-42f1-9af9-c43a34a7dcb7"),
                    AppointmentDate = new DateOnly(2025, 10, 20),
                    AppointmentHour = new TimeOnly(10, 30),
                    PatientId = null,
                    DoctorId = 1,
                    Attended = null,
                    Status = "Available"
                },
            ];

            Assert.Equal(expectedAppointments.Select(x => x.AppointmentId), doctorAppointments.Select(x => x.AppointmentId));
        }

        [Fact]
        public void GetAvailableAppointmentsFromADate()
        {
            //Arrange

            DateOnly consultingDate = new DateOnly(2025, 10, 20);
            TimeOnly consultingHour = new TimeOnly(11, 30);
            DateTime consultingDateTime = consultingDate.ToDateTime(consultingHour);

            //Act

            IEnumerable<Appointment> availableAppointmentsOnADate = appointmentList.Where(x => x.AppointmentDate == consultingDate
                && x.AppointmentDate.ToDateTime(x.AppointmentHour) > consultingDateTime);

            //Assert

            IEnumerable<Appointment> expectedList = 
            [
                new Appointment
                {
                    AppointmentId = Guid.Parse("7ab6532a-b39c-4d5f-b9c3-29c749b2d1a1"),
                    AppointmentDate = new DateOnly(2025, 10, 20),
                    AppointmentHour = new TimeOnly(16, 30),
                    PatientId = null,
                    DoctorId = 1,
                    Attended = null,
                    Status = "Available"
                },
            ];

            Assert.Equal(expectedList.Select(x => x.AppointmentId), availableAppointmentsOnADate.Select(x => x.AppointmentId));
        }
    }
}
