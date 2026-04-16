using HealthManager.Models;
using HealthManager.Models.DTO;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace HealthManager.Services.PDF.AppointmentReceipt
{
    public class AppointmentReceiptService : IAppointmentReceipt
    {
        public byte[] CreateAppointmentReceipt(AppointmentDataPDFDTO appointment)
        {
            string instructions = "Hello, we attatch the information of your appointment below, please make it sure that the day and hour so as the professional of the appointment are correct.\r\nIf for some reason you can't assist to the appointment, you can cancel it in the platfrom before the hour of the same (it has to be 4 hours in advance)\r\nWith that being said, we display the information below. Have a nice day!!";
            MemoryStream streamPdf = new MemoryStream();
            Document.Create(Document =>
            {
                Document.Page(page =>
                {
                    page.MarginHorizontal(30);
                    page.MarginVertical(20);

                    page.Header().Row(row =>
                    {
                        row.ConstantItem(200).Height(80).Column(col =>
                        {
                            col.Item().AlignCenter().Text("HealthManager").Bold().FontSize(25);
                            col.Item().AlignCenter().Text("Appointment");
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("HealthManagerINC.");
                            col.Item().AlignCenter().Text(DateTime.Now.ToLocalTime().ToString());
                            col.Item().AlignCenter().Text("Street 123, Buenos Aires, Argentina");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Text(instructions);
                        col.Spacing(30);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.LightGreen.Medium).Padding(3).Text("Specialty").FontSize(15);
                                header.Cell().Background(Colors.LightGreen.Medium).Padding(3).Text("Doctor").FontSize(15);
                                header.Cell().Background(Colors.LightGreen.Medium).Padding(3).Text("Date").FontSize(15);
                                header.Cell().Background(Colors.LightGreen.Medium).Padding(3).Text("Hour").FontSize(15);
                            });
                            table.Cell().Border(0.5f).Padding(3).Text(appointment.Specialty).FontSize(15);
                            table.Cell().Border(0.5f).Padding(3).Text(appointment.DoctorName).FontSize(15);
                            table.Cell().Border(0.5f).Padding(3).Text(appointment.AppointmentDate.ToString()).FontSize(15);
                            table.Cell().Border(0.5f).Padding(3).Text(appointment.AppointmentHour.ToString()).FontSize(15);
                        });
                    });

                    page.Footer().Column(footerCol =>
                    {
                        footerCol.Item().AlignLeft().Text("If you have any doubts, you can contact us trough the email provided below.");
                        footerCol.Spacing(5);
                        footerCol.Item().AlignLeft().Text("Email: email@email.com");
                    });
                });
            }).GeneratePdf(streamPdf);
            return streamPdf.ToArray();
        }
    }
}
