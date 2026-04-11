using RegisterDocuments.Data;
using System.Windows;
using System.Windows.Controls;
using ServicesLibrary.Modules;
using System.Windows.Input;
using Npgsql;

namespace RegisterDocuments
{
    /// <summary>
    /// Логика взаимодействия для RegistrationRequestWindow.xaml
    /// </summary>
    public partial class RegistrationRequestWindow : Window
    {
        private readonly RegistrationRequestService _service; // сервис для валидации и отправки заявки

        // Флаг отображения пароля
        private bool _isPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        public RegistrationRequestWindow()
        {
            InitializeComponent();

            var context = new ApplicationContext();
            _service = new RegistrationRequestService(context); // Инициализация сервиса
        }

        // Отмена и возврат к окну авторизации
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OpenAuthorizationWindow();
        }

        // Отправка заявки на регистрацию
        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            // Получение значений из полей формы
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string lastname = LastNameBox.Text.Trim();
            string firstname = FirstNameBox.Text.Trim();
            string middlename = MiddleNameBox.Text.Trim();

            // Валидация полей через сервис
            var errors = _service.ValidateRequest(
                login,
                lastname,
                firstname,
                middlename,
                password,
                confirmPassword
            );

            // Если есть ошибки - вывести их списком
            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors),
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Подтверждение отправки заявки
            var result = MessageBox.Show(
                "Вы уверены, что хотите отправить заявку на регистрацию?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Отправка заявки в систему
                _service.SubmitRequest(login, lastname, firstname, middlename, password);

                MessageBox.Show("Заявка успешно отправлена!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // После успешной отправки возврат к авторизации
                OpenAuthorizationWindow();
            }
            catch (PostgresException ex)
            {
                // Обработка ошибок PostgreSQL
                MessageBox.Show(ex.MessageText,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Общая обработка ошибок
                MessageBox.Show(ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Открытие окна авторизации
        private void OpenAuthorizationWindow()
        {
            var window = new AuthorizationWindow();
            window.Show();
            this.Close();
        }

        // Синхронизация пароля
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
        }

        // Синхронизация пароля для подтверждения
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
        }

        // Показ или скрытие пароля
        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Показ пароля
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Возврат обратно в PasswordBox
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }
        }

        // Показ или скрытие подтверждения пароля
        private void ToggleConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;

            if (_isConfirmPasswordVisible)
            {
                // Показ пароля
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
                ConfirmPasswordTextBox.Visibility = Visibility.Visible;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Возврат обратно в PasswordBox
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
                ConfirmPasswordTextBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
            }
        }
    }
}