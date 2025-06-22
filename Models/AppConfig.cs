using System.ComponentModel;

namespace TransInputMethod.Models
{
    public class AppConfig
    {
        public HotkeySettings Hotkeys { get; set; } = new HotkeySettings();
        public ApiSettings Api { get; set; } = new ApiSettings();
        public List<ApiProvider> ApiProviders { get; set; } = new List<ApiProvider>();
        public string CurrentProviderId { get; set; } = "openai";
        public List<TranslationScenario> Scenarios { get; set; } = new List<TranslationScenario>();
        public UiSettings Ui { get; set; } = new UiSettings();
    }

    public class HotkeySettings
    {
        public string ShowWindowHotkey { get; set; } = "Shift+Space";
        public string TranslateHotkey { get; set; } = "Ctrl+Enter";
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        public string ApiKey { get; set; } = string.Empty;
        public string OrganizationId { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini";
        public int Timeout { get; set; } = 30; // seconds
    }

    public class ApiProvider
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string OrganizationId { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Timeout { get; set; } = 30;
        public List<string> AvailableModels { get; set; } = new List<string>();
        public bool RequiresOrganizationId { get; set; } = false;
        public bool IsCustom { get; set; } = false;
    }

    public class TranslationScenario
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
    }

    public class UiSettings
    {
        public int WindowWidth { get; set; } = 600;
        public int WindowOpacity { get; set; } = 95; // percentage
        public bool DarkMode { get; set; } = false;
        public int HistoryPageSize { get; set; } = 10;
    }

    public enum SupportedLanguage
    {
        [Description("自动检测")]
        Auto,
        [Description("中文")]
        Chinese,
        [Description("英文")]
        English
    }
}