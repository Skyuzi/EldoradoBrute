using System.Threading;
using xNet;

namespace ElBrute
{
    class Captcha
    {
        private static string key = "KEY";

        public static string GetID(string sitekey)
        {
            HttpRequest danni = ReqCharacters.Req();
            string url = "https://www.eldorado.ru/";
            string response = danni.Get("http://rucaptcha.com/in.php?key=" + key + "&method=userrecaptcha&googlekey=" + sitekey + "&pageurl=" + url).ToString();

            string result;
            if (response.Contains("OK"))
            {
                response = response.Replace("OK|", "");
                result = response;
            }
            else
                result = "error";

            return result;
        }
        public static string GetAnswer(string idKey)
        {
            Thread.Sleep(60000);
            HttpRequest danni = ReqCharacters.Req();
            string response = danni.Get("http://rucaptcha.com/res.php?key=" + key + "&action=get&id=" + idKey).ToString();

            if (response.Contains("OK"))
                response = response.Replace("OK|", "");

            return response;
        }
    }
}
