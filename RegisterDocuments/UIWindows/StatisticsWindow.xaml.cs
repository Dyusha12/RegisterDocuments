using ServicesLibrary.Interfaces;
using ServicesLibrary.Modules;
using ServicesLibrary.Models;
using RegisterDocuments.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace RegisterDocuments
{
    // Окно для просмотра статистики документов
    public partial class StatisticsWindow : Window, INotifyPropertyChanged
    {
        public List<DocumentTypeViewModel> DocumentTypesList { get; set; } // Список типов документов
        public List<string> Statuses { get; set; } // Список статусов

        private StatisticsService _chartBuilder; // Сервис построения диаграмм
        private string _chartTitle; 
        private bool _allDocuments; // Флаг показа всех документов
        private bool _chartBuilt = false; // Флаг данных диаграммы

        // Провайдеры данных
        private readonly IDocumentProvider _documentProvider;
        private readonly IDocumentTypeProvider _documentTypeProvider;

        // Свойство заголовка диаграммы
        public string ChartTitle
        {
            get => _chartTitle;
            set
            {
                _chartTitle = value;
                OnPropertyChanged(nameof(ChartTitle));
            }
        }

        // Событие для уведомления об изменениях
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public StatisticsWindow(IDocumentProvider documentProvider, IDocumentTypeProvider documentTypeProvider)
        {
            InitializeComponent();
            DataContext = this;

            _documentProvider = documentProvider;
            _documentTypeProvider = documentTypeProvider;

            // Инициализация сервиса построения диаграмм
            _chartBuilder = new StatisticsService(_documentProvider, _documentTypeProvider);

            // Установка значений по умолчанию
            PieChartRadioButton.IsChecked = true;
            FilterByTypeRadioButton.IsChecked = true;

            // Загрузка данных
            LoadDocumentTypes();
            LoadStatuses();
            ShowEmptyChartMessage();

            // Подписка на изменение типа диаграммы и фильтров
            PieChartRadioButton.Checked += ChartTypeRadioButton_Checked;
            BarChartRadioButton.Checked += ChartTypeRadioButton_Checked;
            FilterByTypeRadioButton.Checked += FilterTypeRadioButton_Checked;
            FilterByStatusRadioButton.Checked += FilterTypeRadioButton_Checked;
        }

        // Загрузка типов документов
        private void LoadDocumentTypes()
        {
            DocumentTypesList = _documentTypeProvider.GetDocumentTypes()
                .Select(dt => new DocumentTypeViewModel
                {
                    TypeCode = dt.CodeTypeDocument,
                    TypeName = dt.NameType
                })
                .ToList();

            OnPropertyChanged(nameof(DocumentTypesList));
        }

        // Загрузка статусов документов
        private void LoadStatuses()
        {
            Statuses = _documentProvider.GetDocuments()
                .Select(d => d.StatusName)
                .Distinct()
                .ToList();

            OnPropertyChanged(nameof(Statuses));
            StatusComboBox.SelectedItem = null;
        }

        // Изменение выбранного типа документа
        private void DocumentTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterByTypeRadioButton.IsChecked == true)
                UpdateChart();
        }

        // Изменение выбранного статуса
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterByStatusRadioButton.IsChecked == true)
                UpdateChart();
        }

        // Смена типа диаграммы
        private void ChartTypeRadioButton_Checked(object sender, RoutedEventArgs e) => UpdateChart();
       
        // Смена типа фильтра
        private void FilterTypeRadioButton_Checked(object sender, RoutedEventArgs e) => UpdateChart();

        // Показ статистики по всем документам
        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            _allDocuments = true;
            _chartBuilt = true;
            var chartType = PieChartRadioButton.IsChecked == true ? ChartType.Pie : ChartType.Column;
            var chart = _chartBuilder.BuildChart(GroupByOption.Type, "", "", chartType);
            ChartTitle = "Общая статистика по всем типам документов";
            VisualChartContainer.Content = chart;
        }

        // Построение диаграммы с учетом фильтров
        private void UpdateChart()
        {
            var chartType = PieChartRadioButton.IsChecked == true ? ChartType.Pie : ChartType.Column;

            // Фильтр по типу документа
            if (FilterByTypeRadioButton.IsChecked == true)
            {
                if (DocumentTypeComboBox.SelectedItem is not DocumentTypeViewModel selectedType) return;

                _allDocuments = false;
                var chart = _chartBuilder.BuildChart(
                    GroupByOption.Status,
                    filterCode: selectedType.TypeCode,
                    filterName: selectedType.TypeName,
                    chartType: chartType);

                ChartTitle = $"Статистика по документам типа \"{selectedType.TypeName}\"";
                VisualChartContainer.Content = chart;
                _chartBuilt = true;
            }

            // Фильтр по статусу
            else if (FilterByStatusRadioButton.IsChecked == true)
            {
                if (StatusComboBox.SelectedItem is not string selectedStatus) return;

                _allDocuments = false;
                var chart = _chartBuilder.BuildChart(
                    GroupByOption.Type,
                    filterCode: selectedStatus,
                    filterName: selectedStatus,
                    chartType: chartType);

                ChartTitle = $"Документы со статусом \"{selectedStatus}\"";
                VisualChartContainer.Content = chart;
                _chartBuilt = true;
            }
        }

        // Закрытие окна
        private void ExitButton_Click(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Обновление данных и сброс диаграммы
        private void RefreshIcon_Click(object sender, MouseButtonEventArgs e)
        {
            LoadDocumentTypes();
            LoadStatuses();
            ShowEmptyChartMessage();
            _allDocuments = false;
            _chartBuilt = false;
        }
        
        // Показ сообщения о пустой диаграмме
        private void ShowEmptyChartMessage()
        {
            ChartTitle = "";
            string message = "Нет данных для построения статистики";
            var textBlock = new TextBlock
            {
                Text = message,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center
            };

            var grid = new Grid();
            grid.Children.Add(textBlock);

            VisualChartContainer.Content = grid;
        }

        // Экспорт диаграммы в Excel
        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (!_chartBuilt)
            {
                MessageBox.Show("Сначала необходимо построить диаграмму.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var exportService = new ExcelExportService();
            List<ChartItem> data = new List<ChartItem>();

            // Получение данных в зависимости от выбранного фильтра
            if (FilterByTypeRadioButton.IsChecked == true)
            {
                if (!_allDocuments && DocumentTypeComboBox.SelectedItem is DocumentTypeViewModel selectedType)
                {
                    data = _chartBuilder.GetChartData(GroupByOption.Status, selectedType.TypeCode);
                }
                else
                {
                    data = _chartBuilder.GetChartData(GroupByOption.Type);
                }
            }
            else
            {
                if (!_allDocuments && StatusComboBox.SelectedItem is string selectedStatus)
                {
                    data = _chartBuilder.GetChartData(GroupByOption.Type, selectedStatus);
                }
                else
                {
                    data = _chartBuilder.GetChartData(GroupByOption.Type);
                }
            }

            // Проверка наличия данных
            if (!data.Any())
            {
                MessageBox.Show("Нет данных для экспорта");
                return;
            }

            // Диалог сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                FileName = $"Statistics_{System.DateTime.Now:yyyy-MM-dd hh-mm-ss}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Экспорт в Excel
                bool saved = exportService.ExportChartDataToExcel(data, ChartTitle, saveFileDialog.FileName);
                MessageBox.Show(saved ? "Файл успешно сохранён!" : "Сохранение файла отменено или файл занят Excel.",
                                "Экспорт в Excel", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Блокировка прокрутки колесиком мыши
        private void DocumentTypeComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void StatusComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
    }
}