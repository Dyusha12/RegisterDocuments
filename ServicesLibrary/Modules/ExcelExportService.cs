using System.Collections.Generic;
using ClosedXML.Excel;

namespace ServicesLibrary.Modules
{
    public class ExcelExportService
    {
        // Экспортирует список данных в Excel-файл (.xlsx)
        public bool ExportChartDataToExcel(List<ChartItem> data, string title, string filePath)
        {
            try
            {
                // Создание новой Excel-книг
                using var workbook = new XLWorkbook();

                // Добавление листа
                var worksheet = workbook.Worksheets.Add("Statistics");

                // Запись заголовка в первую строку
                worksheet.Cell(1, 1).Value = title;

                // Запись заголовков таблицы
                worksheet.Cell(3, 1).Value = "Категория";
                worksheet.Cell(3, 2).Value = "Количество";

                int row = 4;

                // Проходим по всем элементам и записываем их в Excel
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Name;   // название категории
                    worksheet.Cell(row, 2).Value = item.Count;  // значение/количество
                    row++;
                }

                // Автоширина колонок
                worksheet.Columns().AdjustToContents();

                // Сохранение файла по указанному пути
                workbook.SaveAs(filePath);

                return true; // экспорт выполнен успешно
            }
            catch
            {
                // Произошла ошибка
                return false;
            }
        }
    }
}