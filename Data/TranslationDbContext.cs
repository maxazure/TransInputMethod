using Microsoft.Data.Sqlite;
using System.Data;
using TransInputMethod.Models;

namespace TransInputMethod.Data
{
    public class TranslationDbContext : IDisposable
    {
        private readonly string _connectionString;
        private SqliteConnection? _connection;

        public Trans​lationDbContext(string databasePath = "translations.db")
        {
            _connectionString = $"Data Source={databasePath}";
            InitializeDatabase();
        }

        private SqliteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = """
                CREATE TABLE IF NOT EXISTS history (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    source_text TEXT NOT NULL,
                    translated_text TEXT NOT NULL,
                    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    source_language TEXT,
                    target_language TEXT,
                    translation_scenario TEXT
                );

                CREATE INDEX IF NOT EXISTS idx_history_timestamp ON history(timestamp DESC);
                CREATE INDEX IF NOT EXISTS idx_history_source_text ON history(source_text);
                """;
            createTableCommand.ExecuteNonQuery();
        }

        public async Task<int> AddTranslationAsync(TranslationHistory translation)
        {
            var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO history (source_text, translated_text, timestamp, source_language, target_language, translation_scenario)
                VALUES (@sourceText, @translatedText, @timestamp, @sourceLanguage, @targetLanguage, @scenario);
                SELECT last_insert_rowid();
                """;

            command.Parameters.AddWithValue("@sourceText", translation.SourceText);
            command.Parameters.AddWithValue("@translatedText", translation.TranslatedText);
            command.Parameters.AddWithValue("@timestamp", translation.Timestamp);
            command.Parameters.AddWithValue("@sourceLanguage", translation.SourceLanguage ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@targetLanguage", translation.TargetLanguage ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@scenario", translation.TranslationScenario ?? (object)DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<PagedResult<TranslationHistory>> GetTranslationHistoryAsync(int page = 1, int pageSize = 10, string? searchText = null)
        {
            var connection = GetConnection();
            
            // Count total records
            using var countCommand = connection.CreateCommand();
            if (string.IsNullOrEmpty(searchText))
            {
                countCommand.CommandText = "SELECT COUNT(*) FROM history";
            }
            else
            {
                countCommand.CommandText = "SELECT COUNT(*) FROM history WHERE source_text LIKE @search OR translated_text LIKE @search";
                countCommand.Parameters.AddWithValue("@search", $"%{searchText}%");
            }

            var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            // Get paged data
            using var dataCommand = connection.CreateCommand();
            if (string.IsNullOrEmpty(searchText))
            {
                dataCommand.CommandText = """
                    SELECT id, source_text, translated_text, timestamp, source_language, target_language, translation_scenario
                    FROM history 
                    ORDER BY timestamp DESC 
                    LIMIT @pageSize OFFSET @offset
                    """;
            }
            else
            {
                dataCommand.CommandText = """
                    SELECT id, source_text, translated_text, timestamp, source_language, target_language, translation_scenario
                    FROM history 
                    WHERE source_text LIKE @search OR translated_text LIKE @search
                    ORDER BY timestamp DESC 
                    LIMIT @pageSize OFFSET @offset
                    """;
                dataCommand.Parameters.AddWithValue("@search", $"%{searchText}%");
            }

            dataCommand.Parameters.AddWithValue("@pageSize", pageSize);
            dataCommand.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

            var translations = new List<TranslationHistory>();
            using var reader = await dataCommand.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                translations.Add(new TranslationHistory
                {
                    Id = reader.GetInt32("id"),
                    SourceText = reader.GetString("source_text"),
                    TranslatedText = reader.GetString("translated_text"),
                    Timestamp = reader.GetDateTime("timestamp"),
                    SourceLanguage = reader.IsDBNull("source_language") ? null : reader.GetString("source_language"),
                    TargetLanguage = reader.IsDBNull("target_language") ? null : reader.GetString("target_language"),
                    TranslationScenario = reader.IsDBNull("translation_scenario") ? null : reader.GetString("translation_scenario")
                });
            }

            return new PagedResult<TranslationHistory>
            {
                Data = translations,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<TranslationHistory?> GetLastTranslationAsync()
        {
            var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT id, source_text, translated_text, timestamp, source_language, target_language, translation_scenario
                FROM history 
                ORDER BY timestamp DESC 
                LIMIT 1
                """;

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new TranslationHistory
                {
                    Id = reader.GetInt32("id"),
                    SourceText = reader.GetString("source_text"),
                    TranslatedText = reader.GetString("translated_text"),
                    Timestamp = reader.GetDateTime("timestamp"),
                    SourceLanguage = reader.IsDBNull("source_language") ? null : reader.GetString("source_language"),
                    TargetLanguage = reader.IsDBNull("target_language") ? null : reader.GetString("target_language"),
                    TranslationScenario = reader.IsDBNull("translation_scenario") ? null : reader.GetString("translation_scenario")
                };
            }

            return null;
        }

        public async Task<bool> DeleteTranslationAsync(int id)
        {
            var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM history WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}