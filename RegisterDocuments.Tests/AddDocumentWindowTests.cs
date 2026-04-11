using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegisterDocuments;
using System.Linq;

namespace RegisterDocuments.Tests
{
    [TestClass]
    public class AddDocumentWindowTests
    {
        // Пустое название документа
        [TestMethod]
        public void Validate_EmptyName_ReturnsError()
        {
            var result = DocumentValidationHelper.Validate("", "1", "1");

            Assert.IsTrue(result.Any(e => e.Contains("required")));
        }

        // Null вместо названия документа
        [TestMethod]
        public void Validate_NullName_ReturnsError()
        {
            var result = DocumentValidationHelper.Validate(null, "1", "1");

            Assert.IsTrue(result.Any(e => e.Contains("required")));
        }

        // Не выбран тип документа
        [TestMethod]
        public void Validate_MissingType_ReturnsError()
        {
            var result = DocumentValidationHelper.Validate("Document", "", "1");

            Assert.IsTrue(result.Any(e => e.Contains("type")));
        }

        // Не выбран статус документа
        [TestMethod]
        public void Validate_MissingStatus_ReturnsError()
        {
            var result = DocumentValidationHelper.Validate("Document", "1", "");

            Assert.IsTrue(result.Any(e => e.Contains("status")));
        }

        // Слишком короткое название (меньше 3 символов)
        [TestMethod]
        public void Validate_NameTooShort_ReturnsError()
        {
            var result = DocumentValidationHelper.Validate("ab", "1", "1");

            Assert.IsTrue(result.Any(e => e.Contains("at least 3")));
        }

        // Слишком длинное название (больше 200 символов)
        [TestMethod]
        public void Validate_NameTooLong_ReturnsError()
        {
            var longName = new string('A', 250);

            var result = DocumentValidationHelper.Validate(longName, "1", "1");

            Assert.IsTrue(result.Any(e => e.Contains("200")));
        }

        // Корректные данные
        [TestMethod]
        public void Validate_ValidData_ReturnsNoErrors()
        {
            var result = DocumentValidationHelper.Validate("Valid Document", "1", "1");

            Assert.AreEqual(0, result.Count);
        }

        // Минимальная граница (3 символа)
        [TestMethod]
        public void Validate_MinLengthBoundary_ReturnsNoErrors()
        {
            var result = DocumentValidationHelper.Validate("Doc", "1", "1");

            Assert.AreEqual(0, result.Count);
        }

        // Максимальная граница (200 символов)
        [TestMethod]
        public void Validate_MaxLengthBoundary_ReturnsNoErrors()
        {
            var name = new string('A', 200);

            var result = DocumentValidationHelper.Validate(name, "1", "1");

            Assert.AreEqual(0, result.Count);
        }
    }
}