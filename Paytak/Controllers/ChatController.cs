using Microsoft.AspNetCore.Mvc;
using Paytak.Models.DTOs;
using System.Text.Json;

namespace Paytak.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly HttpClient _httpClient;

        public ChatController(ILogger<ChatController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            try
            {
                _logger.LogInformation("Chat message received: {Message}", request.Message);

                // Azure OpenAI API configuration (.env: AZURE_OPENAI_API_KEY, AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_DEPLOYMENT)
                var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
                var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
                var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
                var apiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2024-02-15-preview";
                
                _logger.LogInformation("Azure OpenAI Config - Key: {KeyLength}, Endpoint: {Endpoint}, Deployment: {Deployment}", 
                    apiKey?.Length ?? 0, endpoint, deployment);
                
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deployment))
                {
                    _logger.LogWarning("Azure OpenAI configuration not found. .env dosyasında AZURE_OPENAI_API_KEY, AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_DEPLOYMENT tanımlı olmalı.");
                    return Ok(new ChatResponseDto
                    {
                        Success = true,
                        Response = "Merhaba! Ben Paytak AI, akıllı belediye asistanınız. Şu anda demo modunda çalışıyorum. Azure OpenAI yapılandırması eksik. Size nasıl yardımcı olabilirim?"
                    });
                }

                // Prepare the request to Azure OpenAI
                var openAiRequest = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = @"Sen Paitak, akıllı belediye asistanısın. Türkçe konuşuyorsun ve belediye hizmetleri konusunda uzmanlaşmışsın. 
                            
                            ÖNEMLİ KURALLAR:
                            - Kendini sadece ilk tanışmada veya kullanıcı özellikle sorduğunda tanıt
                            - Kendini tanıtırken sadece 'Ben Paitak' de, 'Ben Sen Paitak' deme
                            - Her mesajda kendini tanıtma, bu gereksiz tekrar yaratır
                            - Doğal ve samimi bir şekilde sohbet et, sürekli kendini hatırlatma
                            
                            Ana Görevlerin:
                            - Vatandaşların belediye hizmetleri hakkındaki sorularını yanıtlamak
                            - Belediye süreçleri hakkında bilgi vermek
                            - Şikayet ve talep yönetimi konularında yardım etmek
                            - Belediye hizmetlerinin nasıl kullanılacağını açıklamak
                            - Genel belediye bilgilerini paylaşmak
                            
                            Günlük Sohbet:
                            - Belediye konuları dışında da doğal ve samimi bir şekilde sohbet edebilirsin
                            - Günlük konuşmalarda, selamlaşma, hava durumu, genel konular hakkında da yanıt verebilirsin
                            - Kullanıcı belediye konuları dışında bir şey sorarsa, önce doğal bir şekilde yanıtla, sonra belediye hizmetlerine yönlendirebilirsin
                            - Her zaman nazik, yardımsever ve samimi ol
                            
                            Yanıtların kısa, net ve yardımcı olmalı. Türkçe karakterleri doğru kullan. Sadece gerektiğinde kendini tanıt."
                        },
                        new
                        {
                            role = "user",
                            content = request.Message
                        }
                    },
                    max_completion_tokens = request.MaxTokens ?? 1000,
                    temperature = 1  // Bu model sadece varsayılan (1) değeri destekliyor
                };

                // Send request to Azure OpenAI
                var jsonContent = JsonSerializer.Serialize(openAiRequest);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

                var requestUrl = $"{endpoint.TrimEnd('/')}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";
                var response = await _httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Azure OpenAI response: {Response}", responseContent);
                    
                    // Manual JSON parsing
                    try
                    {
                        using var document = JsonDocument.Parse(responseContent);
                        var choices = document.RootElement.GetProperty("choices");
                        if (choices.GetArrayLength() > 0)
                        {
                            var firstChoice = choices[0];
                            var message = firstChoice.GetProperty("message");
                            var aiContent = message.GetProperty("content").GetString();
                            
                            _logger.LogInformation("Manually parsed content: {Content}", aiContent);
                            
                            return Ok(new ChatResponseDto
                            {
                                Success = true,
                                Response = aiContent ?? "Üzgünüm, şu anda yanıt veremiyorum."
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing JSON response");
                    }

                    return Ok(new ChatResponseDto
                    {
                        Success = true,
                        Response = "Üzgünüm, şu anda yanıt veremiyorum."
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Azure OpenAI API request failed with status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                    return Ok(new ChatResponseDto
                    {
                        Success = true,
                        Response = $"Üzgünüm, şu anda teknik bir sorun yaşıyorum. Hata: {response.StatusCode}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                return Ok(new ChatResponseDto
                {
                    Success = true,
                    Response = "Merhaba! Ben Paytak AI, akıllı belediye asistanınız. Şu anda demo modunda çalışıyorum. Size nasıl yardımcı olabilirim?"
                });
            }
        }
    }

    // Azure OpenAI API response models
    public class OpenAiResponse
    {
        public List<Choice> Choices { get; set; } = new();
        public string Id { get; set; } = string.Empty;
        public string Object { get; set; } = string.Empty;
        public long Created { get; set; }
        public string Model { get; set; } = string.Empty;
        public Usage Usage { get; set; } = new();
    }

    public class Choice
    {
        public Message Message { get; set; } = new();
        public string FinishReason { get; set; } = string.Empty;
        public int Index { get; set; }
        public ContentFilterResults ContentFilterResults { get; set; } = new();
    }

    public class Message
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<object> Annotations { get; set; } = new();
        public object? Refusal { get; set; }
    }

    public class ContentFilterResults
    {
        public FilterResult Hate { get; set; } = new();
        public FilterResult Sexual { get; set; } = new();
        public FilterResult Violence { get; set; } = new();
        public FilterResult SelfHarm { get; set; } = new();
        public FilterResult ProtectedMaterialCode { get; set; } = new();
        public FilterResult ProtectedMaterialText { get; set; } = new();
    }

    public class FilterResult
    {
        public bool Filtered { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool Detected { get; set; }
    }

    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
} 