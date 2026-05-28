using System.Text.RegularExpressions;

namespace RPA_PHARMA.Helpers
{
    public static class DocumentHelper
    {
        public static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return Regex.Replace(input, @"[^\d]", "");
        }

        public static string PadLeadingZeros(string input, int length)
        {
            if (string.IsNullOrWhiteSpace(input)) return new string('0', length);
            string numbers = Sanitize(input);
            return numbers.PadLeft(length, '0');
        }
    }
}