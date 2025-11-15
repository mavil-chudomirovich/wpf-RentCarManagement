using HyCatTeamWPF.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace HyCatTeamWPF.ApiServices
{
    public class AuthApiService
    {
        private readonly HttpClient _client;

        public AuthApiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string?> LoginAsync(UserLoginReq req)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", req);

            if (!response.IsSuccessStatusCode)
                return null;

            var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return data.AccessToken;
            //I write this comment to save my fire
        }
    }
}
