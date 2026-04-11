using RegisterDocuments.Data;
using RegisterDocuments.Provider;
using ServicesLibrary.Modules;
using ServicesLibrary.Interfaces;
using ServicesLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RegisterDocuments
{
    // Окно модерации заявок на регистрацию
    public partial class RegistrationRequestModerationWindow : Window
    {
        private readonly IRegistrationRequestModerationService _requestProvider; // Для отображения
        private readonly RegistrationRequestModerationService _service; // Сервис для одобрения или отклонения заявки
        private List<RegistrationRequestDto> _requests;

        public RegistrationRequestModerationWindow()
        {
            InitializeComponent();

            var context = new ApplicationContext();

            _requestProvider = new RegistrationRequestProvider(context); // Инициализация провайдера

            _service = new RegistrationRequestModerationService(context); // Инициализация сервиса

            LoadRequests(); // Загрузка заявок при открытии окна
        }

        // Загрузка списка заявок
        private void LoadRequests()
        {
            _requests = _requestProvider.GetRequests();
            RequestsListBox.ItemsSource = _requests;
            UpdateEmptyState();
        }

        // Обновление списка
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }

        // Одобрение выбранной заявки
        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            // Получение выбранной заявки
            var selected = RequestsListBox.SelectedItem as RegistrationRequestDto;

            if (selected == null)
            {
                // Если ничего не выбрано - отображение предупреждения
                MessageBox.Show("Выберите заявку.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Подтверждение действия
            var result = MessageBox.Show(
                $"Одобрить заявку пользователя {selected.Login}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Вызов сервиса для одобрения заявки
                _service.ApproveRequest(selected.CodeApplication);

                MessageBox.Show("Заявка успешно одобрена.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadRequests();
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                MessageBox.Show(ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Отклонение выбранной заявки
        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            // Получение выбранной заявки
            var selected = RequestsListBox.SelectedItem as RegistrationRequestDto;

            if (selected == null)
            {
                MessageBox.Show("Выберите заявку.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Подтверждение отклонения
            var result = MessageBox.Show(
                $"Отклонить заявку пользователя {selected.Login}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Вызов сервиса для отклонения заявки
                _service.RejectRequest(selected.CodeApplication);

                MessageBox.Show("Заявка отклонена.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Закрытие окна
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        // Фильтрация списка заявок по тексту поиска
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_requests == null)
                return;

            string search = SearchTextBox.Text.ToLower();

            // Фильтрация по нескольким полям
            RequestsListBox.ItemsSource = _requests
                .Where(r =>
                    (r.Login ?? "").ToLower().Contains(search) ||
                    (r.LastName ?? "").ToLower().Contains(search) ||
                    (r.Name ?? "").ToLower().Contains(search) ||
                    (r.MiddleName ?? "").ToLower().Contains(search))
                .ToList();
            UpdateEmptyState();
        }

        // Метод проверяет, есть ли элементы в списке заявок,
        // и в зависимости от этого показывает или скрывает сообщение о пустом списке
        private void UpdateEmptyState()
        {
            if (RequestsListBox.Items.Count == 0)
            {
                // Показываем сообщение
                EmptyTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                // Скрываем сообщение, если заявки есть
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }
        }
    }
}