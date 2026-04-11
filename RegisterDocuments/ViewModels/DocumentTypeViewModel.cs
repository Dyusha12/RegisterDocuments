namespace RegisterDocuments.ViewModels
{
    public class DocumentTypeViewModel
    {
        public string TypeCode { get; set; } = "";
        public string TypeName { get; set; } = "";
        public List<DocumentViewModel> Documents { get; set; } = new();
    }
}
