using System.ComponentModel.DataAnnotations.Schema;

namespace RegisterDocuments.Data
{
    public class Document
    {
        [Column("Код_документа")]
        public string CodeDocument { get; set; } = "";
        [Column("Код_типа_документа")]
        public string CodeTypeDocument { get; set; } = "";
        [Column("Название_документа")]
        public string NameDocument { get; set; } = "";
        [Column("Калька_отправлена")]
        public bool SendTracingPaper { get; set; }
        [Column("Код_статуса")]
        public string CodeStatus { get; set; } = "";
        [Column("Дата_отправки_письма")]
        public DateOnly? DateSendLetter { get; set; }
        [Column("Дата_согласования")]
        public DateOnly? ApprovalDate { get; set; }
        [Column("Дата_получения_кальки")]
        public DateOnly? DateGetTracingPaper { get; set; }
        [Column("Дата_изменения_записи")]
        public DateTime DateChangeLine { get; set; }

        [Column("Примечание")]
        public string? Note { get; set; } = "";

        public List<DocumentFile> Files { get; set; } = new();
        public Status Status { get; set; } = null!;
    }
}
