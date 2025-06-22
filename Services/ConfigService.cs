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
                    Name = "IT 办公口语",
                    Prompt = "你是一名专业 IT 办公场景双语助手，请在中英之间自然口语化互译，风格贴合新西兰职场/会议/邮件表达。保留专有名词、产品名、命令行和格式不变，适当简化长句保证听说顺畅，仅输出译文，不加任何说明。",
                    IsDefault = true
                },
                new TranslationScenario
                {
                    Name = "正式书面",
                    Prompt = "你是一名专业书面翻译官，请在中英之间正式、精准互译，风格为正式公文/报告/合同，符合新西兰英语书面规范。避免口语化，保持逻辑严谨、措辞得体，技术术语与缩写沿用公认译法（如首现括注），仅输出译文，不加任何说明。",
                },
                new TranslationScenario
                {
                    Name = "IT 技术交流",
                    Prompt = "你是 IT 技术术语翻译专家，请在中英之间精准互译代码注释、技术博客、API 文档等内容，风格专业易读，术语符合业界惯例（IETF、RFC、MSDN）。保持代码块/Markdown/JSON 键值原样，专有名词与函数名不翻译（首现可括注），仅输出译文，不加任何说明。",
                },
                new TranslationScenario
                {
                    Name = "俗语·俚语互转",
                    Prompt = "你是中英俗语与俚语转换大师，请在中英之间转换，优先寻找语境最贴切的目标语言表达，风格生动地道避免直译。遇文化梗/成语/谚语，输出对应国家常用说法，必要时可加等效解释词，仅输出译文，不加任何说明。",
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
                    Model = "gpt-4.1-nano",
                    AvailableModels = new List<string> { "gpt-4.1-nano", "gpt-4o-mini" },
                    RequiresOrganizationId = false,
                    IsCustom = false
                },
                new ApiProvider
                {
                    Id = "deepseek",
                    Name = "DeepSeek",
                    BaseUrl = "https://api.deepseek.com/v1",
                    Model = "deepseek-chat",
                    AvailableModels = new List<string> { "deepseek-chat" },
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