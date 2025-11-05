using HyCatTeamWPF.Helpers;
using HyCatTeamWPF.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace HyCatTeamWPF.ApiServices
{
    public class RentalContractApiService
    {
        private readonly HttpClient _client;

        public RentalContractApiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> CreateRentalContractAsync(CreateRentalContractReq req)
        {
            var response = await _client.PostAsJsonAsync("/api/rental-contracts", req);

            if (!response.IsSuccessStatusCode)
            {
                //var apiError = await response.Content.ReadAsStringAsync();
                //throw new Exception(apiError);
                var apiError = JsonHelper.DeserializeJSON<ErrorMessage>(await response.Content.ReadAsStringAsync());
                throw new Exception(apiError == null ? "ngu" : apiError.detail);
            }
            return response;
        }
        //Lấy danh sách hợp đồng của user
        public async Task<List<MyContractViewRes>> GetMyContractsAsync()
        {
            try
            {
                var result = await _client.GetFromJsonAsync<List<MyContractViewRes>>(
                    "/api/rental-contracts/me"
                );

                return result ?? new List<MyContractViewRes>();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Cannot load contracts: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<MyContractViewRes>();
            }
        }
        //Get All contract 
        public async Task<List<MyContractViewRes>> GetAllContractsAsync()
        {
            try
            {
                var result = await _client.GetFromJsonAsync<List<MyContractViewRes>>(
                    "/api/rental-contracts"
                );

                return result ?? new List<MyContractViewRes>();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Cannot load contracts: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<MyContractViewRes>();
            }
        }

        public async Task<HttpResponseMessage> ConfirmContractReq(Guid id, ConfirmReq req)
        {
          
            var result = await _client.PutAsJsonAsync(
                $"/api/rental-contracts/{id}/confirm", req);

            if (!result.IsSuccessStatusCode)
            {
                //var apiError = await response.Content.ReadAsStringAsync();
                //throw new Exception(apiError);
                var apiError = JsonHelper.DeserializeJSON<ErrorMessage>(await result.Content.ReadAsStringAsync());
                throw new Exception(apiError == null ? "ngu" : apiError.detail);
            }
            return result;

        }
    }
}
