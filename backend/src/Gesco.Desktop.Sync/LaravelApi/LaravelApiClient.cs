using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Sync.LaravelApi
{
    public class LaravelApiClient : ILaravelApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public LaravelApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = Environment.GetEnvironmentVariable("LARAVEL_API_URL") ?? "https://api.example.com";
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<LoginResultDto> LoginAsync(string usuario, string password)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(new { usuario, password }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LoginResultDto>(json);
                    
                    return result ?? new LoginResultDto
                    {
                        Success = false,
                        Message = "Error deserializing response"
                    };
                }

                return new LoginResultDto
                {
                    Success = false,
                    Message = "Error al conectar con el servidor"
                };
            }
            catch (Exception ex)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActivationResultDto> ActivateAsync(string codigo, int organizacionId)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(new { codigo, organizacionId }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_baseUrl}/license/activate", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ActivationResultDto>(json);
                    
                    return result ?? new ActivationResultDto
                    {
                        Success = false,
                        Message = "Error deserializing response"
                    };
                }

                return new ActivationResultDto
                {
                    Success = false,
                    Message = "Error al activar licencia"
                };
            }
            catch (Exception ex)
            {
                return new ActivationResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<bool> ValidateLicenseAsync(string codigo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/license/validate/{codigo}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}