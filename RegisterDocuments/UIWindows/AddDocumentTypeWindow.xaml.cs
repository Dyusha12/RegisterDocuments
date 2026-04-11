using Microsoft.EntityFrameworkCore;
using RegisterDocuments.Data;
using System.Text.RegularExpressions;
using System.Windows;

namespace RegisterDocuments
{
    public partial class AddDocumentTypeWindow : Window
    {
        public AddDocumentTypeWindow()
        {
            InitializeComponent();
        }

        // Закрывает окно без сохранения изменений
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Добавляет новый тип документа после проверки валидности
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string typeName = TypeNameTextBox.Text?.Trim();

            var errors = new List<string>();

            // Проверка обязательного поля
            if (string.IsNullOrWhiteSpace(typeName))
                errors.Add("Название типа документа обязательно.");

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                int letterCount = typeName.Count(char.IsLetter);
                if (letterCount < 3)
                    errors.Add("Название типа документа должно содержать минимум 3 буквы.");

                if (typeName.Length > 50)
                    errors.Add("Название типа документа не должно превышать 50 символов.");

                if (!Regex.IsMatch(typeName, @"^[А-Яа-яA-Za-z\s-.]+$"))
                    errors.Add("Название типа документа может содержать только буквы, пробелы, дефисы и точку.");
            }

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

                string normalizedName = typeName.Trim().ToLower();

                // Проверка на уникальность типа документа
                bool typeExists = context.DocumentTypes
                    .Any(t => t.NameType.ToLower() == normalizedName);

                if (typeExists)
                {
                    MessageBox.Show("Тип документа с таким названием уже существует.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // Выполнение хранимой процедуры для добавления типа документа
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "CALL create_document_type(@name)";
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@name", typeName.Trim()));
                command.ExecuteNonQuery();

                MessageBox.Show("Тип документа успешно создан!",
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
    }
}