using System.ComponentModel.DataAnnotations.Schema;

namespace RegisterDocuments.Data
{
    public class Status
    {
        [Column("Код_статуса")]
        public string CodeStatus { get; set; } = "";
        [Column("Название_статуса")]
        public string NameStatus { get; set; } = "";
    }
}
