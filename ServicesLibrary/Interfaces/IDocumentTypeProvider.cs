using ServicesLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesLibrary.Interfaces
{
    // Интерфейс провайдера типов документов
    public interface IDocumentTypeProvider
    {
        // Метод для получения списка типов документов
        List<DocumentTypeDto> GetDocumentTypes();
    }
}
