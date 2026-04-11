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
                errors.Add("Passwords do not match.");

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
                return "Login is required.";

            login = login.Trim();

            if (login.Contains(" "))
                return "Login must not contain spaces.";

            if (login.Length < 3)
                return "Login must be at least 3 characters long.";

            if (login.Length > 15)
                return "Login must not exceed 15 characters.";

            if (!Regex.IsMatch(login, @"[A-Za-zА-Яа-я]"))
                return "Login must contain at least one letter.";

            if (!Regex.IsMatch(login, @"^[A-Za-zА-Яа-я0-9-]+$"))
                return "Login can contain only letters, digits, and '-'.";

            return null;
        }

        // Валидация поля пароль
        private string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password is required.";

            password = password.Trim();

            if (password.Contains(" "))
                return "Password must not contain spaces.";

            if (password.Length < 5)
                return "Password must be at least 5 characters long.";

            if (password.Length > 20)
                return "Password must not exceed 20 characters.";

            if (!Regex.IsMatch(password, @"[A-ZА-Я]"))
                return "Password must contain at least one uppercase letter.";

            string specialChars = @"!+_/><$#&()=|{}№?*%";

            if (!Regex.IsMatch(password, "[" + Regex.Escape(specialChars) + "]"))
                return $"Password must contain at least one special character.";

            bool hasLetter = Regex.IsMatch(password, @"[A-Za-zА-Яа-я]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasSpecial = Regex.IsMatch(password, "[" + Regex.Escape(specialChars) + "]");

            int categories = (hasLetter ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);

            if (categories < 2)
                return "Password must contain at least two types of characters.";

            return null;
        }

        // Валидация поля фамилии
        private string? ValidateLastName(string lastname)
        {
            if (string.IsNullOrWhiteSpace(lastname))
                return "Last name is required.";

            lastname = lastname.Trim();

            if (lastname.Contains(" "))
                return "Last name must not contain spaces.";

            if (lastname.Length < 2)
                return "Last name must be at least 2 characters long.";

            if (lastname.Length > 50)
                return "Last name must not exceed 50 characters.";

            if (!Regex.IsMatch(lastname, @"^[A-Za-zА-Яа-я]+$"))
                return "Last name can contain only letters.";

            return null;
        }

        // Валидация поля имени
        private string? ValidateFirstName(string firstname)
        {
            if (string.IsNullOrWhiteSpace(firstname))
                return "First name is required.";

            firstname = firstname.Trim();

            if (firstname.Contains(" "))
                return "First name must not contain spaces.";

            if (firstname.Length < 2)
                return "First name must be at least 2 characters long.";

            if (firstname.Length > 50)
                return "First name must not exceed 50 characters.";

            if (!Regex.IsMatch(firstname, @"^[A-Za-zА-Яа-я]+$"))
                return "First name can contain only letters.";

            return null;
        }

        // Валидация поля отчества
        private string? ValidateMiddleName(string middlename)
        {
            if (string.IsNullOrWhiteSpace(middlename))
                return null;

            middlename = middlename.Trim();

            if (middlename.Contains(" "))
                return "Middle name must not contain spaces.";

            if (middlename.Length < 2)
                return "Middle name must be at least 2 characters long.";

            if (middlename.Length > 50)
                return "Middle name must not exceed 50 characters.";

            if (!Regex.IsMatch(middlename, @"^[A-Za-zА-Яа-я]+$"))
                return "Middle name can contain only letters.";

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
