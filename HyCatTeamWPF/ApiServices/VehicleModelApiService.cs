using HyCatTeamWPF.Helpers;
using HyCatTeamWPF.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace HyCatTeamWPF.ApiServices;

public class VehicleApiService
{
    private readonly HttpClient _client;

    public VehicleApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<VehicleModelViewRes>> SearchVehiclesAsync(Guid stationId, DateTimeOffset start, DateTimeOffset end)
    {
        string startStr = start.ToString("yyyy-MM-ddTHH:mm:ss");
        string endStr = end.ToString("yyyy-MM-ddTHH:mm:ss");
        var url = $"/api/vehicle-models/search?StationId={stationId}&StartDate={startStr}&EndDate={endStr}";
        //MessageBox.Show(url);
        return await _client.GetFromJsonAsync<List<VehicleModelViewRes>>(url)
               ?? new List<VehicleModelViewRes>();
    }

}
