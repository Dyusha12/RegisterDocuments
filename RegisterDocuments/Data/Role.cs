using System.ComponentModel.DataAnnotations.Schema;

namespace RegisterDocuments.Data
{
    public class Role
    {
        [Column("Код_роли")]
        public string RoleCode { get; set; } = null!;

        [Column("Название_роли")]
        public string RoleName { get; set; } = null!;

        [Column("Описание")]
        public string? Description { get; set; }

        public List<EmployeeRole> EmployeeRoles { get; set; } = new();
    }
}
