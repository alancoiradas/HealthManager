using HealthManager.Models.DTO;
using HealthManager.Services.Mail;
using HealthManager.Services.PDF.AppointmentReceipt;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthManagerTest.Services
{
    public class MailTesting
    {


        [Fact]
        public async Task TestMail()
        {
            IAppointmentReceipt appointmentMock = Substitute.For<IAppointmentReceipt>();
            IMailService mailMock = Substitute.For<IMailService>();

            AppointmentDataPDFDTO appointmentData = new AppointmentDataPDFDTO
            {
                AppointmentDate = new DateOnly(2020, 04, 09),
                AppointmentHour = new TimeOnly(13, 45),
                Specialty = "Oftalmology",
                DoctorName = "Alfonso Benitez",
                PatientName = "Anastasia Romanov"
            };

            MailDTO mailProfile = new MailDTO
            {
                DestinataryMail = "pepito@gmail.com",
                DestinataryName = "Pepe",
                MailSubject = "Appointment Reservation",
                MailTitle = "Title",
            };

            var arrBytes = appointmentMock.CreateAppointmentReceipt(appointmentData);

            MethodResponse response = mailMock.SendAppointmentConfirmationMail(mailProfile, arrBytes);

            Assert.True(response.Success);
        }
    }
}
