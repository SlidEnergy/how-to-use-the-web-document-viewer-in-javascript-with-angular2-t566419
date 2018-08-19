using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.WebDocumentViewer;
using System.Data;
using System.IO;

namespace ServerSide
{
	public class CustomWebDocumentViewerReportResolver : IWebDocumentViewerReportResolver
	{
		public CustomWebDocumentViewerReportResolver() { }

		private XtraReport GetReport()
		{
			DataTable dataSource = new DataTable("DataSourceName");
			dataSource.Columns.Add(new DataColumn("ColumnNameTest", typeof(int)));

			var report = new XtraReport();

			// Создаем элементы шаблона отчета
			var headerBand = new ReportHeaderBand();
			report.Bands.Add(headerBand);

			var detailBand = new DetailBand
			{
				Height = 23
			};
			report.Bands.Add(detailBand);

			var label = new XRLabel
			{
				Text = "Заголовок отчета"
			};
			headerBand.Controls.Add(label);

			var cell = new XRTableCell
			{
				Borders = DevExpress.XtraPrinting.BorderSide.All
			};
			cell.DataBindings.Add("Text", null, "ColumnNameTest", "{0}");
			detailBand.Controls.Add(cell);

			// Привязка шаблона к отчетной форме
			report.DataSource = dataSource;
			report.DataMember = "DataSourceName";

			// Заполняем источник данных
			for (int i = 0; i < 10; i++)
			{
				DataRow row = dataSource.NewRow();
				row["ColumnNameTest"] = i * 10;
				dataSource.Rows.Add(row);
			}

			// Создание отчета (связываем шаблон с данными)
			report.CreateDocument();

			return report;
		}

		private XtraReport ImitateApplicationServerInterop(XtraReport report)
		{
			// Application Server-side
			byte[] buffer;

			using (MemoryStream stream = new MemoryStream())
			{
				report.PrintingSystem.SaveDocument(stream);
				buffer = stream.ToArray();
			}


			// Transfer through network...


			// IIS Server-side
			XtraReport report2 = new XtraReport();

			using (MemoryStream stream2 = new MemoryStream(buffer))
			{
				report2.PrintingSystem.LoadDocument(stream2);
			}

			return report2;
		}

		public XtraReport Resolve(string reportTypeName)
		{
			var report = GetReport();
			report = ImitateApplicationServerInterop(report);
			return report;
		}
	}
}