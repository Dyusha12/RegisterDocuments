using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using ServicesLibrary.Interfaces;
using ServicesLibrary.Models;

namespace ServicesLibrary.Modules
{
    // Типы диаграмм, которые можно строить
    public enum ChartType { Pie, Column }

    // Опции группировки для статистики
    public enum GroupByOption { Type, Status }

    // Класс для хранения данных диаграммы
    public class ChartItem
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    // Сервис для построения статистики по документам
    public class StatisticsService
    {
        private readonly IDocumentProvider _documentProvider; // Источник данных документов
        private readonly IDocumentTypeProvider _documentTypeProvider; // Источник данных типов документов

        public StatisticsService(IDocumentProvider documentProvider, IDocumentTypeProvider documentTypeProvider)
        {
            _documentProvider = documentProvider;
            _documentTypeProvider = documentTypeProvider;
        }

        // Строит диаграмму по выбранной опции группировки и типу диаграммы
        public UIElement BuildChart(GroupByOption groupByOption, string filterCode = "", string filterName = "", ChartType chartType = ChartType.Pie)
        {
            var documents = _documentProvider.GetDocuments(); // Получение всех документов
            var groupedData = new List<ChartItem>();

            if (!documents.Any())
                return EmptyChart("Нет данных для подсчета статистики"); // Eсли нет документов, показываем сообщение

            switch (groupByOption)
            {
                case GroupByOption.Type:
                    // Фильтрация по статусу, если указан
                    if (!string.IsNullOrEmpty(filterCode))
                        documents = documents.Where(d => d.StatusName == filterCode).ToList();

                    // Группировка по типу документа
                    groupedData = documents
                        .GroupBy(d => d.CodeTypeDocument)
                        .Select(g => new ChartItem
                        {
                            // Получение названия типа по коду
                            Name = _documentTypeProvider.GetDocumentTypes()
                                    .First(dt => dt.CodeTypeDocument == g.Key).NameType,
                            Count = g.Count() // Количество документов данного типа
                        })
                        .ToList();
                    break;

                case GroupByOption.Status:
                    // Фильтрация по типу документа, если указан
                    if (!string.IsNullOrEmpty(filterCode))
                        documents = documents.Where(d => d.CodeTypeDocument == filterCode).ToList(); // фильтруем по типу, если выбран фильтр по типу

                    // Группировка по статусу
                    groupedData = documents
                        .GroupBy(d => d.StatusName)
                        .Select(g => new ChartItem
                        {
                            Name = g.Key,
                            Count = g.Count()
                        })
                        .ToList();
                    break;
            }

            if (!groupedData.Any()) // Если после фильтрации нет данных
                return EmptyChart("Нет данных для подсчета статистики");

            // Возвращение нужного типа диаграммы
            return chartType == ChartType.Pie
                ? BuildPieChart(groupedData, filterName)
                : BuildColumnChart(groupedData, filterName);
        }

        // Пустая диаграмма с сообщением
        private Grid EmptyChart(string message)
        {
            var grid = new Grid();
            var text = new TextBlock
            {
                Text = message,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Gray
            };
            grid.Children.Add(text);
            return grid;
        }

        // Построение круговой диаграммы (Pie)
        private Grid BuildPieChart(List<ChartItem> groupedData, string titleText)
        {
            var seriesCollection = new SeriesCollection();

            // Добавление каждого сектора диаграммы
            foreach (var item in groupedData)
            {
                seriesCollection.Add(new PieSeries
                {
                    Title = item.Name, // Название сектора
                    Values = new ChartValues<int> { item.Count }, // Количество
                    DataLabels = true // Показывать подписи
                });
            }

            // Настройка свойств диаграммы
            var pieChart = new PieChart
            {
                Series = seriesCollection,
                LegendLocation = LegendLocation.Right,
                Hoverable = true,
                Width = 600,
                Height = 400
            };

            // Контейнер для размещения диаграммы
            var container = new Grid();
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.Children.Add(pieChart);
            Grid.SetRow(pieChart, 1);

            return container;
        }

        // Построение столбчатой диаграммы (Column)
        private Grid BuildColumnChart(List<ChartItem> groupedData, string titleText)
        {
            var series = new ColumnSeries
            {
                Title = "Документы",
                Values = new ChartValues<int>(groupedData.Select(x => x.Count)) // Значение по оси Y
            };

            var cartesianChart = new CartesianChart
            {
                Series = new SeriesCollection { series },

                // Ось X (категории)
                AxisX = new AxesCollection
                {
                    new Axis
                    {
                        Title = "Категория",
                        Labels = groupedData.Select(x => x.Name).ToList()
                    }
                },

                // Ось Y (значения)
                AxisY = new AxesCollection
                {
                    new Axis
                    {
                        Title = "Количество документов",
                        LabelFormatter = value => value.ToString("0"),
                        MinValue = 0,
                        Separator = new LiveCharts.Wpf.Separator { Step = 1 }
                    }
                },
                Width = 600,
                Height = 400
            };

            // Контейнер для размещения диаграммы
            var container = new Grid();
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.Children.Add(cartesianChart);
            Grid.SetRow(cartesianChart, 1);

            return container;
        }

        // Получение данных для экспорта
        public List<ChartItem> GetChartData(GroupByOption groupByOption, string filterCode = "")
        {
            var documents = _documentProvider.GetDocuments();
            var groupedData = new List<ChartItem>();

            if (!documents.Any())
                return groupedData;

            // Группировка аналогично BuildChart
            switch (groupByOption)
            {
                case GroupByOption.Type:
                    if (!string.IsNullOrEmpty(filterCode))
                        documents = documents.Where(d => d.StatusName == filterCode).ToList();

                    groupedData = documents
                        .GroupBy(d => d.CodeTypeDocument)
                        .Select(g => new ChartItem
                        {
                            Name = _documentTypeProvider.GetDocumentTypes()
                                    .First(dt => dt.CodeTypeDocument == g.Key).NameType,
                            Count = g.Count()
                        })
                        .ToList();
                    break;

                case GroupByOption.Status:
                    if (!string.IsNullOrEmpty(filterCode))
                        documents = documents.Where(d => d.CodeTypeDocument == filterCode).ToList(); // фильтруем по типу, если выбран фильтр по типу

                    groupedData = documents
                        .GroupBy(d => d.StatusName)
                        .Select(g => new ChartItem
                        {
                            Name = g.Key,
                            Count = g.Count()
                        })
                        .ToList();
                    break;
            }
            return groupedData;
        }
    }
}