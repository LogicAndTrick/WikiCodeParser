using System.Linq;

namespace WikiCodeParser.Tags
{
    public static class TagExtensions
    {
        public static Tag WithScopes(this Tag tag, params string[] scopes)
        {
            tag.Scopes = scopes.ToList();
            return tag;
        }

        public static Tag WithToken(this Tag tag, string token)
        {
            tag.Token = token;
            return tag;
        }

        public static Tag WithElement(this Tag tag, string element)
        {
            tag.Element = element;
            return tag;
        }

        public static Tag WithElementClass(this Tag tag, string elementClass)
        {
            tag.ElementClass = elementClass;
            return tag;
        }
    }
}