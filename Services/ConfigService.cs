using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using TransInputMethod.Models;

namespace TransInputMethod.Services
{
    public class ConfigService
    {
        private readonly string _configPath;
        private readonly string _keyPath;
        private AppConfig? _config;

        public ConfigService(string configPath = "config.json")
        {
            _configPath = configPath;
            _keyPath = Path.ChangeExtension(configPath, ".key");
        }

        public async Task<AppConfig> GetConfigAsync()
        {
            if (_config == null)
            {
                _config = await LoadConfigAsync();
            }
            return _config;
        }

        public async Task SaveConfigAsync(AppConfig config)
        {
            _config = config;
            await SaveConfigToFileAsync(config);
        }

        private async Task<AppConfig> LoadConfigAsync()
        {
            if (!File.Exists(_configPath))
            {
                return CreateDefaultConfig();
            }

            try
            {
                var encryptedContent = await File.ReadAllTextAsync(_configPath);
                var decryptedContent = DecryptString(encryptedContent);
                var config = JsonConvert.DeserializeObject<AppConfig>(decryptedContent);
                return config != null ? MigrateConfigIfNeeded(config) : CreateDefaultConfig();
            }
            catch
            {
                // If decryption fails, try to read as plain text (backward compatibility)
                try
                {
                    var content = await File.ReadAllTextAsync(_configPath);
                    var config = JsonConvert.DeserializeObject<AppConfig>(content);
                    if (config != null)
                    {
                        config = MigrateConfigIfNeeded(config);
                        // Re-save with encryption
                        await SaveConfigToFileAsync(config);
                        return config;
                    }
                }
                catch { }

                return CreateDefaultConfig();
            }
        }

        private async Task SaveConfigToFileAsync(AppConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            var encryptedContent = EncryptString(json);
            await File.WriteAllTextAsync(_configPath, encryptedContent);
        }

        private AppConfig CreateDefaultConfig()
        {
            var defaultScenarios = new List<TranslationScenario>
            {
                new TranslationScenario
                {
                    Name = "生活",
                    Prompt = "你是一个专业的翻译助手。请将用户输入的文本准确翻译。如果是中文，翻译成英文；如果是英文，翻译成中文。翻译应该自然流畅，符合日常对话的表达习惯。只返回翻译结果，不要添加任何解释。",
                    IsDefault = true
                },
                new TranslationScenario
                {
                    Name = "书面",
                    Prompt = "你是一个专业的翻译助手。请将用户输入的文本准确翻译。如果是中文，翻译成英文；如果是英文，翻译成中文。翻译应该正式、准确，适合书面语和正式场合。只返回翻译结果，不要添加任何解释。",
                },
                new TranslationScenario
                {
                    Name = "科技",
                    Prompt = "你是一个专业的翻译助手。请将用户输入的文本准确翻译。如果是中文，翻译成英文；如果是英文，翻译成中文。翻译应该准确使用科技和技术领域的专业术语。只返回翻译结果，不要添加任何解释。",
                }
            };

            var defaultProviders = CreateDefaultProviders();

            return new AppConfig
            {
                Scenarios = defaultScenarios,
                ApiProviders = defaultProviders,
                CurrentProviderId = "openai"
            };
        }

        private List<ApiProvider> CreateDefaultProviders()
        {
            return new List<ApiProvider>
            {
                new ApiProvider
                {
                    Id = "openai",
                    Name = "OpenAI",
                    BaseUrl = "https://api.openai.com/v1",
                    Model = "gpt-4o-mini",
                    AvailableModels = new List<string> { "gpt-4o", "gpt-4o-mini", "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" },
                    RequiresOrganizationId = false,
                    IsCustom = false
                },
                new ApiProvider
                {
                    Id = "deepseek",
                    Name = "DeepSeek",
                    BaseUrl = "https://api.deepseek.com/v1",
                    Model = "deepseek-chat",
                    AvailableModels = new List<string> { "deepseek-chat", "deepseek-coder" },
                    RequiresOrganizationId = false,
                    IsCustom = false
                },
                new ApiProvider
                {
                    Id = "qwen",
                    Name = "Qwen",
                    BaseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1",
                    Model = "qwen-turbo",
                    AvailableModels = new List<string> { "qwen-turbo", "qwen-plus", "qwen-max", "qwen-max-longcontext" },
                    RequiresOrganizationId = false,
                    IsCustom = false
                }
            };
        }

        private AppConfig MigrateConfigIfNeeded(AppConfig config)
        {
            // If no providers exist, migrate from old API settings or create defaults
            if (config.ApiProviders == null || !config.ApiProviders.Any())
            {
                config.ApiProviders = CreateDefaultProviders();
                
                // If old API settings exist, migrate them to current provider
                if (!string.IsNullOrEmpty(config.Api?.ApiKey))
                {
                    var currentProvider = config.ApiProviders.FirstOrDefault(p => p.Id == config.CurrentProviderId) 
                                        ?? config.ApiProviders.First();
                    
                    currentProvider.ApiKey = config.Api.ApiKey;
                    currentProvider.OrganizationId = config.Api.OrganizationId ?? string.Empty;
                    currentProvider.Model = config.Api.Model ?? currentProvider.Model;
                    currentProvider.Timeout = config.Api.Timeout;
                    currentProvider.BaseUrl = config.Api.BaseUrl ?? currentProvider.BaseUrl;
                }
            }

            // Ensure current provider ID is valid
            if (string.IsNullOrEmpty(config.CurrentProviderId) || 
                !config.ApiProviders.Any(p => p.Id == config.CurrentProviderId))
            {
                config.CurrentProviderId = config.ApiProviders.First().Id;
            }

            // Ensure default providers exist (add any missing ones)
            var defaultProviders = CreateDefaultProviders();
            foreach (var defaultProvider in defaultProviders)
            {
                if (!config.ApiProviders.Any(p => p.Id == defaultProvider.Id))
                {
                    config.ApiProviders.Add(defaultProvider);
                }
            }

            return config;
        }

        private string EncryptString(string plainText)
        {
            var key = GetOrCreateKey();
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            var iv = aes.IV;
            var encrypted = msEncrypt.ToArray();
            var result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

            return Convert.ToBase64String(result);
        }

        private string DecryptString(string cipherText)
        {
            var key = GetOrCreateKey();
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = key;

            var iv = new byte[aes.IV.Length];
            var encrypted = new byte[buffer.Length - iv.Length];

            Buffer.BlockCopy(buffer, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(buffer, iv.Length, encrypted, 0, encrypted.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(encrypted);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }

        private byte[] GetOrCreateKey()
        {
            if (File.Exists(_keyPath))
            {
                var keyData = File.ReadAllBytes(_keyPath);
                if (keyData.Length == 32) // 256 bits
                {
                    return keyData;
                }
            }

            // Generate new key
            using var aes = Aes.Create();
            aes.GenerateKey();
            File.WriteAllBytes(_keyPath, aes.Key);
            
            // Hide the key file
            try
            {
                File.SetAttributes(_keyPath, FileAttributes.Hidden);
            }
            catch { } // Ignore if unable to hide

            return aes.Key;
        }
    }
}