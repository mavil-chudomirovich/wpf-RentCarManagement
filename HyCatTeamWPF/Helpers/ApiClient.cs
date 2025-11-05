using HyCatTeamWPF.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HyCatTeamWPF.Helpers
{
    public static class ApiClient
    {
        public static HttpClient Client { get; private set; }

        static ApiClient()
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5160")
            };
        }

        public static void AttachToken()
        {
            var token = TokenStorage.LoadAccessToken();
            if (token == null) return;

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }
    }
}
