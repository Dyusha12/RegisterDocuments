using Microsoft.EntityFrameworkCore;
using RegisterDocuments.Data;
using System.Windows;
using System.Windows.Input;

namespace RegisterDocuments
{
    public partial class AddDocumentWindow : Window
    {
        public AddDocumentWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
        }

        // Загружает типы и статусы документов в ComboBox
        private void LoadComboBoxes()
        {
            var context = new ApplicationContext();

            var types = context.DocumentTypes
                .Select(t => new { t.CodeTypeDocument, t.NameType })
                .ToList();

            DocumentTypeComboBox.ItemsSource = types;

            if (types.Count > 0)
                DocumentTypeComboBox.SelectedIndex = 0;

            var statuses = context.Statuses
                .Select(s => new { s.CodeStatus, s.NameStatus })
                .ToList();

            StatusComboBox.ItemsSource = statuses;

            if (statuses.Count > 0)
                StatusComboBox.SelectedIndex = 0;
        }

        // Закрывает окно без добавления документа
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Добавляет новый документ после проверки валидности данных
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string name = DocumentNameTextBox.Text?.Trim();
            string typeCode = DocumentTypeComboBox.SelectedValue?.ToString();
            string statusCode = StatusComboBox.SelectedValue?.ToString();

            var errors = DocumentValidationHelper.Validate(name, typeCode, statusCode);

            // Если есть ошибки, показываем их пользователю
            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors),
                                "Ошибка заполнения",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var context = new ApplicationContext();

                // Проверка на уникальность названия документа
                bool documentExists = context.Documents
                    .Any(d => d.NameDocument == name);

                if (documentExists)
                {
                    MessageBox.Show("Документ с таким названием уже существует.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // Выполнение хранимой процедуры для добавления документа
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "CALL create_document(@name, @type_code, @status_code)";
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@name", name));
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@type_code", typeCode));
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@status_code", statusCode));
                command.ExecuteNonQuery();

                MessageBox.Show("Документ успешно создан!",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // Отключение прокрутки колеса мыши для ComboBox
        private void DocumentTypeComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        // Отключение прокрутки колеса мыши для ComboBox
        private void StatusComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
    }
}