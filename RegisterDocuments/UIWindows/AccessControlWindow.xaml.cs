using Microsoft.EntityFrameworkCore;
using RegisterDocuments.Data;
using RegisterDocuments.ViewModels;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegisterDocuments
{
    public partial class AccessControlWindow : Window, INotifyPropertyChanged
    {
        public string UserName { get; set; } // Имя текущего пользователя
        public string UserRoleName { get; set; } // Роль текущего пользователя
        public List<EmployeeAccessViewModel> Employees { get; set; } // Отфильтрованный список сотрудников
        private List<EmployeeAccessViewModel> AllEmployees; // Полный список сотрудников
        private bool? currentArchiveFilter = null; // Фильтр архивных сотрудников
        public List<string> RolesList { get; set; } // Список ролей

        private EmployeeAccessViewModel selectedEmployee;
        public EmployeeAccessViewModel SelectedEmployee
        {
            get => selectedEmployee;
            set
            {
                selectedEmployee = value;
                OnPropertyChanged(nameof(SelectedEmployee));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public AccessControlWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadUserInfo();
            LoadRoles();
            LoadEmployees();
        }

        // Загружает информацию о текущем пользователе
        private void LoadUserInfo()
        {
            var context = new ApplicationContext();

            var employee = context.Employees
                .Where(e => e.EmployeeCode == CurrentUser.EmployeeCode)
                .Select(e => new
                {
                    e.LastName,
                    e.FirstName,
                    e.MiddleName,
                    Roles = e.EmployeeRoles.Select(er => er.Role.RoleName).ToList()
                })
                .FirstOrDefault();

            if (employee != null)
            {
                UserName = $"{employee.LastName} {employee.FirstName}".Trim();
                UserRoleName = employee.Roles.FirstOrDefault() ?? CurrentUser.EmployeeRole;
            }
            else
            {
                UserName = CurrentUser.EmployeeCode;
                UserRoleName = CurrentUser.EmployeeRole;
            }
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(UserRoleName));
        }

        // Загружает список всех ролей
        private void LoadRoles()
        {
            var context = new ApplicationContext();

            RolesList = context.Roles
                .Select(r => r.RoleName)
                .ToList();

            OnPropertyChanged(nameof(RolesList));
        }

        // Загружает список всех сотрудников
        private void LoadEmployees()
        {
            var context = new ApplicationContext();

            AllEmployees = context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .Select(e => new EmployeeAccessViewModel
                {
                    Code = e.EmployeeCode,
                    Login = e.Login,
                    Password = e.Password,
                    LastName = e.LastName,
                    FirstName = e.FirstName,
                    MiddleName = e.MiddleName ?? "",
                    RoleName = e.EmployeeRoles.First().Role.RoleName,
                    Archived = e.Archived
                })
                .ToList();

            Employees = AllEmployees.ToList();
            OnPropertyChanged(nameof(Employees));
        }

        // Обрабатывает выбор сотрудника в списке
        private void EmployeesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
                SelectedEmployee = listBox.SelectedItem as EmployeeAccessViewModel;
        }

        // Сохраняет изменения выбранного сотрудника
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var errors = new List<string>();

            // Проверка логина
            var loginError = ValidateLogin(SelectedEmployee.Login);
            if (loginError != null) errors.Add(loginError);

            // Проверка фамилии
            var lastNameError = ValidateLastName(SelectedEmployee.LastName);
            if (lastNameError != null) errors.Add(lastNameError);

            // Проверка имени
            var firstNameError = ValidateFirstName(SelectedEmployee.FirstName);
            if (firstNameError != null) errors.Add(firstNameError);

            // Проверка отчества
            var middleNameError = ValidateMiddleName(SelectedEmployee.MiddleName);
            if (middleNameError != null) errors.Add(middleNameError);

            // Проверка пароля
            var passwordError = ValidatePassword(SelectedEmployee.Password);
            if (passwordError != null) errors.Add(passwordError);

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors),
                                "Ошибка заполнения",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            var context = new ApplicationContext();

            // Проверка на существование логина
            bool loginExists = context.Employees
                .Any(e => e.Login == SelectedEmployee.Login &&
                          e.EmployeeCode != SelectedEmployee.Code);

            if (loginExists)
            {
                MessageBox.Show("Пользователь с таким логином уже существует.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            var employee = context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .FirstOrDefault(e => e.EmployeeCode == SelectedEmployee.Code);

            if (employee == null) return;

            // Получаем роль редактируемого пользователя
            var editedUserRole = employee.EmployeeRoles
                .Where(er => er.Role != null)
                .Select(er => er.Role.RoleName)
                .FirstOrDefault();

            // Проверяем: если редактируем Администратора (не себя)
            bool isEditingAnotherAdmin =
                editedUserRole == "Администратор" &&
                employee.EmployeeCode != CurrentUser.EmployeeCode;

            // Попытка изменить пароль
            bool isPasswordChanged = employee.Password != SelectedEmployee.Password;

            if (isEditingAnotherAdmin && isPasswordChanged)
            {
                MessageBox.Show("Нельзя изменять пароль другого администратора.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Проверка изменения данных
            bool isLoginChanged = employee.Login != SelectedEmployee.Login;
            bool isLastNameChanged = employee.LastName != SelectedEmployee.LastName;
            bool isFirstNameChanged = employee.FirstName != SelectedEmployee.FirstName;
            bool isMiddleNameChanged = employee.MiddleName != SelectedEmployee.MiddleName;

            // Если редактируем чужого администратора - запрещаем менять любые данные
            if (isEditingAnotherAdmin &&
                (isPasswordChanged || isLoginChanged || isLastNameChanged || isFirstNameChanged || isMiddleNameChanged))
            {
                MessageBox.Show("Нельзя изменять данные другого администратора.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Обновление данных
            employee.Login = SelectedEmployee.Login;
            employee.Password = SelectedEmployee.Password;
            employee.LastName = SelectedEmployee.LastName;
            employee.FirstName = SelectedEmployee.FirstName;
            employee.MiddleName = SelectedEmployee.MiddleName;
            employee.Archived = SelectedEmployee.Archived;

            var role = context.Roles
                .FirstOrDefault(r => r.RoleName == SelectedEmployee.RoleName);

            if (role != null)
            {
                employee.EmployeeRoles.Clear();
                employee.EmployeeRoles.Add(new EmployeeRole
                {
                    EmployeeCode = employee.EmployeeCode,
                    RoleCode = role.RoleCode
                });
            }

            context.SaveChanges();

            MessageBox.Show("Изменения сохранены",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            LoadEmployees();
        }

        // Применяет фильтры поиска и архивации к списку сотрудников
        private void ApplyFilter(string? searchText = null, bool? archivedFilter = null)
        {
            var query = AllEmployees.AsQueryable();

            if (archivedFilter.HasValue)
                query = query.Where(e => e.Archived == archivedFilter.Value);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();

                query = query.Where(e =>
                    (e.Login ?? "").ToLower().Contains(searchText) ||
                    (e.FirstName ?? "").ToLower().Contains(searchText) ||
                    (e.LastName ?? "").ToLower().Contains(searchText) ||
                    (e.MiddleName ?? "").ToLower().Contains(searchText) ||
                    (e.RoleName ?? "").ToLower().Contains(searchText)
                );
            }
            else
            {
                LoadEmployees();
            }

            Employees = query.ToList();
            OnPropertyChanged(nameof(Employees));
        }

        // Валидация логина
        private string? ValidateLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return "Логин обязателен.";

            login = login.Trim();

            if (login.Length < 3)
                return "Логин должен содержать минимум 3 символа.";

            if (login.Length > 15)
                return "Логин не должен превышать 15 символов.";

            if (!Regex.IsMatch(login, @"[A-Za-zА-Яа-я]"))
                return "Логин должен содержать хотя бы одну букву.";

            if (!Regex.IsMatch(login, @"^[A-Za-zА-Яа-я0-9-]+$"))
                return "Логин может содержать только буквы, цифры и знак '-'.";

            return null;
        }

        // Проверка фамилии
        private string? ValidateLastName(string lastname)
        {
            if (string.IsNullOrWhiteSpace(lastname))
                return "Фамилия обязательна.";

            lastname = lastname.Trim();

            if (lastname.Length < 2)
                return "Фамилия должна содержать минимум 2 символа.";

            if (lastname.Length > 50)
                return "Фамилия не должна превышать 50 символов.";

            if (!Regex.IsMatch(lastname, @"^[А-Яа-я]+$"))
                return "Фамилия может содержать только русские буквы.";

            return null;
        }


        // Проверка имени
        private string? ValidateFirstName(string firstname)
        {
            if (string.IsNullOrWhiteSpace(firstname))
                return "Имя обязательно.";

            firstname = firstname.Trim();

            if (firstname.Length < 2)
                return "Имя должно содержать минимум 2 символа.";

            if (firstname.Length > 50)
                return "Имя не должно превышать 50 символов.";

            if (!Regex.IsMatch(firstname, @"^[А-Яа-я]+$"))
                return "Имя может содержать только русские буквы.";

            return null;
        }


        // Проверка отчества
        private string? ValidateMiddleName(string middlename)
        {
            if (string.IsNullOrWhiteSpace(middlename))
                return null;

            middlename = middlename.Trim();

            if (middlename.Length < 2)
                return "Отчество должно содержать минимум 2 символа.";

            if (middlename.Length > 50)
                return "Отчество не должно превышать 50 символов.";

            if (!Regex.IsMatch(middlename, @"^[А-Яа-я]+$"))
                return "Отчество может содержать только русские буквы.";

            return null;
        }

        private string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Пароль обязателен.";

            password = password.Trim();

            if (password.Length < 5)
                return "Пароль должен содержать минимум 5 символов.";

            string specialChars = @"!+_/><$#&()=|{}№?*%";

            if (!Regex.IsMatch(password, @"[A-ZА-Я]"))
                return "Пароль должен содержать хотя бы одну заглавную букву.";

            if (!Regex.IsMatch(password, "[" + Regex.Escape(specialChars) + "]"))
                return "Пароль должен содержать хотя бы один спецсимвол";

            // Проверка, чтобы пароль не состоял из только букв, только цифр или только спецсимволов
            bool hasLetter = Regex.IsMatch(password, @"[A-Za-zА-Яа-я]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasSpecial = Regex.IsMatch(password, "[" + Regex.Escape(specialChars) + "]");

            int categories = (hasLetter ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);

            if (categories < 2)
                return "Пароль должен содержать хотя бы два типа символов: буквы, цифры, спецсимволы.";

            return null;
        }

        // Обрабатывает изменение текста поиска
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(SearchTextBox.Text, currentArchiveFilter);
        }

        // Возвращает на окно работы
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {

            var window = new WorkWindow();
            window.Show();
            Close();
        }

        // Показать только активных сотрудников
        private void ShowActive_Click(object sender, RoutedEventArgs e)
        {
            currentArchiveFilter = false;
            ApplyFilter(SearchTextBox.Text, currentArchiveFilter);
        }

        // Показать только архивированных сотрудников
        private void ShowArchived_Click(object sender, RoutedEventArgs e)
        {
            currentArchiveFilter = true;
            ApplyFilter(SearchTextBox.Text, currentArchiveFilter);
        }

        // Открывает окно для добавления нового сотрудника
        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEmployeeWindow
            {
                Owner = this
            };

            if (addWindow.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        // Открывает окно просмотра истории изменений документов
        private void ViewHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new DocumentHistoryWindow
            {
                Owner = this
            };
            window.ShowDialog();
        }

        // Открывает окно просмотра списка заявок на регистрацию
        private void ViewRegistrationListButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationRequestModerationWindow
            {
                Owner = this
            };

            if(window.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        // Обновляет данные окна
        private void RefreshIcon_Click(object sender, RoutedEventArgs e)
        {
            currentArchiveFilter = null;
            LoadEmployees();
            LoadUserInfo();
        }

        // Отключение прокрутки колеса мыши для ComboBox
        private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
    }
}