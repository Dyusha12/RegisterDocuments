using System.ComponentModel.DataAnnotations.Schema;

namespace RegisterDocuments.Data
{
    public class Employee
    {
        [Column("Код_сотрудника")]
        public string EmployeeCode { get; set; } = null!;

        [Column("Логин")]
        public string Login { get; set; } = null!;

        [Column("Пароль")]
        public string Password { get; set; } = null!;

        [Column("Фамилия")]
        public string LastName { get; set; } = null!;

        [Column("Имя")]
        public string FirstName { get; set; } = null!;

        [Column("Отчество")]
        public string? MiddleName { get; set; }

        [Column("Архивный")]
        public bool Archived { get; set; }

        public List<EmployeeRole> EmployeeRoles { get; set; } = new();
    }
}