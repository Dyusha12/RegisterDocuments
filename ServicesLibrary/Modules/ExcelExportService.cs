using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ServicesLibrary.Modules
{
    public class ExcelExportService
    {
        public bool ExportChartDataToExcel(List<ChartItem> data, string title, string filePath)
        {
            Application excelApp = null;
            Workbook workbook = null;

            try
            {
                excelApp = new Application();
                workbook = excelApp.Workbooks.Add();  // Добавление новой книги
                Worksheet worksheet = workbook.ActiveSheet; // Выбор активного листа

                // Записываем заголовок
                worksheet.Cells[1, 1] = title;

                // Заголовки таблицы
                worksheet.Cells[3, 1] = "Категория";
                worksheet.Cells[3, 2] = "Количество";

                int row = 4;

                // Заполнение строки данными
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1] = item.Name;
                    worksheet.Cells[row, 2] = item.Count;
                    row++;
                }

                // Автоматическая подгонка ширины колонок
                worksheet.Columns.AutoFit();

                try
                {
                    workbook.SaveAs(filePath);
                    return true; // Успешно сохранено
                }
                catch (COMException)
                {
                    // Пользователь нажал "Нет" или файл занят
                    workbook.Close(false); // Изменения не сохраняются
                    return false;
                }
            }
            finally
            {
                // Освобождение COM-объектов, чтобы Excel не оставался в памяти
                if (workbook != null) Marshal.ReleaseComObject(workbook);
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }
            }
        }
    }
}