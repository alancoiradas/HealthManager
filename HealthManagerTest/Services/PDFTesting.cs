using HealthManager.Models.DTO;
using HealthManager.Services.PDF.AppointmentReceipt;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthManagerTest.Services
{
    public class PDFTesting
    {
        IAppointmentReceipt mockInterface  = Substitute.For<IAppointmentReceipt>();

        [Fact]
        public void PDFReturnsSuccessfully()
        {
            AppointmentDataPDFDTO appointmentData = new AppointmentDataPDFDTO
            {
                AppointmentDate = new DateOnly(2020, 04, 09),
                AppointmentHour = new TimeOnly(13, 45),
                Specialty = "Oftalmology",
                DoctorName = "Alfonso Benitez",
                PatientName = "Anastasia Romanov"
            };

            var arrBytes = mockInterface.CreateAppointmentReceipt(appointmentData);

            Assert.IsType<byte[]>(arrBytes);
        }
    }
}
