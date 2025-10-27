using Paytak.Models.DTOs;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Paytak.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<ChatResponseDto> GetChatResponseAsync(ChatRequestDto request)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
                var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
                var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deployment))
                {
                    throw new Exception("Azure OpenAI yapılandırması eksik. Lütfen .env dosyasını kontrol edin.");
                }

                var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deployment}/chat/completions?api-version=2024-02-15-preview";

                var requestBody = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = @"Sen Paytak AI, belediyeler için özel olarak tasarlanmış kurumsal yapay zeka asistanısın. 
                            Görevlerin:
                            - Belediye hizmetleri hakkında bilgi vermek
                            - Raporlama konularında yardım etmek
                            - Vatandaş taleplerini anlamak ve yönlendirmek
                            - Belediye süreçleri hakkında açıklama yapmak
                            
                            Yanıtların:
                            - Profesyonel ve kurumsal olmalı
                            - Türkçe dilinde olmalı
                            - Belediye terminolojisini kullanmalı
                            - Kısa ve öz olmalı
                            - Yardımcı ve bilgilendirici olmalı"
                        },
                        new
                        {
                            role = "user",
                            content = request.Message
                        }
                    },
                    max_tokens = request.MaxTokens ?? 1000,
                    temperature = request.Temperature ?? 0.7,
                    top_p = 0.95,
                    frequency_penalty = 0,
                    presence_penalty = 0
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var choices = responseJson.GetProperty("choices");
                    var firstChoice = choices[0];
                    var message = firstChoice.GetProperty("message");
                    var responseText = message.GetProperty("content").GetString();

                    var usage = responseJson.GetProperty("usage");
                    var totalTokens = usage.GetProperty("total_tokens").GetInt32();

                    return new ChatResponseDto
                    {
                        Response = responseText,
                        Model = request.Model ?? "gpt-4o",
                        TokensUsed = totalTokens,
                        CreatedAt = DateTime.UtcNow,
                        Success = true
                    };
                }
                else
                {
                    return new ChatResponseDto
                    {
                        Success = false,
                        ErrorMessage = $"API Hatası: {response.StatusCode} - {responseContent}",
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                return new ChatResponseDto
                {
                    Success = false,
                    ErrorMessage = $"Hata: {ex.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
        }
    }
} 