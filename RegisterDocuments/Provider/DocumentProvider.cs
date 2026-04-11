using Microsoft.EntityFrameworkCore;
using RegisterDocuments.Data;
using ServicesLibrary.Interfaces;
using ServicesLibrary.Models;

namespace RegisterDocuments.Provider
{
    // Провайдер для работы с документами
    public class DocumentProvider : IDocumentProvider
    {
        private readonly ApplicationContext _context; // Контекст базы данных

        public DocumentProvider(ApplicationContext context)
        {
            _context = context;
        }

        // Получение списка документов с названием статуса
        public List<DocumentDto> GetDocuments()
        {
            return _context.Documents
                .Include(d => d.Status) // Подгружаем статус документа
                .Select(d => new DocumentDto
                {
                    // Заполнение DTO данными из сущности Document
                    CodeTypeDocument = d.CodeTypeDocument,
                    CodeStatus = d.CodeStatus,
                    StatusName = d.Status.NameStatus
                })
                .ToList();
        }
    }
}