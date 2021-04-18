using System.Text.RegularExpressions;

namespace WikiCodeParser.Models
{
    public class WikiRevision
    {
        public static string CreateSlug(string text)
        {
            text = text.Replace(' ', '_');
            text = Regex.Replace(text, @"[^-$_.+!*\'""(),:;<>^{}|~0-9a-z[\]]", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return text;
        }
    }
}
