using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;

namespace ServerSide {
    public class MyReportStorage : ReportStorageWebExtension {
        public Dictionary<string, XtraReport> Reports = new Dictionary<string, XtraReport>();

        public MyReportStorage() {
            Reports.Add("Products", new XtraReport1());
            Reports.Add("Categories", new XtraReport2());
        }

        public override bool CanSetData(string url) {
            return true;
        }

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
			//// Сохраняем отчет в массив байт
			//var memoryStream = new System.IO.MemoryStream();
			//report.PrintingSystem.SaveDocument(memoryStream);
			//var buffer = memoryStream.ToArray();
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

		public override byte[] GetData(string url) {
			//var report = Reports[url];
			var report = GetReport();

			report = ImitateApplicationServerInterop(report);

			using (MemoryStream stream = new MemoryStream()) {
				report.SaveLayoutToXml(stream);
				//report.PrintingSystem.SaveDocument(stream);
				return stream.ToArray();
            }
        }

        public override Dictionary<string, string> GetUrls() {
            return Reports.ToDictionary(x => x.Key, y => y.Key);
        }

        public override void SetData(XtraReport report, string url) {
            if(Reports.ContainsKey(url)) {
                Reports[url] = report;
            }
            else {
                Reports.Add(url, report);
            }
        }

        public override string SetNewData(XtraReport report, string defaultUrl) {
            SetData(report, defaultUrl);
            return defaultUrl;
        }

        public override bool IsValidUrl(string url) {
            return true;
        }
    }
}