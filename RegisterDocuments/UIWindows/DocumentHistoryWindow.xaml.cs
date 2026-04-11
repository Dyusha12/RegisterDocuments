using Microsoft.EntityFrameworkCore;
using RegisterDocuments.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RegisterDocuments
{
    public partial class DocumentHistoryWindow : Window
    {
        public DocumentHistoryWindow()
        {
            InitializeComponent();
        }

        // Закрывает окно просмотра истории документа
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Выполняет поиск и отображение истории изменений для введённого кода документа
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string docCode = DocumentCodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(docCode))
            {
                MessageBox.Show("Введите код документа", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = new ApplicationContext();

            var history = context.DocumentHistory
                .Where(h => h.DocumentCode == docCode)
                .OrderByDescending(h => h.ChangeDate)
                .Select(h => new
                {
                    EmployeeCode = h.EmployeeCode,
                    ChangeDate = h.ChangeDate
                })
                .ToList();

            if (!history.Any())
            {
                MessageBox.Show("Записи истории для данного документа отсутствуют", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                HistoryDataGrid.ItemsSource = null;
                return;
            }

            HistoryDataGrid.ItemsSource = history;
        }
    }
}