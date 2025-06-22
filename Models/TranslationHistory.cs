using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransInputMethod.Models
{
    [Table("history")]
    public class TranslationHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("source_text")]
        public string SourceText { get; set; } = string.Empty;

        [Required]
        [Column("translated_text")]
        public string TranslatedText { get; set; } = string.Empty;

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Column("source_language")]
        public string? SourceLanguage { get; set; }

        [Column("target_language")]
        public string? TargetLanguage { get; set; }

        [Column("translation_scenario")]
        public string? TranslationScenario { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}