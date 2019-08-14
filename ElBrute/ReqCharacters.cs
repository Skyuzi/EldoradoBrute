using xNet;

namespace ElBrute
{
    class ReqCharacters
    {
        public CookieDictionary cookies { get; set; }
        public string proxy { get; set; }

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";


        public static HttpRequest Req()
        {
            HttpRequest request = new HttpRequest();
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
            request.IgnoreProtocolErrors = true;

            return request;
        }

        public static HttpRequest Req(CookieDictionary cookies)
        {
            HttpRequest request = new HttpRequest();
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
            request.Cookies = cookies;
            request.IgnoreProtocolErrors = true;


            return request;
        }

        public static HttpRequest Req(CookieDictionary cookies, string proxy)
        {
            HttpRequest request = new HttpRequest();
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
            request.Cookies = cookies;
            request.ConnectTimeout = 10000;
            request.Proxy = ProxyClient.Parse(ProxyType.Http, proxy);
            request.IgnoreProtocolErrors = true;


            return request;
        }
    }
}
