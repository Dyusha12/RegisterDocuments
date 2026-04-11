using ServicesLibrary.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RegisterDocuments.Tests
{
    [TestClass]
    public class RegistrationRequestServiceTests
    {
        private RegistrationRequestService CreateService()
        {
            return new RegistrationRequestService(null);
        }

        // Валидные данные
        [TestMethod]
        public void ValidateRequest_ValidData_ReturnsNoErrors()
        {
            var service = CreateService();

            var result = service.ValidateRequest(
                "User123",
                "Иванов",
                "Иван",
                "Иванович",
                "Password1!",
                "Password1!"
            );

            Assert.AreEqual(0, result.Count);
        }

        // Логин слишком короткий
        [TestMethod]
        public void ValidateRequest_LoginTooShort_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "ab", "Иванов", "Иван", "Иванович", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("минимум 3 символа")));
        }

        // Логин с пробелами
        [TestMethod]
        public void ValidateRequest_LoginWithSpaces_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User 123", "Иванов", "Иван", "Иванович", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("пробелы")));
        }

        // Логин без букв
        [TestMethod]
        public void ValidateRequest_LoginNoLetters_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "123456", "Иванов", "Иван", "Иванович", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("хотя бы одну букву")));
        }

        // Пароль слишком короткий
        [TestMethod]
        public void ValidateRequest_PasswordTooShort_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Иванов", "Иван", "Иванович", "P1!", "P1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("минимум 5 символов")));
        }

        // Нет заглавной буквы
        [TestMethod]
        public void ValidateRequest_PasswordNoUppercase_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Иванов", "Иван", "Иванович", "password1!", "password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("заглавную букву")));
        }

        // Нет спецсимвола
        [TestMethod]
        public void ValidateRequest_PasswordNoSpecialChar_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Иванов", "Иван", "Иванович", "Password1", "Password1"
            );

            Assert.IsTrue(result.Any(e => e.Contains("спецсимвол")));
        }

        // Пароли не совпадают
        [TestMethod]
        public void ValidateRequest_PasswordMismatch_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Иванов", "Иван", "Иванович", "Password1!", "Password2!"
            );

            Assert.IsTrue(result.Contains("Пароли не совпадают."));
        }

        // Фамилия на латинице
        [TestMethod]
        public void ValidateRequest_LastNameLatin_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Ivanov", "Иван", "Иванович", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("русские буквы")));
        }

        // Имя пустое
        [TestMethod]
        public void ValidateRequest_FirstNameEmpty_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Иванов", "", "Иванович", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("Имя обязательно")));
        }

        // Отчество необязательное
        [TestMethod]
        public void ValidateRequest_MiddleNameOptional_NoError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Иванов", "Иван", "", "Password1!", "Password1!"
            );

            Assert.IsFalse(result.Any(e => e.Contains("Отчество")));
        }

        // Много ошибок
        [TestMethod]
        public void ValidateRequest_AllInvalid_ReturnsMultipleErrors()
        {
            var result = CreateService().ValidateRequest(
                "1", "", "", "", "123", "456"
            );

            Assert.IsTrue(result.Count > 3);
        }
    }
}