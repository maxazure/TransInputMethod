using Newtonsoft.Json;
using System.Text;
using TransInputMethod.Models;

namespace TransInputMethod.Services
{
    public class TranslationService : IDisposable
    {
        private readonly ConfigService _configService;
        private HttpClient? _httpClient;
        private AppConfig? _lastConfig;

        public TranslationService(ConfigService configService)
        {
            _configService = configService;
        }

        private void EnsureHttpClient(AppConfig config)
        {
            // Get current provider
            var currentProvider = GetCurrentProvider(config);
            
            // Check if we need to recreate the HttpClient due to config changes
            if (_httpClient == null || _lastConfig == null || !ConfigEquals(_lastConfig, config))
            {
                _httpClient?.Dispose();
                _httpClient = new HttpClient();
                
                // Set headers and timeout only once when creating the client
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {currentProvider.ApiKey}");
                
                // Add Organization ID header if provided
                if (!string.IsNullOrEmpty(currentProvider.OrganizationId))
                {
                    _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", currentProvider.OrganizationId);
                }
                
                // Add User-Agent header as recommended by OpenAI
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "TransInputMethod/1.0");
                
                _httpClient.Timeout = TimeSpan.FromSeconds(currentProvider.Timeout);
                
                _lastConfig = config;
            }
        }

        private ApiProvider GetCurrentProvider(AppConfig config)
        {
            var provider = config.ApiProviders?.FirstOrDefault(p => p.Id == config.CurrentProviderId);
            if (provider != null)
            {
                return provider;
            }

            // Fallback to legacy API settings
            return new ApiProvider
            {
                Id = "legacy",
                Name = "Legacy",
                BaseUrl = config.Api.BaseUrl,
                ApiKey = config.Api.ApiKey,
                OrganizationId = config.Api.OrganizationId,
                Model = config.Api.Model,
                Timeout = config.Api.Timeout
            };
        }

        private bool ConfigEquals(AppConfig config1, AppConfig config2)
        {
            var provider1 = GetCurrentProvider(config1);
            var provider2 = GetCurrentProvider(config2);
            
            return provider1.BaseUrl == provider2.BaseUrl &&
                   provider1.ApiKey == provider2.ApiKey &&
                   provider1.OrganizationId == provider2.OrganizationId &&
                   provider1.Timeout == provider2.Timeout;
        }

        public async Task<TranslationResult> TranslateAsync(string text, string? scenario = null)
        {
            var config = await _configService.GetConfigAsync();
            var currentProvider = GetCurrentProvider(config);
            
            if (string.IsNullOrEmpty(currentProvider.ApiKey))
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
                // Ensure HttpClient is properly configured
                EnsureHttpClient(config);

                var request = new OpenAIRequest
                {
                    Model = currentProvider.Model,
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

                var baseUrl = currentProvider.BaseUrl.TrimEnd('/');
                var response = await _httpClient!.PostAsync($"{baseUrl}/chat/completions", content);

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
            _httpClient = null;
            _lastConfig = null;
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