using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
//using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Globalization;
using System.Text.Json;
using Utils;

namespace AspNet.Controllers
{
    public class MeasuresController : Controller
    {
        private DateTime now = DateTime.Now.AddSeconds(-8);
        private IEnumerable<Models.MeasureViewModel> measures;
        private Speditor speditor = JsonSerializer.Deserialize<Speditor>(System.IO.File.ReadAllText("speditor.json"));

        [HttpPost]
        public IActionResult Index(string product, DateTime from, DateTime to, int? kanbel, string? plrem )
        {
            ViewBag.Product = product;
            ViewBag.From = from.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.To = to.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.Kanbel = kanbel;
            ViewBag.Plrem = plrem;

            this.measures = MeasuresServices.GetMeasures(product, from, to, kanbel, plrem);
            var model = new
            {
                Measures = measures,
                Products = MeasuresServices.GetProducts()
            };

            return this.View(model);
        }

        public IActionResult DownloadSearch(string? product, DateTime from, DateTime to, int? kanbel, string? plrem)
        {
            string dateFormat = "d.M.yy";
            var measures = MeasuresServices.GetMeasures(product, from, to, kanbel, plrem);

            var plannedTrucksToday = MeasuresServices.GetTrucksPlannedMonthly()?.Where(w => w.Key == now.ToString("d.M.yyyy")).FirstOrDefault().Value;

            string header1 = speditor.Supplier;
            string header2 = "  Справка за проведените измервания";
            string header3 = "за периода:" + from + " - " + to;
            if (product != null)
                header3 +=  ", с условие: Вид товар: " + product;
            if (plrem != null)
                header3 += ", № ремарке съдържа \"" + plrem + "\"";

            string[] tableHeader = "№;Кант.бел.;От дата;Рег.№ влекач;Рег.№ рем.;Бруто;Час;Тара;Час;Нето;Водач;Фирма;Дестинация".Split(";", StringSplitOptions.RemoveEmptyEntries);
            string fileName = "Measures.xlsx";
            string wsName = measures.LastOrDefault().Brutotm.ToString(dateFormat);

            using MemoryStream stream = new MemoryStream();

            // // // If you use EPPlus in a noncommercial context according to the Polyform Noncommercial license:  
            // // ExcelPackage.LicenseContext = LicenseContext.NonCommercial; //but we use version 4.5.3.3
            // using ExcelPackage package = new ExcelPackage(stream);
            // ExcelWorksheet ws = package.Workbook.Worksheets.Add(wsName);

            // ws.Cells[1, 3].Value = header1;
            // ws.Cells[2, 3].Value = header2;
            // ws.Cells[3, 2].Value = header3;

            // for (int i = 0; i < tableHeader.Length; i++)
            // {
            //     ws.Cells[5, i + 1].Value = tableHeader[i];
            // }

            // ws.Cells["A5"].LoadFromCollection(measures, true);

            // package.Save();

            // stream.Position = 0;
            // return File(stream, "application/octet-stream", fileName);


            //using NPOI
            IWorkbook workbook = new XSSFWorkbook();
            var style = (XSSFCellStyle)workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Left;
            style.VerticalAlignment = VerticalAlignment.Top;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;

            ISheet sheet1 = workbook.CreateSheet(wsName);

            var rowIndex = 0;
            IRow row = sheet1.CreateRow(rowIndex);

            // ChatGPT:
            // var properties = typeof(Models.MeasureViewModel).GetProperties();
            // var columnIndex = 0;
            // foreach (var property in properties)
            // {
            //     var cell = row.CreateCell(columnIndex);
            //     cell.SetCellValue(property.Name);
            //     columnIndex++;
            // }
            // rowIndex++;
            // foreach (var measure in measures)
            // {
            //     row = ws.CreateRow(rowIndex);
            //     columnIndex = 0;
            //     foreach (var property in properties)
            //     {
            //         var cell = row.CreateCell(columnIndex);
            //         var value = property.GetValue(measure);
            //         if (value != null)
            //         {
            //             cell.SetCellValue(value.ToString());
            //         }
            //         columnIndex++;
            //     }
            //     rowIndex++;
            // }

            row.CreateCell(2).SetCellValue(header1);
            rowIndex++;
            row = sheet1.CreateRow(rowIndex);
            row.CreateCell(2).SetCellValue(header2);
            rowIndex++;
            row = sheet1.CreateRow(rowIndex);
            row.CreateCell(1).SetCellValue(header3);
            rowIndex++;
            row = sheet1.CreateRow(++rowIndex);
            for (int col = 0; col < tableHeader.Length; col++)
            {
                row.CreateCell(col).SetCellValue(tableHeader[col]);
                row.GetCell(col).CellStyle = style;
            }

            rowIndex++;
            int counter = 1;
            foreach (var measure in measures)
            {
                var plremCyr = TextFile.ReplaceCyrillic(measure.Plrem);
                row = sheet1.CreateRow(rowIndex);
                row.CreateCell(0).SetCellValue(counter++);
                row.CreateCell(1).SetCellValue((int)measure.Kanbel);
                row.CreateCell(2).SetCellValue(measure.Brutotm.ToString(dateFormat));
                row.CreateCell(3).SetCellValue(measure.TruckPlvl);
                row.CreateCell(4).SetCellValue(measure.Plrem);
                row.CreateCell(5).SetCellValue((int)measure.Bruto);
                row.CreateCell(6).SetCellValue(measure.Brutotm.ToString("HH:mm"));
                row.CreateCell(7).SetCellValue((int)measure.Tara);
                row.CreateCell(8).SetCellValue((measure.Taratm != null ? ((DateTime)measure.Taratm).ToString("HH:mm") : string.Empty));
                row.CreateCell(9).SetCellValue((int)measure.Bruto - (int)measure.Tara);
                row.CreateCell(10).SetCellValue(measure.Name + " " + measure.Sname + " " + measure.Fam);
                row.CreateCell(11).SetCellValue(measure.CompanyName);
                if (plannedTrucksToday != null)
                    row.CreateCell(12).SetCellValue(plannedTrucksToday.Keys.Contains(plremCyr) ? plannedTrucksToday[plremCyr] : string.Empty);

                var cells = row.Cells;
                foreach (var cell in cells)
                    cell.CellStyle = style;

                rowIndex++;
            }
            row = sheet1.CreateRow(rowIndex);
            row.CreateCell(8).SetCellValue("ВСИЧКО:");
            row.CreateCell(9).SetCellValue((int)measures.Sum(m => m.Neto));

            var warnings = new Dictionary<string, List<string>>();
            var today = now.ToString("d.M.yyyy");
            if (warnings.ContainsKey(today))
            {
                List<string> warningsToday = warnings[today];
                foreach (var warning in warningsToday)
                {
                    rowIndex++;
                    row = sheet1.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(warning);
                }
            }

            sheet1.SetColumnWidth(0, 4 * 256);
            sheet1.SetColumnWidth(1, 9 * 256);
            sheet1.SetColumnWidth(3, 13 * 256);
            sheet1.SetColumnWidth(4, 12 * 256);
            sheet1.SetColumnWidth(6, 6 * 256);
            sheet1.SetColumnWidth(8, 8 * 256);
            sheet1.SetColumnWidth(10, 30 * 256);
            sheet1.SetColumnWidth(11, 12 * 256);

            workbook.Write(stream);

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public IActionResult DownloadMonthShifts()
        {            
            var dataTable = new DataTable();
            dataTable.Columns.Add("Дата", typeof(string));
            dataTable.Columns.Add("Смяна", typeof(byte));
            dataTable.Columns.Add("Нето [тон]", typeof(decimal));
            dataTable.Columns.Add("Пепел [%]", typeof(string));
            dataTable.Columns.Add("Брой", typeof(int));

            var from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (now.Day == 1 && now.Hour < 11)
                    from = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1);

            var measures = MeasuresServices.GetMeasures(speditor.Product, from, now);           
            int days = DateTime.DaysInMonth(from.Year, from.Month);
                        
            for (int day = 1; day <= days; day++)
            {
                var dailyMeasures = measures.Where(m =>
                    m.Brutotm.Day == day && m.Brutotm.TimeOfDay >= new TimeSpan(8, 0, 0) && m.Brutotm.TimeOfDay < new TimeSpan(20, 0, 0));

                var nightMeasures = measures.Where(m =>
                    (m.Brutotm.Day == day && m.Brutotm.TimeOfDay >= new TimeSpan(20, 0, 0)) ||
                    (m.Brutotm.Day == day + 1 && m.Brutotm.TimeOfDay < new TimeSpan(8, 0, 0)));

                for (int shift = 1; shift <= 2; shift++)
                {
                    var shiftMeasures = dailyMeasures;
                    if (shift == 2)
                        shiftMeasures = nightMeasures;

                    if (shiftMeasures.Count() == 0)
                        dataTable.Rows.Add(day, shift);

                    else
                        dataTable.Rows.Add(day, shift, shiftMeasures.Sum(s => s.Neto) / 1000.0, "N/A", shiftMeasures.Count());
                }
            }

            var stream = new MemoryStream();

            using ExcelPackage package = new ExcelPackage(stream);

            while (package.Workbook.Worksheets.Count > 0)
            {
                package.Workbook.Worksheets.Delete(0);
            }
            ExcelWorksheet ws = package.Workbook.Worksheets.Add("м." + now.Month.ToString());
            ws.DefaultRowHeight = 13.5;
            ws.Cells["A1:D67"].Style.Font.Size = 10;
            ws.Cells["A1:D67"].Style.Font.Name = "Arial";

            ws.Column(1).Width = 11;
            ws.Column(2).Width = 7;
            ws.Column(3).Width = 11;
            ws.Column(4).Width = 11;
            ws.Column(5).Width = 9;

            ws.Row(5).Style.Font.Bold = true;
            ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[3, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ws.Cells[3, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells[1, 2].Value = "Рекапитулация по дни";
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A3"].Style.Font.Bold = true;
            ws.Cells[3, 1].Value = "Период: ";
            ws.Cells[3, 2].Value = " " + from;
            ws.Cells[3, 4].Value = " - " + now;

            //add all the content from the DataTable, starting at given cell
            ws.Cells["A5"].LoadFromDataTable(dataTable, true);

            package.Save();

            stream.Position = 0;
            string excelName = "MonthShifts.xlsx";
            return File(stream, "application/octet-stream", excelName);
        }

        public IActionResult DownloadMonthOpis()
        {
            var plannedTrucks = MeasuresServices.GetTrucksPlannedMonthly();
            string[] tableHeader = "N;Кант.бел.;От дата;Рег.№ рем.;Бруто;Бруто час;Тара;Нето kg;Кач-во р3;Ед.цена €;К-во ден;Анализ;Направление;Invoice;Баржа".Split(";", StringSplitOptions.RemoveEmptyEntries);
            string fileName = "Opis-Export.xlsx";

            IWorkbook workbook = new XSSFWorkbook();
            using MemoryStream stream = new MemoryStream();

            ISheet sheet1 = workbook.CreateSheet("м." + now.Month.ToString());

            var style = (XSSFCellStyle)workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Left;
            style.VerticalAlignment = VerticalAlignment.Top;
            // style.BorderBottom = BorderStyle.Thin;
            // style.BorderTop = BorderStyle.Thin;
            // style.BorderLeft = BorderStyle.Thin;
            // style.BorderRight = BorderStyle.Thin;

            var rowIndex = 0;
            IRow row = sheet1.CreateRow(rowIndex);
            for (int col = 0; col <= tableHeader.Length - 1; col++)
            {
                row.CreateCell(col).SetCellValue(tableHeader[col]);
                row.GetCell(col).CellStyle = style;
            }

            var from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var measures = MeasuresServices.GetMeasures(speditor.Product, from, now);
            if (now.Day == 1 && now.Hour < 11)
                measures = MeasuresServices.GetMeasures(speditor.Product, from.AddMonths(-1), DateTime.Today.AddDays(-DateTime.Today.Day));

            rowIndex++;
            int counter = 1;
            int currentDay = measures.LastOrDefault().Brutotm.Day;
            foreach (var measure in measures)
            {
                string currentDate = currentDay + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
                if (measure.Brutotm.Day != currentDay)
                {
                    counter = 1;
                    currentDay = measure.Brutotm.Day;
                }
                var plrem = TextFile.ReplaceCyrillic(measure.Plrem);
                row = sheet1.CreateRow(rowIndex);
                row.CreateCell(0).SetCellValue(counter++);
                row.CreateCell(1).SetCellValue((int)measure.Kanbel);
                row.CreateCell(2).SetCellValue(measure.Brutotm.ToString("dd/MM/yy", CultureInfo.InvariantCulture));
                row.CreateCell(3).SetCellValue(plrem);
                row.CreateCell(4).SetCellValue((int)measure.Bruto);
                row.CreateCell(5).SetCellValue(measure.Brutotm.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                row.CreateCell(6).SetCellValue((int)measure.Tara);
                row.CreateCell(7).SetCellValue((int)measure.Bruto - (int)measure.Tara == (int)measure.Neto ? (int)measure.Neto : throw new Exception("Bruto - Tara != Neto"));
                if (plannedTrucks != null && plannedTrucks.ContainsKey(currentDate))
                {
                    var plannedTrucksToday = plannedTrucks[currentDate];
                    row.CreateCell(12).SetCellValue(plannedTrucks[currentDate].Keys.Contains(plrem) ? plannedTrucks[currentDate][plrem] : string.Empty);
                }

                var cells = row.Cells;
                foreach (var cell in cells)
                    cell.CellStyle = style;
                rowIndex++;
            }

            workbook.Write(stream);

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}