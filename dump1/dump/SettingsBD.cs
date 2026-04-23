using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace dump
{
    public static class SettingsBD
    {
        private const string CONFIG_FILE = "db_config.json";

        public class ConnectionConfig
        {
            public string Server { get; set; } = "localhost";
            public string Username { get; set; } = "root";
            public string Password { get; set; } = "";
            public string Database { get; set; } = "da";

            public string GetConnectionString()
            {
                return $"server={Server};username={Username};password={Password};database={Database};";
            }
        }

        private static ConnectionConfig _currentConfig;
        private static string _activeConnectionString;

        static SettingsBD()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    _currentConfig = JsonSerializer.Deserialize<ConnectionConfig>(json);
                }
                else
                {
                    // Создаем конфиг по умолчанию
                    _currentConfig = new ConnectionConfig();
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки конфига: {ex.Message}");
                _currentConfig = new ConnectionConfig();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                string json = JsonSerializer.Serialize(_currentConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONFIG_FILE, json);

                // Сбрасываем активное подключение при сохранении новых настроек
                _activeConnectionString = null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения конфигурации: {ex.Message}");
            }
        }

        public static ConnectionConfig GetCurrentConfig()
        {
            return new ConnectionConfig
            {
                Server = _currentConfig.Server,
                Username = _currentConfig.Username,
                Password = _currentConfig.Password,
                Database = _currentConfig.Database
            };
        }

        public static void UpdateConfig(ConnectionConfig newConfig)
        {
            _currentConfig = newConfig;
            SaveConfig();
        }

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_activeConnectionString))
                {
                    _activeConnectionString = _currentConfig.GetConnectionString();

                    if (!TestConnection(_activeConnectionString))
                    {
                        throw new InvalidOperationException("Не удалось подключиться к базе данных с текущими настройками");
                    }
                }
                return _activeConnectionString;
            }
        }

        public static bool TestConnection(string connectionString = null)
        {
            string testString = connectionString ?? _currentConfig.GetConnectionString();

            try
            {
                using (var connection = new MySqlConnection(testString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}