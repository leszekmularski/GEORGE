using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using GEORGE.Shared.Models;
using System.Net.Http.Json;

namespace GEORGE.Client.Pages.Mail
{
    public class EncryptionService
    {
        private readonly HttpClient _httpClient;

        public EncryptionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Metoda pobiera hasło SQL dla pracownika na podstawie ID
        public async Task<byte[]> GetEncryptionKeyForPracownikAsync(string rowIdPracownika)
        {
            // Pobieranie hasła SQL z API serwera
            var hasloSQL = await _httpClient.GetFromJsonAsync<string>($"api/pracownicy/{rowIdPracownika}");

            // Sprawdzenie, czy hasło zostało znalezione
            if (hasloSQL == null)
                throw new InvalidOperationException("HasloSQL not found for the given pracownik.");

            // Generowanie klucza szyfrującego na podstawie hasła SQL
            return GenerateEncryptionKey(hasloSQL);
        }

        // Metoda generująca klucz szyfrujący na podstawie hasła SQL
        private static byte[] GenerateEncryptionKey(string hasloSQL)
        {
            var combinedKey = hasloSQL + "123"; // Łączenie hasła z dodatkowymi danymi
            return Encoding.UTF8.GetBytes(combinedKey.PadRight(32).Substring(0, 32)); // Generowanie klucza o stałej długości 32 bajtów
        }
    }
}
