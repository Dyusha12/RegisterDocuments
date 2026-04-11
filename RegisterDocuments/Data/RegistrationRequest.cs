using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegisterDocuments.Data
{
    public class RegistrationRequest
    {
        [Column("Код_заявки")]
        public string CodeApplication { get; set; }

        [Column("Логин")]
        public string Login { get; set; }

        [Column("Пароль")]
        public string Password { get; set; }

        [Column ("Фамилия")]
        public string LastName { get; set; }

        [Column("Имя")]
        public string Name { get; set; }

        [Column("Отчество")]
        public string? MiddleName { get; set; }

        [Column("Статус")]
        public string Status { get; set; }

        [Column("Дата_создания")]
        public DateTime DateCreate { get; set; }
    }
}
