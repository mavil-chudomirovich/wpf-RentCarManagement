using HyCatTeamWPF.Helpers;
using HyCatTeamWPF.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace HyCatTeamWPF.ApiServices
{
    public class UserApiService
    {
        private readonly HttpClient _client;

        public UserApiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<UserProfileViewRes?> GetMeAsync()
        {
            ApiClient.AttachToken(); // Add Bearer token

            var res = await _client.GetAsync("/api/me");
            if (!res.IsSuccessStatusCode)
                return null;

            return await res.Content.ReadFromJsonAsync<UserProfileViewRes>();
        }
    }
}
