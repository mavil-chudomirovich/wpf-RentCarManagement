using HyCatTeamWPF.Helpers;
using HyCatTeamWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HyCatTeamWPF.ApiServices
{
    public class InvoiceApiService
    {
        private readonly HttpClient _client;

        public InvoiceApiService(HttpClient client)
        {
            _client = client;
        }
        public async Task<HttpResponseMessage> PaymentInvoice(Guid id, PaymentReq req)
        {

            var result = await _client.PutAsJsonAsync(
                $"/api/invoices/{id}/payment", req);

            if (!result.IsSuccessStatusCode)
            {
                var apiError = JsonHelper.DeserializeJSON<ErrorMessage>(await result.Content.ReadAsStringAsync());
                throw new Exception(apiError == null ? "fail to payment invoice" : apiError.detail);
            }
            return result;
        }
        public async Task<InvoiceViewRes> GetInvoiceById(Guid id)
        {
            try
            {
                var result = await _client.GetFromJsonAsync<InvoiceViewRes>(
               $"/api/invoices/{id}");

                return result ?? new InvoiceViewRes();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Cannot load invoice: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new InvoiceViewRes();
            }

        }
    }
}
