using Microsoft.EntityFrameworkCore;
using RegisterDocuments.Data;
using System.Text.RegularExpressions;
using System.Windows;


namespace RegisterDocuments
{
    /// <summary>
    /// Логика взаимодействия для AddStatusWindow.xaml
    /// </summary>
    public partial class AddDocumentStatusWindow : Window
    {
        public AddDocumentStatusWindow()
        {
            InitializeComponent();
        }

        // Закрывает окно без сохранения изменений
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Добавляет новый статус документа после проверки валидности
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string statusName = StatusNameTextBox.Text?.Trim();

            var errors = StatusValidationHelper.Validate(statusName);

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

                string normalizedName = statusName.Trim().ToLower();

                // Проверка на уникальность статуса
                bool statusExists = context.Statuses
                    .Any(s => s.NameStatus.ToLower() == normalizedName);

                if (statusExists)
                {
                    MessageBox.Show("Статус с таким названием уже существует.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // Выполнение хранимой процедуры для добавления статуса
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "CALL create_status(@name)";
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@name", statusName.Trim()));
                command.ExecuteNonQuery();

                MessageBox.Show("Статус успешно создан!",
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
