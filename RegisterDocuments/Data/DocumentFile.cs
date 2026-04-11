using System.ComponentModel.DataAnnotations.Schema;


namespace RegisterDocuments.Data
{
    public class DocumentFile
    {
        [Column("Код_файла")]
        public string CodeFile { get; set; } = "";
        [Column("Код_документа")]
        public string CodeDocument { get; set; } = "";
        [Column("Код_типа_файла")]
        public string CodeTypeFile { get; set; } = "";
        [Column("Путь_файла")]
        public string FilePath { get; set; } = "";
        public FileType FileType { get; set; } = null!;
    }
}
