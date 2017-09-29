using System.Text.RegularExpressions;

namespace Enferno.Public.Utils
{
    public static class Validation
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            const string patternLenient = @"^[a-z0-9]*([a-z0-9-+\._]+)*@[a-z0-9-]+([-\.][a-z0-9]+)*\.[a-z0-9]+([-\.][a-z0-9]+)*$";
            var regexEmail = new Regex(patternLenient, RegexOptions.IgnoreCase);
            return regexEmail.IsMatch(email.Trim());
        }
    }
}
