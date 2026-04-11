using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegisterDocuments;
using System.Linq;

namespace RegisterDocuments.Tests
{
    [TestClass]
    public class AddDocumentStatusWindowTests
    {
        // Пустое значение
        [TestMethod]

        public void ValidateStatusName_Empty_ReturnsRequiredError()
        {
            var result = StatusValidationHelper.Validate("");

            Assert.IsTrue(result.Any(e => e.Contains("required")));
        }

        // Null значение
        [TestMethod]
        public void ValidateStatusName_Null_ReturnsRequiredError()
        {
            var result = StatusValidationHelper.Validate(null);

            Assert.IsTrue(result.Any(e => e.Contains("required")));
        }

        // Меньше 3 букв
        [TestMethod]
        public void ValidateStatusName_LessThan3Letters_ReturnsError()
        {
            var result = StatusValidationHelper.Validate("AB");

            Assert.IsTrue(result.Any(e => e.Contains("at least 3 letters")));
        }

        // Слишком длинное название
        [TestMethod]
        public void ValidateStatusName_TooLong_ReturnsError()
        {
            var longName = new string('A', 60);

            var result = StatusValidationHelper.Validate(longName);

            Assert.IsTrue(result.Any(e => e.Contains("must not exceed")));
        }

        // Недопустимые символы
        [TestMethod]
        public void ValidateStatusName_InvalidCharacters_ReturnsError()
        {
            var result = StatusValidationHelper.Validate("Status@123");

            Assert.IsTrue(result.Any(e => e.Contains("letters, spaces and hyphens")));
        }

        // Валидное значение
        [TestMethod]
        public void ValidateStatusName_Valid_ReturnsNoErrors()
        {
            var result = StatusValidationHelper.Validate("In Progress");

            Assert.AreEqual(0, result.Count);
        }

        
        // Дефис допустим
        [TestMethod]
        public void ValidateStatusName_WithHyphen_ReturnsNoErrors()
        {
            var result = StatusValidationHelper.Validate("In-Progress");

            Assert.AreEqual(0, result.Count);
        }
    }
}