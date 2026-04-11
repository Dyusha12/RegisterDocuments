using ServicesLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesLibrary.Interfaces
{
    // Интерфейс сервиса для получения заявок на регистрацию
    public interface IRegistrationRequestModerationService
    {
        // Метод для получения списка заявок на регистрацию
        List<RegistrationRequestDto> GetRequests();
    }
}
