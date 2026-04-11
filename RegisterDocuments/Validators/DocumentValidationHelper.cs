using System;
using System.Collections.Generic;

namespace RegisterDocuments
{
    public static class DocumentValidationHelper
    {
        public static List<string> Validate(string? name, string? typeCode, string? statusCode)
        {
            var errors = new List<string>();

            // Проверка обязательного поля
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Document name is required.");

            if (string.IsNullOrWhiteSpace(typeCode))
                errors.Add("Document type is not selected.");

            if (string.IsNullOrWhiteSpace(statusCode))
                errors.Add("Document status is not selected.");

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (name.Length < 3)
                    errors.Add("Document name must contain at least 3 characters.");

                if (name.Length > 200)
                    errors.Add("Document name must not exceed 200 characters.");
            }

            return errors;
        }
    }
}