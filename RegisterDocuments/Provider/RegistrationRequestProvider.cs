using RegisterDocuments.Data;
using ServicesLibrary.Interfaces;
using ServicesLibrary.Models;

namespace RegisterDocuments.Provider
{
    // Провайдер для работы с регистрационными заявками
    public class RegistrationRequestProvider : IRegistrationRequestModerationService
    {
        private readonly ApplicationContext _context; // Контекст базы данных

        public RegistrationRequestProvider(ApplicationContext context)
        {
            _context = context;
        }

        // Получение списка новых заявок, отсортированных по дате создания
        public List<RegistrationRequestDto> GetRequests()
        {
            return _context.RegistrationRequests
                .Where(r => r.Status == "Новая") // Только новые заявки
                .OrderBy(r => r.DateCreate)      // Сортировка по дате создания
                .Select(r => new RegistrationRequestDto
                {
                    // Заполнение DTO данными из сущности RegistrationRequest
                    CodeApplication = r.CodeApplication,
                    Login = r.Login,
                    Password = r.Password,
                    LastName = r.LastName,
                    Name = r.Name,
                    MiddleName = r.MiddleName,
                    Status = r.Status,
                    DateCreate = r.DateCreate
                })
                .ToList();
        }
    }
}