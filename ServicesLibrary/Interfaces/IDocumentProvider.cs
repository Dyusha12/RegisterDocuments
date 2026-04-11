using ServicesLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesLibrary.Interfaces
{
    // Интерфейс провайдера документов
    public interface IDocumentProvider
    {
        // Метод для получения списка документов
        List<DocumentDto> GetDocuments();
    }
}
