using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using RegisterDocuments.Data;
using RegisterDocuments.ViewModels;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using ServicesLibrary.Interfaces;
using RegisterDocuments.Provider;
using System.Windows.Input;

namespace RegisterDocuments
{
    /// <summary>
    /// Логика взаимодействия для WorkWindow.xaml
    /// </summary>
    public partial class WorkWindow : Window, INotifyPropertyChanged
    {
        public string UserName { get; set; }
        public string UserRoleName { get; set; }
        private List<DocumentTypeViewModel> _allDocumentTypes;
        private string _currentSearch = "";

        private bool _sortAscendingSend = true;
        private bool _sortAscendingApproval = true;
        private bool _sortAscendingReceive = true;

        private DocumentViewModel _selectedDocument;

        private IDocumentProvider _documentProvider;
        private IDocumentTypeProvider _documentTypeProvider;

        public DocumentViewModel SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                _selectedDocument = value;
                OnPropertyChanged(nameof(SelectedDocument));
            }
        }

        public List<DocumentTypeViewModel> DocumentTypes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        public WorkWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadDocuments();
            LoadUserInfo();
            ApplyRolePermissions();

        }

        public List<string> Statuses { get; set; }
        public List<DocumentTypeViewModel> DocumentTypesList { get; set; }


        // Загружает информацию о текущем пользователе и его роли
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

        // Загружает список документов
        private void LoadDocuments()
        {
            var context = new ApplicationContext();

            _documentProvider = new DocumentProvider(context);
            _documentTypeProvider = new DocumentTypeProvider(context);

            Statuses = context.Statuses
                .Select(s => s.NameStatus)
                .ToList();
            OnPropertyChanged(nameof(Statuses));

            DocumentTypesList = context.DocumentTypes
                .Select(dt => new DocumentTypeViewModel
                {
                    TypeCode = dt.CodeTypeDocument,
                    TypeName = dt.NameType
                })
                .ToList();
            OnPropertyChanged(nameof(DocumentTypesList));

            var documents = context.Documents
                .Include(d => d.Status)
                .Include(d => d.Files)
                    .ThenInclude(f => f.FileType)
                .ToList();

            var grouped = documents
                .GroupBy(d => d.CodeTypeDocument)
                .ToList();

            DocumentTypes = grouped.Select(g => new DocumentTypeViewModel
            {
                TypeName = context.DocumentTypes
                    .First(t => t.CodeTypeDocument == g.Key).NameType,

                Documents = g.Select(d => new DocumentViewModel
                {
                    CodeDocument = d.CodeDocument,
                    DocumentName = d.NameDocument,
                    TypeDocumentCode = d.CodeTypeDocument,
                    IsSent = d.SendTracingPaper,
                    SendDate = d.DateSendLetter?.ToDateTime(TimeOnly.MinValue),
                    ApprovalDate = d.ApprovalDate?.ToDateTime(TimeOnly.MinValue),
                    ReceiveDate = d.DateGetTracingPaper?.ToDateTime(TimeOnly.MinValue),
                    StatusName = d.Status.NameStatus,
                    SentFileName = d.Files
                        .FirstOrDefault(f => f.FileType.NameTypeFile == "Отправленный")
                        ?.FilePath ?? "",
                    ReceivedFileName = d.Files
                        .FirstOrDefault(f => f.FileType.NameTypeFile == "Полученный")
                        ?.FilePath ?? "",
                    Note = d.Note
                }).ToList()
            }).ToList();

            TabControlDocuments.ItemsSource = DocumentTypes;
            _allDocumentTypes = DocumentTypes;
        }

        // Обрабатывает клик по текстовому полю отправленного PDF: открывает файл или позволяет выбрать новый
        private void PdfSentTextBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedDocument == null)
                return;

            e.Handled = true;

            if (!string.IsNullOrWhiteSpace(SelectedDocument.SentFileName))
            {
                // Если файл существует
                if (File.Exists(SelectedDocument.SentFileName))
                {
                    // Открываем файл через стандартное приложение
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SelectedDocument.SentFileName,
                        UseShellExecute = true
                    });
                    return;
                }
                else
                {
                    // Файл отсутствует - предупреждаем пользователя
                    MessageBox.Show("Файл не найден или был перемещен.",
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                    SelectedDocument.SentFileName = null; // cбрасываем путь к файлу
                    OnPropertyChanged(nameof(SelectedDocument));

                    // Находим документ в базе
                    var context = new ApplicationContext();

                    var document = context.Documents
                        .Include(d => d.Files)
                            .ThenInclude(f => f.FileType)
                        .FirstOrDefault(d => d.CodeDocument == SelectedDocument.CodeDocument);

                    if (document != null)
                    {
                        // Удаляем записи о файлах типа "Отправленный"
                        var filesToRemove = document.Files
                            .Where(f => f.FileType.NameTypeFile == "Отправленный")
                            .ToList();

                        if (filesToRemove.Any())
                        {
                            context.RemoveRange(filesToRemove);
                            context.SaveChanges();
                            // Обновляем список документов и сохраняем выбранную вкладку
                            int selectedTabIndex = TabControlDocuments.SelectedIndex;
                            LoadDocuments();
                            if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                                TabControlDocuments.SelectedIndex = selectedTabIndex;
                        }
                    }
                }
            }

            // Если файла нет или пользователь хочет выбрать новый - открываем диалог выбора
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedDocument.SentFileName = openFileDialog.FileName; // сохраняем путь к выбранному файлу
                OnPropertyChanged(nameof(SelectedDocument));
            }
        }

        // Обрабатывает клик по текстовому полю полученного PDF: открывает файл или позволяет выбрать новый
        private void PdfReceivedTextBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedDocument == null)
                return;

            e.Handled = true;

            if (!string.IsNullOrWhiteSpace(SelectedDocument.ReceivedFileName))
            {
                if (File.Exists(SelectedDocument.ReceivedFileName)) // Проверяем существование файла
                {
                    // Открываем файл
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SelectedDocument.ReceivedFileName,
                        UseShellExecute = true
                    });
                    return;
                }
                else
                {
                    // Файл отсутствует — предупреждаем пользователя
                    MessageBox.Show("Файл не найден или был перемещен.",
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                    SelectedDocument.ReceivedFileName = null; // сбрасываем путь к файлу
                    OnPropertyChanged(nameof(SelectedDocument));

                    var context = new ApplicationContext();

                    var document = context.Documents
                        .Include(d => d.Files)
                            .ThenInclude(f => f.FileType)
                        .FirstOrDefault(d => d.CodeDocument == SelectedDocument.CodeDocument);

                    if (document != null)
                    {
                        // Удаляем файлы типа "Полученный", которые уже не существуют
                        var filesToRemove = document.Files
                            .Where(f => f.FileType.NameTypeFile == "Полученный")
                            .ToList();

                        if (filesToRemove.Any())
                        {
                            context.RemoveRange(filesToRemove);
                            context.SaveChanges();
                            // Обновляем список документов и сохраняем выбранную вкладку
                            int selectedTabIndex = TabControlDocuments.SelectedIndex;
                            LoadDocuments();
                            if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                                TabControlDocuments.SelectedIndex = selectedTabIndex;
                        }
                    }
                }
            }

            // Диалог выбора нового PDF файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedDocument.ReceivedFileName = openFileDialog.FileName; // сохраняем путь
                OnPropertyChanged(nameof(SelectedDocument));
            }
        }

        // Обновляет выбранный документ при смене выделения в списке
        private void DocumentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                SelectedDocument = listBox.SelectedItem as DocumentViewModel;
            }
        }

        // Сохраняет изменения документа, проверяет корректность данных и обновляет историю изменений
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument == null)
                return;

            string selectedCode = SelectedDocument.CodeDocument;

            // Сохраняем индекс текущей вкладки для восстановления после перезагрузки списка
            int selectedTabIndex = TabControlDocuments.SelectedIndex;

            SelectedDocument.DocumentName = SelectedDocument.DocumentName?.Trim();

            var errors = new List<string>();

            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(SelectedDocument.DocumentName))
                errors.Add("Название документа обязательно.");

            if (!string.IsNullOrWhiteSpace(SelectedDocument.DocumentName))
            {
                if (SelectedDocument.DocumentName.Length < 3)
                    errors.Add("Название документа должно содержать минимум 3 символа.");

                if (SelectedDocument.DocumentName.Length > 200)
                    errors.Add("Название документа не должно превышать 200 символов.");
            }

            if (string.IsNullOrWhiteSpace(SelectedDocument.StatusName))
                errors.Add("Статус документа обязателен.");

            if (string.IsNullOrWhiteSpace(SelectedDocument.TypeDocumentCode))
                errors.Add("Тип документа обязателен.");


            // Если не выбрана дата отправки
            if (!SelectedDocument.SendDate.HasValue)
            {
                if (SelectedDocument.ApprovalDate.HasValue)
                    errors.Add("Нельзя указать дату согласования без даты отправки письма.");

                if (SelectedDocument.ReceiveDate.HasValue)
                    errors.Add("Нельзя указать дату получения без даты отправки письма.");
            }

            // Если нет даты согласования
            if (!SelectedDocument.ApprovalDate.HasValue && SelectedDocument.ReceiveDate.HasValue)
            {
                errors.Add("Нельзя указать дату получения без даты согласования.");
            }

            // Проверка последовательности дат
            if (SelectedDocument.SendDate.HasValue &&
                SelectedDocument.ReceiveDate.HasValue &&
                SelectedDocument.ReceiveDate < SelectedDocument.SendDate)
            {
                errors.Add("Дата получения не может быть раньше даты отправки.");
            }

            if (SelectedDocument.ApprovalDate.HasValue &&
                SelectedDocument.SendDate.HasValue &&
                SelectedDocument.ApprovalDate < SelectedDocument.SendDate)
            {
                errors.Add("Дата согласования не может быть раньше даты отправки.");
            }

            // Проверка файла полученного письма
            if (!SelectedDocument.ReceiveDate.HasValue &&
                !string.IsNullOrWhiteSpace(SelectedDocument.ReceivedFileName))
            {
                errors.Add("Нельзя прикрепить файл полученного письма без указания даты получения.");
            }

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors),
                                "Ошибка заполнения",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Проверка на уникальность названия
            if (!string.IsNullOrWhiteSpace(SelectedDocument.DocumentName))
            {
                var contextCheck = new ApplicationContext();

                bool nameExists = contextCheck.Documents
                    .Any(d => d.NameDocument == SelectedDocument.DocumentName
                           && d.CodeDocument != SelectedDocument.CodeDocument);

                if (nameExists)
                {
                    MessageBox.Show("Документ с таким названием уже существует.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }
            }

            // Сохранение изменений в базе данных
            var context = new ApplicationContext();

            var document = context.Documents
                .FirstOrDefault(d => d.CodeDocument == SelectedDocument.CodeDocument);

            if (document == null)
                return;

            document.NameDocument = SelectedDocument.DocumentName;
            document.SendTracingPaper = SelectedDocument.IsSent;
            document.DateSendLetter = SelectedDocument.SendDate.HasValue
                ? DateOnly.FromDateTime(SelectedDocument.SendDate.Value) : null;
            document.ApprovalDate = SelectedDocument.ApprovalDate.HasValue
                ? DateOnly.FromDateTime(SelectedDocument.ApprovalDate.Value) : null;
            document.DateGetTracingPaper = SelectedDocument.ReceiveDate.HasValue
                ? DateOnly.FromDateTime(SelectedDocument.ReceiveDate.Value) : null;
            document.DateChangeLine = DateTime.UtcNow;
            document.Note = SelectedDocument.Note;

            var status = context.Statuses
                .FirstOrDefault(s => s.NameStatus == SelectedDocument.StatusName);

            if (status != null)
                document.CodeStatus = status.CodeStatus;

            document.CodeTypeDocument = SelectedDocument.TypeDocumentCode;

            context.SaveChanges();

            // Сохранение файлов
            SaveFile(selectedCode, "Отправленный", SelectedDocument.SentFileName);
            SaveFile(selectedCode, "Полученный", SelectedDocument.ReceivedFileName);

            // Добавление записи истории изменений
            if (!string.IsNullOrEmpty(CurrentUser.EmployeeCode))
            {
                var employee = context.Employees
                    .FirstOrDefault(e => e.EmployeeCode == CurrentUser.EmployeeCode);

                document = context.Documents
                    .FirstOrDefault(d => d.CodeDocument == selectedCode);

                if (employee != null && document != null)
                {
                    var historyRecord = new DocumentChangeHistory
                    {
                        EmployeeCode = employee.EmployeeCode,
                        DocumentCode = document.CodeDocument,
                        ChangeDate = DateTime.UtcNow,
                        Employee = employee,
                        Document = document
                    };

                    context.DocumentHistory.Add(historyRecord);
                    context.SaveChanges();
                }
            }

            MessageBox.Show("Изменения сохранены",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            LoadDocuments();
            TabControlDocuments.SelectedIndex = selectedTabIndex;
        }

        // Сохраняет файл документа в базе данных через хранимую процедуру
        private void SaveFile(string documentCode, string fileTypeName, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            var context = new ApplicationContext();

            var fileType = context.FileTypes
                .FirstOrDefault(ft => ft.NameTypeFile == fileTypeName);

            if (fileType == null)
                return;

            var connection = context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "CALL create_document_file(@doc, @type, @path)";
            command.Parameters.Add(new Npgsql.NpgsqlParameter("@doc", documentCode));
            command.Parameters.Add(new Npgsql.NpgsqlParameter("@type", fileType.CodeTypeFile));
            command.Parameters.Add(new Npgsql.NpgsqlParameter("@path", path));

            command.ExecuteNonQuery();
        }

        // Обрабатывает клик по иконке отправленного PDF: открывает файл или позволяет выбрать новый
        private void PdfSentIcon_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument == null) return;

            // Если уже есть файл, пытаемся открыть
            if (!string.IsNullOrWhiteSpace(SelectedDocument.SentFileName))
            {
                if (File.Exists(SelectedDocument.SentFileName))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SelectedDocument.SentFileName,
                        UseShellExecute = true
                    });
                    return;
                }
                else
                {
                    // Файл не найден - уведомление, очистка и удаление из БД
                    MessageBox.Show("Файл не найден или был перемещен.",
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                    SelectedDocument.SentFileName = null;
                    OnPropertyChanged(nameof(SelectedDocument));

                    var context = new ApplicationContext();

                    var document = context.Documents
                        .Include(d => d.Files)
                            .ThenInclude(f => f.FileType)
                        .FirstOrDefault(d => d.CodeDocument == SelectedDocument.CodeDocument);

                    if (document != null)
                    {
                        var filesToRemove = document.Files
                            .Where(f => f.FileType.NameTypeFile == "Отправленный")
                            .ToList();

                        if (filesToRemove.Any())
                        {
                            context.RemoveRange(filesToRemove);
                            context.SaveChanges();
                            // Обновляем список документов с сохранением вкладки
                            int selectedTabIndex = TabControlDocuments.SelectedIndex;
                            LoadDocuments();
                            if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                                TabControlDocuments.SelectedIndex = selectedTabIndex;
                        }
                    }
                }
            }

            // Выбор нового PDF файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf|Все файлы (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedDocument.SentFileName = openFileDialog.FileName;
                OnPropertyChanged(nameof(SelectedDocument));
            }
        }

        // Обрабатывает клик по иконке полученного PDF: открывает файл или позволяет выбрать новый
        private void PdfReceivedIcon_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument == null) return;

            // Если уже есть файл, пытаемся открыть
            if (!string.IsNullOrWhiteSpace(SelectedDocument.ReceivedFileName))
            {
                if (File.Exists(SelectedDocument.ReceivedFileName))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SelectedDocument.ReceivedFileName,
                        UseShellExecute = true
                    });
                    return;
                }
                else
                {
                    // Файл не найден - уведомление, очистка и удаление из БД
                    MessageBox.Show("Файл не найден или был перемещен.",
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                    SelectedDocument.ReceivedFileName = null;
                    OnPropertyChanged(nameof(SelectedDocument));

                    var context = new ApplicationContext();

                    var document = context.Documents
                        .Include(d => d.Files)
                            .ThenInclude(f => f.FileType)
                        .FirstOrDefault(d => d.CodeDocument == SelectedDocument.CodeDocument);

                    if (document != null)
                    {
                        var filesToRemove = document.Files
                            .Where(f => f.FileType.NameTypeFile == "Полученный")
                            .ToList();

                        if (filesToRemove.Any())
                        {
                            context.RemoveRange(filesToRemove);
                            context.SaveChanges();
                            // Обновляем список документов с сохранением вкладки
                            int selectedTabIndex = TabControlDocuments.SelectedIndex;
                            LoadDocuments();
                            if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                                TabControlDocuments.SelectedIndex = selectedTabIndex;
                        }
                    }
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf|Все файлы (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedDocument.ReceivedFileName = openFileDialog.FileName;
                OnPropertyChanged(nameof(SelectedDocument));
            }
        }

        // Открывает окно для добавления нового типа документа
        private void AddTypeDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddDocumentTypeWindow
            {
                Owner = this
            };

            if (addWindow.ShowDialog() == true)
            {
                int selectedTabIndex = TabControlDocuments.SelectedIndex;
                LoadDocuments();
                if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                    TabControlDocuments.SelectedIndex = selectedTabIndex;
            }
        }

        // Открывает окно для добавления нового статуса документа
        private void AddStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddDocumentStatusWindow
            {
                Owner = this
            };

            if (addWindow.ShowDialog() == true)
            {
                int selectedTabIndex = TabControlDocuments.SelectedIndex;

                LoadDocuments();

                if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                    TabControlDocuments.SelectedIndex = selectedTabIndex;
            }
        }

        // Открывает окно для добавления нового документа
        private void AddDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddDocumentWindow
            {
                Owner = this
            };

            if (addWindow.ShowDialog() == true)
            {
                int selectedTabIndex = TabControlDocuments.SelectedIndex;
                LoadDocuments();
                if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                    TabControlDocuments.SelectedIndex = selectedTabIndex;
            }
        }

        // Удаляет выбранный документ после подтверждения пользователя
        private void DeleteDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка выбранного документа
            if (SelectedDocument == null)
            {
                MessageBox.Show("Выберите документ для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить документ \"{SelectedDocument.DocumentName}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var context = new ApplicationContext();

                // Поиск документа в базе
                var document = context.Documents
                    .FirstOrDefault(d => d.CodeDocument == SelectedDocument.CodeDocument);

                if (document == null)
                {
                    MessageBox.Show("Документ не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Удаление и сохранение изменений
                context.Documents.Remove(document);
                context.SaveChanges();

                MessageBox.Show("Документ успешно удалён.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновление списка документов с сохранением вкладки
                int selectedTabIndex = TabControlDocuments.SelectedIndex;
                LoadDocuments();
                if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                    TabControlDocuments.SelectedIndex = selectedTabIndex;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении документа:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обрабатывает выход пользователя из системы с подтверждением
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            var loginWindow = new AuthorizationWindow();
            loginWindow.Show();
            this.Close();
        }

        // Открывает окно управления доступом и закрывает текущее
        private void AccessControlButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AccessControlWindow();
            window.Show();
            this.Close();
        }

        // Применяет права доступа пользователя к элементам интерфейса
        private void ApplyRolePermissions()
        {
            string role = CurrentUser.EmployeeRole;

            AddTypeDocumentButton.IsEnabled = false;

            AddStatusButton.IsEnabled = false;
            AddDocumentButton.IsEnabled = false;
            AccessControlButton.IsEnabled = false;
            DeleteDocumentButton.IsEnabled = false;
            SaveButton.IsEnabled = false;

            bool allowEditFields = false;

            switch (role)
            {
                case "RL1":
                    allowEditFields = false;
                    break;

                case "RL2":
                    AddDocumentButton.IsEnabled = true;
                    SaveButton.IsEnabled = true;
                    allowEditFields = true;
                    break;

                case "RL3":
                    AddTypeDocumentButton.IsEnabled = true;
                    AddStatusButton.IsEnabled = true;
                    AddDocumentButton.IsEnabled = true;
                    AccessControlButton.IsEnabled = true;
                    DeleteDocumentButton.IsEnabled = true;
                    SaveButton.IsEnabled = true;
                    allowEditFields = true;
                    break;
            }

            SetEditingEnabled(allowEditFields);
        }

        // Включает или отключает редактирование полей документа
        private void SetEditingEnabled(bool isEnabled)
        {
            DocumentNameTextBox.IsReadOnly = !isEnabled;
            DocumentTypeComboBox.IsEnabled = isEnabled;
            SendTracingPaperCheckBox.IsEnabled = isEnabled;
            StatusComboBox.IsEnabled = isEnabled;
            SendDatePicker.IsEnabled = isEnabled;
            ApprovalDatePicker.IsEnabled = isEnabled;
            ReceiveDatePicker.IsEnabled = isEnabled;
            PdfSentTextBox.IsEnabled = isEnabled;
            PdfReceivedTextBox.IsEnabled = isEnabled;
            DocumentNoteTextBox.IsEnabled = isEnabled;
        }

        // Удаляет отправленный PDF из выбранного документа
        private void PdfSentDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument == null) return;

            SelectedDocument.SentFileName = string.Empty;
            OnPropertyChanged(nameof(SelectedDocument));

            DeleteDocumentFile(SelectedDocument.CodeDocument, "Отправленный");
        }

        // Удаляет полученный PDF из выбранного документа
        private void PdfReceivedDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument == null) return;

            SelectedDocument.ReceivedFileName = string.Empty;
            OnPropertyChanged(nameof(SelectedDocument));

            DeleteDocumentFile(SelectedDocument.CodeDocument, "Полученный");
        }

        // Удаляет файл документа через хранимую процедуру
        private void DeleteDocumentFile(string documentCode, string fileTypeName)
        {
            var context = new ApplicationContext();

            var fileType = context.FileTypes
                .FirstOrDefault(ft => ft.NameTypeFile == fileTypeName);

            if (fileType == null)
                return;

            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "CALL delete_document_file(@doc, @type)";
            command.Parameters.Add(new Npgsql.NpgsqlParameter("@doc", documentCode));
            command.Parameters.Add(new Npgsql.NpgsqlParameter("@type", fileType.CodeTypeFile));

            command.ExecuteNonQuery();
        }

        // Фильтрует документы по дате отправки
        private void FilterBySendDate_Click(object sender, RoutedEventArgs e)
        {
            SortDate("send");
        }

        // Фильтрует документы по дате утверждения
        private void FilterByApprovalDate_Click(object sender, RoutedEventArgs e)
        {
            SortDate("approval");
        }

        // Фильтрует документы по дате получения
        private void FilterByReceiveDate_Click(object sender, RoutedEventArgs e)
        {
            SortDate("receive");
        }

        // Обрабатывает изменение текста поиска и применяет фильтры
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = SearchTextBox.Text?.ToLower() ?? "";
            ApplyFilters();
        }

        // Применяет текущие фильтры поиска к списку документов
        private void ApplyFilters()
        {
            var filteredTypes = _allDocumentTypes
                .Select(type => new DocumentTypeViewModel
                {
                    TypeCode = type.TypeCode,
                    TypeName = type.TypeName,
                    Documents = type.Documents
                        .Where(d =>
                            string.IsNullOrWhiteSpace(_currentSearch) ||
                            (d.DocumentName ?? "").ToLower().Contains(_currentSearch.ToLower()) ||
                            (d.StatusName ?? "").ToLower().Contains(_currentSearch.ToLower())
                        )
                        .ToList()
                })
                .Where(t => t.Documents.Any())
                .ToList();

            DocumentTypes = filteredTypes;
            TabControlDocuments.ItemsSource = DocumentTypes;
        }

        // Сортирует документы по выбранной дате
        private void SortDate(string dateType)
        {
            if (TabControlDocuments.SelectedItem is not DocumentTypeViewModel selectedType)
                return;

            switch (dateType)
            {
                case "send":
                    if (_sortAscendingSend)
                        selectedType.Documents = selectedType.Documents
                            .OrderBy(d => d.SendDate == null)
                            .ThenBy(d => d.SendDate)
                            .ToList();
                    else
                        selectedType.Documents = selectedType.Documents
                            .OrderBy(d => d.SendDate == null)
                            .ThenByDescending(d => d.SendDate)
                            .ToList();

                    _sortAscendingSend = !_sortAscendingSend;
                    break;

                case "approval":
                    if (_sortAscendingApproval)
                        selectedType.Documents = selectedType.Documents
                            .OrderBy(d => d.ApprovalDate == null)
                            .ThenBy(d => d.ApprovalDate)
                            .ToList();
                    else
                        selectedType.Documents = selectedType.Documents
                            .OrderBy(d => d.ApprovalDate == null)
                            .ThenByDescending(d => d.ApprovalDate)
                            .ToList();

                    _sortAscendingApproval = !_sortAscendingApproval;
                    break;

                case "receive":
                    if (_sortAscendingReceive)
                        selectedType.Documents = selectedType.Documents
                            .OrderBy(d => d.ReceiveDate == null)
                            .ThenBy(d => d.ReceiveDate)
                            .ToList();
                    else
                        selectedType.Documents = selectedType.Documents
                            .OrderBy(d => d.ReceiveDate == null)
                            .ThenByDescending(d => d.ReceiveDate)
                            .ToList();

                    _sortAscendingReceive = !_sortAscendingReceive;
                    break;
            }

            TabControlDocuments.Items.Refresh();
        }

        // Обновляет список документов и сохраняет текущую вкладку
        private void RefreshIcon_Click(object sender, RoutedEventArgs e)
        {
            int selectedTabIndex = TabControlDocuments.SelectedIndex;
            LoadDocuments();
            if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                TabControlDocuments.SelectedIndex = selectedTabIndex;
        }

        // Открытие окна статистики документов
        private void ShowStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new StatisticsWindow(_documentProvider, _documentTypeProvider)
            {
                Owner = this
            };
            if (window.ShowDialog() == true)
            {
                // Сохраняем текущую вкладку, обновляем список документов и восстанавливаем вкладку
                int selectedTabIndex = TabControlDocuments.SelectedIndex;
                LoadDocuments();
                if (selectedTabIndex >= 0 && selectedTabIndex < TabControlDocuments.Items.Count)
                    TabControlDocuments.SelectedIndex = selectedTabIndex;
            }

        }

        // Блокировка прокрутки колесиком мыши для ComboBox
        private void DocumentTypeComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        // Блокировка прокрутки колесиком мыши для ComboBox
        private void StatusComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
    }
}