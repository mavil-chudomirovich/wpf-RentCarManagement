using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Helpers
{
    public class JsonHelper
    {
        public static T? DeserializeJSON<T>(string? valueJson) where T : class
        {
            if (string.IsNullOrEmpty(valueJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(valueJson);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
