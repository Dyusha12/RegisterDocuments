using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServicesLibrary.Modules
{
    public class RegistrationRequestService
    {
        private readonly DbContext _context;

        public RegistrationRequestService(DbContext context)
        {
            _context = context;
        }

        // Производит валидацию заявки на регистрацию и возвращает список ошибок
        public List<string> ValidateRequest(
        string login,
        string lastname,
        string firstname,
        string middlename,
        string password,
        string confirmPassword)
        {
            var errors = new List<string>();

            // Проверка логина
            var loginError = ValidateLogin(login);
            if (loginError != null) errors.Add(loginError);

            // Проверка пароля
            var passwordError = ValidatePassword(password);
            if (passwordError != null) errors.Add(passwordError);

            // Проверка совпадения паролей
            if (password != confirmPassword)
                errors.Add("Пароли не совпадают.");

            // Проверка фамилии
            var lastNameError = ValidateLastName(lastname);
            if (lastNameError != null) errors.Add(lastNameError);

            // Проверка имени
            var firstNameError = ValidateFirstName(firstname);
            if (firstNameError != null) errors.Add(firstNameError);

            // Проверка отчества
            var middleNameError = ValidateMiddleName(middlename);
            if (middleNameError != null) errors.Add(middleNameError);

            return errors;
        }

        // Валидация поля логина
        private string? ValidateLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return "Логин обязателен.";

            login = login.Trim();

            if (login.Contains(" "))
                return "Логин не должен содержать пробелы.";

            if (login.Length < 3)
                return "Логин должен содержать минимум 3 символа.";

            if (login.Length > 15)
                return "Логин не должен превышать 15 символов.";

            if (!Regex.IsMatch(login, @"[A-Za-zА-Яа-я]"))
                return "Логин должен содержать хотя бы одну букву.";

            if (!Regex.IsMatch(login, @"^[A-Za-zА-Яа-я0-9-]+$"))
                return "Логин может содержать только буквы, цифры и знак '-'.";

            return null;
        }

        // Валидация поля пароль
        private string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Пароль обязателен.";

            password = password.Trim();

            if (password.Contains(" "))
                return "Пароль не должен содержать пробелы.";

            if (password.Length < 5)
                return "Пароль должен содержать минимум 5 символов.";

            if (password.Length > 20)
                return "Пароль должен содержать минимум 5 символов.";

            if (!Regex.IsMatch(password, @"[A-ZА-Я]"))
                return "Пароль должен содержать хотя бы одну заглавную букву.";

            string specialChars = @"!+_/><$#&()=|{}№?*%";

            if (!Regex.IsMatch(password, "[" + Regex.Escape(specialChars) + "]"))
                return $"Пароль должен содержать хотя бы один спецсимвол.";

            bool hasLetter = Regex.IsMatch(password, @"[A-Za-zА-Яа-я]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasSpecial = Regex.IsMatch(password, "[" + Regex.Escape(specialChars) + "]");

            int categories = (hasLetter ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);

            if (categories < 2)
                return "Пароль должен содержать минимум два типа символов.";

            return null;
        }

        // Валидация поля фамилии
        private string? ValidateLastName(string lastname)
        {
            if (string.IsNullOrWhiteSpace(lastname))
                return "Фамилия обязательна.";

            lastname = lastname.Trim();

            if (lastname.Contains(" "))
                return "Фамилия не должна содержать пробелы.";

            if (lastname.Length < 2)
                return "Фамилия должна содержать минимум 2 символа.";

            if (lastname.Length > 50)
                return "Фамилия не должна превышать 50 символов.";

            if (!Regex.IsMatch(lastname, @"^[А-Яа-я]+$"))
                return "Фамилия может содержать только русские буквы, без символов.";

            return null;
        }

        // Валидация поля имени
        private string? ValidateFirstName(string firstname)
        {
            if (string.IsNullOrWhiteSpace(firstname))
                return "Имя обязательно.";

            firstname = firstname.Trim();

            if (firstname.Contains(" "))
                return "Имя не должно содержать пробелы.";

            if (firstname.Length < 2)
                return "Имя должно содержать минимум 2 символа.";

            if (firstname.Length > 50)
                return "Имя не должно превышать 50 символов.";

            if (!Regex.IsMatch(firstname, @"^[А-Яа-я]+$"))
                return "Имя может содержать только русские буквы, без символов.";

            return null;
        }

        // Валидация поля отчества
        private string? ValidateMiddleName(string middlename)
        {
            if (string.IsNullOrWhiteSpace(middlename))
                return null;

            middlename = middlename.Trim();

            if (middlename.Contains(" "))
                return "Отчество не должно содержать пробелы.";

            if (middlename.Length < 2)
                return "Отчество должно содержать минимум 2 символа.";

            if (middlename.Length > 50)
                return "Отчество не должно превышать 50 символов.";

            if (!Regex.IsMatch(middlename, @"^[А-Яа-я]+$"))
                return "Отчество может содержать только русские буквы, без символов.";

            return null;
        }

        // Отправляет заявку на регистрацию в базу данных через хранимую процедуру
        public void SubmitRequest(
            string login,
            string lastname,
            string firstname,
            string middlename,
            string password
            )
        {
            // Получение подключения к базе данных
            var connection = _context.Database.GetDbConnection();

            // Открытие соединения при необходимости
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            var command = connection.CreateCommand();

            // Вызов хранимой процедуры для добавления заявки
            command.CommandText =
                "CALL add_registration_request(@login, @password, @lastname, @firstname, @middlename)";

            // Добавление параметров
            command.Parameters.Add(new NpgsqlParameter("@login", login));
            command.Parameters.Add(new NpgsqlParameter("@password", password));
            command.Parameters.Add(new NpgsqlParameter("@lastname", lastname));
            command.Parameters.Add(new NpgsqlParameter("@firstname", firstname));
            command.Parameters.Add(new NpgsqlParameter("@middlename",
                string.IsNullOrWhiteSpace(middlename) ? DBNull.Value : middlename));

            // Выполняем команду
            command.ExecuteNonQuery();
        }
    }
}
