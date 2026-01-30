using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace frontend
{
    public class BaseClient
    {
        private static BaseClient? _instance;

        public readonly HttpClient HttpClient;

        private BaseClient(string baseUrl)
        {
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public static void Initialize(string baseUrl)
        {
            if (_instance == null)
                _instance = new BaseClient(baseUrl);
        }

        public static BaseClient Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("BaseClient not initialized. Call BaseClient.Initialize(baseUrl) first.");
                return _instance;
            }
        }
    }
}
