using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

public class User
{
    public int id { get; set; }
    public string username { get; set; } = "";
    public string email { get; set; } = "";
}

namespace frontend
{
    public class AuthClient
    {
        private static HttpClient m_httpClient => BaseClient.Instance.HttpClient;

        public static async Task<bool> Register(string username, string email, string password)
        {
            var request = new { Username = username, Email = email, Password = password };
            var response = await m_httpClient.PostAsJsonAsync("/api/auth/register", request);
            return response.IsSuccessStatusCode;
        }

        public static async Task<User?> Login(string email, string password)
        {
            var request = new { Email = email, Password = password };
            var response = await m_httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<User>();
        }
    }
}
