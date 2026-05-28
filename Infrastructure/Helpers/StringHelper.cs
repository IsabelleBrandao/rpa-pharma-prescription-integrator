using System;
using System.Globalization;
using System.Text;

namespace RPA_PHARMA.Helpers
{
    public static class StringHelper
    {
        public static string NormalizeForUi(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            text = text.Trim();

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .ToUpper();
        }
    }
}