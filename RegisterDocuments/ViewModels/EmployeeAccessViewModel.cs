namespace RegisterDocuments.ViewModels
{
    public class EmployeeAccessViewModel
    {
        public string Code { get; set; } = "";
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string RoleName { get; set; } = "";
        public bool Archived { get; set; } = false;
    }
}
