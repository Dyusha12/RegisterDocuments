using RegisterDocuments.Data;
using ServicesLibrary.Interfaces;
using ServicesLibrary.Models;

namespace RegisterDocuments.Provider
{
    // Провайдер для работы с типами документов
    public class DocumentTypeProvider : IDocumentTypeProvider
    {
        private readonly ApplicationContext _context; // Контекст базы данных

        public DocumentTypeProvider(ApplicationContext context)
        {
            _context = context;
        }

        // Получение списка типов документов
        public List<DocumentTypeDto> GetDocumentTypes()
        {
            return _context.DocumentTypes
                .Select(dt => new DocumentTypeDto
                {
                    // Заполнение DTO данными из сущности DocumentType
                    CodeTypeDocument = dt.CodeTypeDocument,
                    NameType = dt.NameType
                })
                .ToList();
        }
    }
}