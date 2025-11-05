using HyCatTeamWPF.Helpers;
using HyCatTeamWPF.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace HyCatTeamWPF.ApiServices
{
    public class StationApiService
    {
        private readonly HttpClient _client;

        public StationApiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<StationViewRes>> GetAllStationsAsync()
        {
            ApiClient.AttachToken(); // attach Bearer token

            var response = await _client.GetAsync("/api/stations");

            if (!response.IsSuccessStatusCode)
                return new List<StationViewRes>();

            return await response.Content.ReadFromJsonAsync<List<StationViewRes>>();
        }
    }
}
