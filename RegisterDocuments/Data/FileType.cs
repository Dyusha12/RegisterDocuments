using System.ComponentModel.DataAnnotations.Schema;

namespace RegisterDocuments.Data
{
    public class FileType
    {
        [Column("Код_типа_файла")]
        public string CodeTypeFile { get; set; } = "";
        [Column("Название_типа_файла")]
        public string NameTypeFile { get; set; } = "";
    }
}
