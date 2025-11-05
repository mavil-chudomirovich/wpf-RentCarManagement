using HyCatTeamWPF.Models;
using System;
using System.IO;
using System.Text.Json;

namespace HyCatTeamWPF.Helpers
{
    public static class TokenStorage
    {
        private static readonly string TokenFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tokens.json");

        public static void SaveAccessToken(string accessToken)
        {
            var data = new TokenData
            {
                AccessToken = accessToken,
                SavedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(TokenFilePath, json);
        }

        public static TokenData? LoadAccessToken()
        {
            if (!File.Exists(TokenFilePath))
                return null;

            var json = File.ReadAllText(TokenFilePath);
            return JsonSerializer.Deserialize<TokenData>(json);
        }

        public static void ClearToken()
        {
            if (File.Exists(TokenFilePath))
                File.Delete(TokenFilePath);
        }

        public static bool HasToken()
        {
            var t = LoadAccessToken();
            return t != null && !string.IsNullOrEmpty(t.AccessToken);
        }

        
    }
}
