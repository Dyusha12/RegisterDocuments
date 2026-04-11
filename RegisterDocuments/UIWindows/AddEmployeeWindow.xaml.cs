using Microsoft.EntityFrameworkCore;
using RegisterDocuments.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace RegisterDocuments
{
    /// <summary>
    /// Логика взаимодействия для AddEmployeeWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        public AddEmployeeWindow()
        {
            InitializeComponent();
        }

        // Закрывает окно без сохранения изменений
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Добавляет нового сотрудника после проверки валидности
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text?.Trim();
            string password = PasswordBox.Password?.Trim();
            string lastname = LastNameTextBox.Text?.Trim();
            string firstname = FirstNameTextBox.Text?.Trim();
            string middlename = MiddleNameTextBox.Text?.Trim();

            var errors = new List<string>();

            var loginError = ValidateLogin(login);
            if (loginError != null) errors.Add(loginError);

            var passwordError = ValidatePassword(password);
            if (passwordError != null) errors.Add(passwordError);

            var lastNameError = ValidateLastName(lastname);
            if (lastNameError != null) errors.Add(lastNameError);

            var firstNameError = ValidateFirstName(firstname);
            if (firstNameError != null) errors.Add(firstNameError);

            var middleNameError = ValidateMiddleName(middlename);
            if (middleNameError != null) errors.Add(middleNameError);

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors),
                    "Ошибка заполнения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

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

                // Проверка на уникальность логина
                if (context.Employees.Any(e => e.Login == login))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Выполнение хранимой процедуры для добавления сотрудника
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "CALL add_employee(@login, @password, @lastname, @firstname, @middlename)";
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@login", login));
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@password", password));
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@lastname", lastname));
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@firstname", firstname));
                command.Parameters.Add(new Npgsql.NpgsqlParameter("@middlename", middlename));
                command.ExecuteNonQuery();

                MessageBox.Show("Сотрудник создан успешно!",
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

        // Проверка логина
        private string? ValidateLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return "Логин обязателен.";

            if (login.Contains(" "))
                return "Логин не должен содержать пробелы.";

            if (login.Length < 3)
                return "Логин должен содержать минимум 3 символа.";

            if (login.Length > 15)
                return "Логин не должен превышать 15 символов.";

            if (!Regex.IsMatch(login, @"[a-zA-Zа-яА-Я]"))
                return "Логин должен содержать хотя бы одну букву.";

            if (!Regex.IsMatch(login, @"^[a-zA-Zа-яА-Я0-9-]+$"))
                return "Логин может содержать только буквы, цифры и знак '-'.";

            return null;
        }


        // Проверка пароля
        private string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Пароль обязателен.";

            if (password.Contains(" "))
                return "Пароль не должен содержать пробелы.";

            if (password.Length < 5)
                return "Пароль должен содержать минимум 5 символов.";

            if (!Regex.IsMatch(password, @"[A-ZА-Я]"))
                return "Пароль должен содержать хотя бы одну заглавную букву.";

            if (!Regex.IsMatch(password, @"[!+_/><$#&()=|{}№?*%]"))
                return "Пароль должен содержать хотя бы один спецсимвол.";

            bool hasLetter = Regex.IsMatch(password, @"[a-zA-Zа-яА-Я]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasSpecial = Regex.IsMatch(password, @"[!""№?*%]");

            int categories = (hasLetter ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);

            if (categories < 2)
                return "Пароль должен содержать буквы, цифры или спецсимволы (не один тип).";

            return null;
        }


        // Проверка фамилии
        private string? ValidateLastName(string lastname)
        {
            if (string.IsNullOrWhiteSpace(lastname))
                return "Фамилия обязательна.";

            if (lastname.Contains(" "))
                return "Фамилия не должна содержать пробелы.";

            if (lastname.Length < 2)
                return "Фамилия должна содержать минимум 2 символа.";

            if (lastname.Length > 50)
                return "Фамилия не должна превышать 50 символов.";

            if (!Regex.IsMatch(lastname, @"^[а-яА-Я]+$"))
                return "Фамилия может содержать только русские буквы.";

            return null;
        }


        // Проверка имени
        private string? ValidateFirstName(string firstname)
        {
            if (string.IsNullOrWhiteSpace(firstname))
                return "Имя обязательно.";

            if (firstname.Contains(" "))
                return "Имя не должно содержать пробелы.";

            if (firstname.Length < 2)
                return "Имя должно содержать минимум 2 символа.";

            if (firstname.Length > 50)
                return "Имя не должно превышать 50 символов.";

            if (!Regex.IsMatch(firstname, @"^[а-яА-Я]+$"))
                return "Имя может содержать только русские буквы.";

            return null;
        }


        // Проверка отчества
        private string? ValidateMiddleName(string middlename)
        {
            if (string.IsNullOrWhiteSpace(middlename))
                return null;

            if (middlename.Contains(" "))
                return "Отчество не должно содержать пробелы.";

            if (middlename.Length < 2)
                return "Отчество должно содержать минимум 2 символа.";

            if (middlename.Length > 50)
                return "Отчество не должно превышать 50 символов.";

            if (!Regex.IsMatch(middlename, @"^[а-яА-Я]+$"))
                return "Отчество может содержать только русские буквы.";

            return null;
        }
    }
}
