using System.Text.RegularExpressions;

namespace RegisterDocuments
{
    public static class StatusValidationHelper
    {
        public static List<string> Validate(string? statusName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(statusName))
            {
                errors.Add("Status name is required.");
                return errors;
            }

            statusName = statusName.Trim();

            int letterCount = statusName.Count(char.IsLetter);

            if (letterCount < 3)
                errors.Add("Status name must contain at least 3 letters.");

            if (statusName.Length > 50)
                errors.Add("Status name must not exceed 50 characters.");

            if (!Regex.IsMatch(statusName, @"^[A-Za-zА-Яа-я\s-]+$"))
                errors.Add("Only letters, spaces and hyphens are allowed.");

            return errors;
        }
    }
}