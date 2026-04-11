namespace RegisterDocuments.ViewModels
{
    public class DocumentViewModel
    {
        public string CodeDocument { get; set; }
        public string DocumentName { get; set; } = "";
        public string TypeDocumentCode { get; set; } = "";
        public DateTime? SendDate { get; set; }
        public bool IsSent { get; set; }
        public string StatusName { get; set; } = "";
        public DateTime? ApprovalDate { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public string SentFileName { get; set; } = "";
        public string ReceivedFileName { get; set; } = "";
        public string Note { get; set; } = "";

        public string SentFileIcon => string.IsNullOrWhiteSpace(SentFileName) ? "pack://application:,,,/Images/ic_fileDownloads.png" : "pack://application:,,,/Images/ic_viewFile.png";
        public string ReceivedFileIcon => string.IsNullOrWhiteSpace(ReceivedFileName) ? "pack://application:,,,/Images/ic_fileDownloads.png" : "pack://application:,,,/Images/ic_viewFile.png";
    }
}