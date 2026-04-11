using ServicesLibrary.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Globalization;

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
                "Ivanov",
                "Ivan",
                "Ivanovich",
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
                "ab", "Ivanov", "Ivan", "Ivanovich", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("at least 3 characters")));
        }

        // Логин с пробелами
        [TestMethod]
        public void ValidateRequest_LoginWithSpaces_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User 123", "Ivanov", "Ivan", "Ivanovich", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("must not contain spaces")));
        }

        // Логин без букв
        [TestMethod]
        public void ValidateRequest_LoginNoLetters_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "123456", "Ivanov", "Ivan", "Ivanovich", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("at least one letter")));
        }

        // Пароль слишком короткий
        [TestMethod]
        public void ValidateRequest_PasswordTooShort_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Ivanov", "Ivan", "Ivanovich", "P1!", "P1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("at least 5 characters")));
        }

        // Нет заглавной буквы
        [TestMethod]
        public void ValidateRequest_PasswordNoUppercase_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Ivanov", "Ivan", "Ivanovich", "password1!", "password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("uppercase letter")));
        }

        // Нет спецсимвола
        [TestMethod]
        public void ValidateRequest_PasswordNoSpecialChar_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Ivanov", "Ivan", "Ivanovich", "Password1", "Password1"
            );

            Assert.IsTrue(result.Any(e => e.Contains("special character")));
        }

        // Пароли не совпадают
        [TestMethod]
        public void ValidateRequest_PasswordMismatch_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Ivanov", "Ivan", "Ivanovich", "Password1!", "Password2!"
            );

            Assert.IsTrue(result.Contains("Passwords do not match."));
        }

        // Фамилия на латинице
        [TestMethod]
        public void ValidateRequest_LastNameLatin_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Ivanov", "Ivan", "Ivanovich", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("only letters")));
        }

        // Имя пустое
        [TestMethod]
        public void ValidateRequest_FirstNameEmpty_ReturnsError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Ivanov", "", "Ivanovich", "Password1!", "Password1!"
            );

            Assert.IsTrue(result.Any(e => e.Contains("First name is required")));
        }

        // Отчество необязательное
        [TestMethod]
        public void ValidateRequest_MiddleNameOptional_NoError()
        {
            var result = CreateService().ValidateRequest(
                "User123", "Ivanov", "Ivan", "", "Password1!", "Password1!"
            );

            Assert.IsFalse(result.Any(e => e.Contains("Middle name")));
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