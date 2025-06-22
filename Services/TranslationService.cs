using Newtonsoft.Json;
using System.Text;
using TransInputMethod.Models;

namespace TransInputMethod.Services
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigService _configService;

        public TranslationService(ConfigService configService)
        {
            _configService = configService;
            _httpClient = new HttpClient();
        }

        public async Task<TranslationResult> TranslateAsync(string text, string? scenario = null)
        {
            var config = await _configService.GetConfigAsync();
            
            if (string.IsNullOrEmpty(config.Api.ApiKey))
            {
                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = "API Key未配置"
                };
            }

            var selectedScenario = config.Scenarios.FirstOrDefault(s => s.Name == scenario) 
                ?? config.Scenarios.FirstOrDefault(s => s.IsDefault) 
                ?? config.Scenarios.FirstOrDefault();

            if (selectedScenario == null)
            {
                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = "未找到翻译场景配置"
                };
            }

            try
            {
                var request = new OpenAIRequest
                {
                    Model = config.Api.Model,
                    Messages = new List<OpenAIMessage>
                    {
                        new OpenAIMessage
                        {
                            Role = "system",
                            Content = selectedScenario.Prompt
                        },
                        new OpenAIMessage
                        {
                            Role = "user",
                            Content = text
                        }
                    },
                    MaxTokens = 2000,
                    Temperature = 0.3
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.Api.ApiKey}");
                _httpClient.Timeout = TimeSpan.FromSeconds(config.Api.Timeout);

                var baseUrl = config.Api.BaseUrl.TrimEnd('/');
                var response = await _httpClient.PostAsync($"{baseUrl}/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new TranslationResult
                    {
                        Success = false,
                        ErrorMessage = $"API调用失败: {response.StatusCode} - {errorContent}"
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);

                if (openAIResponse?.Choices?.Any() == true)
                {
                    var translatedText = openAIResponse.Choices[0].Message.Content.Trim();
                    
                    // Detect source and target languages (simple heuristic)
                    var sourceLanguage = DetectLanguage(text);
                    var targetLanguage = sourceLanguage == "Chinese" ? "English" : "Chinese";

                    return new TranslationResult
                    {
                        Success = true,
                        OriginalText = text,
                        TranslatedText = translatedText,
                        SourceLanguage = sourceLanguage,
                        TargetLanguage = targetLanguage,
                        Scenario = selectedScenario.Name
                    };
                }

                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = "API响应格式异常"
                };
            }
            catch (TaskCanceledException)
            {
                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = "请求超时"
                };
            }
            catch (Exception ex)
            {
                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = $"翻译失败: {ex.Message}"
                };
            }
        }

        private string DetectLanguage(string text)
        {
            // Simple language detection based on character ranges
            var chineseCharCount = text.Count(c => c >= 0x4E00 && c <= 0x9FFF);
            var totalChars = text.Length;
            
            return chineseCharCount > totalChars * 0.3 ? "Chinese" : "English";
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class TranslationResult
    {
        public bool Success { get; set; }
        public string OriginalText { get; set; } = string.Empty;
        public string TranslatedText { get; set; } = string.Empty;
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
        public string? Scenario { get; set; }
        public string? ErrorMessage { get; set; }
    }

    internal class OpenAIRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonProperty("messages")]
        public List<OpenAIMessage> Messages { get; set; } = new List<OpenAIMessage>();

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }
    }

    internal class OpenAIMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }

    internal class OpenAIResponse
    {
        [JsonProperty("choices")]
        public List<OpenAIChoice> Choices { get; set; } = new List<OpenAIChoice>();
    }

    internal class OpenAIChoice
    {
        [JsonProperty("message")]
        public OpenAIMessage Message { get; set; } = new OpenAIMessage();
    }
}