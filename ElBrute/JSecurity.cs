using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;

namespace ElBrute
{
    class JSecurity
    {
        private static string AllowChars = "0123456789qwertyuiopasdfghjklzxcvbnm:?!";

        public static string GetCookie(ReqCharacters characters)
        {
            var doc = new HtmlDocument();

            var html = Request.Get(characters, "https://www.eldorado.ru");
            doc.LoadHtml(html);

            var node = doc.DocumentNode.Descendants("script").FirstOrDefault(x => x.InnerHtml.Contains("var cE = ") &&
                                                                                  x.InnerHtml.Contains("var cK = "));
            if (node == null)
                return null;

            string jScript = node.InnerHtml;

            var cERegex = new Regex("(?<=var cE = \").*(?=\";)");
            var cKRegex = new Regex("(?<=var cK = ).*(?=;)");

            string cE = cERegex.Match(jScript).Value;
            int cK = int.Parse(cKRegex.Match(jScript).Value);

            string cookie = EncodeCookie(cK, cE);

            return cookie;
        }

        private static string EncodeCookie(int number, string str)
        {
            string result = string.Empty;
            foreach (var c in str)
            {
                result += SupportFunction(number, c);
                if (++number > AllowChars.Length - 1)
                {
                    number = 0;
                }
            }
            return result;
        }

        private static char SupportFunction(int number, char character)
        {
            if (!AllowChars.Contains(character))
                return character;

            var charIndex = AllowChars.IndexOf(character) - number;

            if (charIndex > AllowChars.Length - 1)
            {
                charIndex -= AllowChars.Length - 2;
            }

            if (charIndex < 0)
            {
                charIndex += AllowChars.Length;
            }

            return AllowChars[charIndex];
        }
    }
}