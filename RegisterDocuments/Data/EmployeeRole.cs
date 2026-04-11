using System.ComponentModel.DataAnnotations.Schema;

namespace RegisterDocuments.Data
{
    public class EmployeeRole
    {
        [Column("Код_сотрудника")]
        public string EmployeeCode { get; set; } = null!;

        [Column("Код_роли")]
        public string RoleCode { get; set; } = null!;

        public Employee Employee { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
