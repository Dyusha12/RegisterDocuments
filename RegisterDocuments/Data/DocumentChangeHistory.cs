using System.ComponentModel.DataAnnotations.Schema;

namespace RegisterDocuments.Data
{
    public class DocumentChangeHistory
    {
        [Column("Код_сотрудника")]
        public string EmployeeCode { get; set; } = null!;

        [Column("Код_документа")]
        public string DocumentCode { get; set; } = null!;

        [Column("Дата_изменения")]
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

        public Employee Employee { get; set; } = null!;
        public Document Document { get; set; } = null!;
    }
}