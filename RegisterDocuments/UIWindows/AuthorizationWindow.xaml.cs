using Microsoft.EntityFrameworkCore;
using Npgsql;
using RegisterDocuments.Data;
using System.Windows;
using System.Windows.Input;

namespace RegisterDocuments
{
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        // Обрабатывает авторизацию пользователя по логину и паролю
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text?.Trim();
            string password = PasswordBox.Password?.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var context = new ApplicationContext();

                // Проверка пароля с помощью хранимой процедуры
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "check_password";

                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_login", login));
                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_password", password));
                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_employee_code", NpgsqlTypes.NpgsqlDbType.Varchar)
                {
                    Direction = System.Data.ParameterDirection.Output
                });

                command.ExecuteNonQuery();

                string employeeCode = command.Parameters["p_employee_code"].Value?.ToString();

                if (string.IsNullOrEmpty(employeeCode))
                {
                    MessageBox.Show("Неверный логин или пароль",
                                    "Ошибка авторизации",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                // Получения данных об текущем пользователе
                CurrentUser.EmployeeCode = employeeCode;

                var employee = context.Employees
                    .Include(e => e.EmployeeRoles)
                        .ThenInclude(er => er.Role)
                    .FirstOrDefault(e => e.EmployeeCode == employeeCode);

                CurrentUser.EmployeeRole = employee?.EmployeeRoles.FirstOrDefault()?.Role.RoleCode;

                var workWindow = new WorkWindow();
                workWindow.Show();
                this.Close();
            }
            catch (PostgresException ex)
            {
                MessageBox.Show(ex.MessageText,
                                "Ошибка авторизации",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Неизвестная ошибка: " + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // Открытие окна для регистрации
        private void OpenRegistration_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationRequestWindow();
            window.Show();
            this.Close();
        }
    }
}