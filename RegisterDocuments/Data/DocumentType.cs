using System.ComponentModel.DataAnnotations.Schema;

namespace RegisterDocuments.Data
{
    public class DocumentType
    {
        [Column("Код_типа_документа")]
        public string CodeTypeDocument { get; set; } = "";
        [Column("Название_типа")]
        public string NameType { get; set; } = "";
        public List<Document> Documents { get; set; } = new();
    }
}
