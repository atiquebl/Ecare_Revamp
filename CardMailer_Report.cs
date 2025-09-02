using DevExpress.XtraReports.UI;
using System.Data;
namespace ECare_Revamp.Models
{
    public class CardMailer_Report : XtraReport
    {
        public CardMailer_Report(DataTable table)
        {
            DataSource = table;

            // Create a detail band
            var detail = new DetailBand();
            Bands.Add(detail);

            // Example: Display one column (replace with your fields)
            var label = new XRLabel
            {
                WidthF = 400,
                ExpressionBindings = {
                new ExpressionBinding("BeforePrint", "Text", "[CARD_NUMBER]") // Use actual column name
            }
            };
            detail.Controls.Add(label);
        }
    }
}
