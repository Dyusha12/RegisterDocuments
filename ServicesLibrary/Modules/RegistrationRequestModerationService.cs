using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServicesLibrary.Models;
using System.Collections.Generic;
using System.Linq;

namespace ServicesLibrary.Modules
{
    public class RegistrationRequestModerationService
    {
        private readonly DbContext _context; // Контекст базы данных

        public RegistrationRequestModerationService(DbContext context)
        {
            _context = context;
        }

       
        // Одобрение заявки
        public void ApproveRequest(string requestCode)
        {
            // Получение подключения к базе данных
            var connection = _context.Database.GetDbConnection();

            // Открытие соединения, если оно закрыто
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            // Создание SQL-команды
            using var command = connection.CreateCommand();

            // Вызов хранимой процедуры PostgreSQL
            command.CommandText = "CALL approve_registration_request(@code)";
            command.Parameters.Add(new NpgsqlParameter("@code", requestCode));

            // Выполнение команды
            command.ExecuteNonQuery();
        }

        // Отклонение заявки
        public void RejectRequest(string requestCode)
        {
            // Получение подключения к базе данных
            var connection = _context.Database.GetDbConnection();

            // Открытие соединения, если оно закрыто
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            // Создание SQL-команды
            using var command = connection.CreateCommand();

            // Вызов хранимой процедуры PostgreSQL
            command.CommandText = "CALL reject_registration_request(@code)";
            command.Parameters.Add(new NpgsqlParameter("@code", requestCode));

            // Выполнение команды
            command.ExecuteNonQuery();
        }
    }
}