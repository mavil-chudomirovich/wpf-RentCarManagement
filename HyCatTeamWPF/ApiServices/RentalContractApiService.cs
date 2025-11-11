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
        public async Task<List<RentalContractViewRes>> GetMyContractsAsync()
        {
            try
            {
                var result = await _client.GetFromJsonAsync<List<RentalContractViewRes>>(
                    "/api/rental-contracts/me"
                );

                return result ?? new List<RentalContractViewRes>();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Cannot load contracts: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<RentalContractViewRes>();
            }
        }
        //Get All contract 
        public async Task<List<RentalContractViewRes>> GetAllContractsAsync()
        {
            try
            {
                var result = await _client.GetFromJsonAsync<List<RentalContractViewRes>>(
                    "/api/rental-contracts"
                );

                return result ?? new List<RentalContractViewRes>();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Cannot load contracts: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<RentalContractViewRes>();
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
                throw new Exception(apiError == null ? "fail to comfirm contract" : apiError.detail);
            }
            return result;

        }
        public async Task<HttpResponseMessage> CancleRentalContract(Guid id)
        {

            var result = await _client.PutAsync(
                $"/api/rental-contracts/{id}/cancel", null);

            if (!result.IsSuccessStatusCode)
            {
                //var apiError = await response.Content.ReadAsStringAsync();
                //throw new Exception(apiError);
                var apiError = JsonHelper.DeserializeJSON<ErrorMessage>(await result.Content.ReadAsStringAsync());
                throw new Exception(apiError == null ? "fail to cancel contract" : apiError.detail);
            }
            return result;

        }

        public async Task<RentalContractViewRes> GetContractById(Guid id)
        {            
            try
            {
                var result = await _client.GetFromJsonAsync<RentalContractViewRes>(
               $"/api/rental-contracts/{id}");

                return result ?? new RentalContractViewRes();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Cannot load contract: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new RentalContractViewRes();
            }

        }

        public async Task<HttpResponseMessage> UpdateRentalContractStatus(Guid id)
        {

            var result = await _client.PutAsync(
                $"/api/rental-contracts/{id}", null);

            if (!result.IsSuccessStatusCode)
            {
                var apiError = JsonHelper.DeserializeJSON<ErrorMessage>(await result.Content.ReadAsStringAsync());
                throw new Exception(apiError == null ? "fail to update contract" : apiError.detail);
            }
            return result;

        }

        public async Task<HttpResponseMessage> HandoverRentalContract(Guid id, HandoverContractReq req)
        {

            var result = await _client.PutAsJsonAsync(
                $"/api/rental-contracts/{id}/handover", req);

            if (!result.IsSuccessStatusCode)
            {
                var apiError = JsonHelper.DeserializeJSON<ErrorMessage>(await result.Content.ReadAsStringAsync());
                throw new Exception(apiError == null ? "fail to handover contract" : apiError.detail);
            }
            return result;

        }
    }
}
