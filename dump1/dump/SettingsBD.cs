using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace dump
{
    /// <summary>
    /// Статический класс для управления настройками подключения к базе данных.
    /// Обеспечивает сохранение, загрузку и проверку параметров подключения к MySQL.
    /// </summary>
    public static class SettingsBD
    {
        private const string CONFIG_FILE = "db_config.json";

        /// <summary>
        /// Класс конфигурации подключения к базе данных.
        /// </summary>
        public class ConnectionConfig
        {
            /// <summary>
            /// Адрес сервера базы данных.
            /// </summary>
            public string Server { get; set; } = "localhost";

            /// <summary>
            /// Имя пользователя для подключения.
            /// </summary>
            public string Username { get; set; } = "root";

            /// <summary>
            /// Пароль пользователя.
            /// </summary>
            public string Password { get; set; } = "";

            /// <summary>
            /// Имя базы данных.
            /// </summary>
            public string Database { get; set; } = "da";

            /// <summary>
            /// Формирует строку подключения к MySQL.
            /// </summary>
            /// <returns>Строка подключения с параметрами.</returns>
            public string GetConnectionString()
            {
                return $"server={Server};username={Username};password={Password};database={Database};Charset=utf8mb4;Allow User Variables=True;";
            }
        }

        private static ConnectionConfig _currentConfig;
        private static string _activeConnectionString;

        /// <summary>
        /// Статический конструктор, загружающий конфигурацию при первом обращении.
        /// </summary>
        static SettingsBD()
        {
            LoadConfig();
        }

        /// <summary>
        /// Загружает конфигурацию из JSON-файла.
        /// Если файл отсутствует, создаёт конфигурацию по умолчанию.
        /// </summary>
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

        /// <summary>
        /// Сохраняет текущую конфигурацию в JSON-файл.
        /// </summary>
        /// <exception cref="Exception">Выбрасывается при ошибке сохранения.</exception>
        public static void SaveConfig()
        {
            try
            {
                string json = JsonSerializer.Serialize(_currentConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONFIG_FILE, json);
                _activeConnectionString = null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения конфигурации: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает копию текущей конфигурации подключения.
        /// </summary>
        /// <returns>Объект ConnectionConfig с текущими настройками.</returns>
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

        /// <summary>
        /// Обновляет конфигурацию подключения и сохраняет её.
        /// </summary>
        /// <param name="newConfig">Новые настройки подключения.</param>
        public static void UpdateConfig(ConnectionConfig newConfig)
        {
            _currentConfig = newConfig;
            SaveConfig();
        }

        /// <summary>
        /// Получает строку подключения на основе текущей конфигурации.
        /// </summary>
        /// <returns>Строка подключения.</returns>
        public static string GetConnectionString()
        {
            return _currentConfig.GetConnectionString();
        }

        /// <summary>
        /// Свойство, возвращающее проверенную строку подключения.
        /// При первом обращении проверяет работоспособность подключения.
        /// </summary>
        /// <exception cref="InvalidOperationException">Выбрасывается при невозможности подключения.</exception>
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

        /// <summary>
        /// Проверяет возможность подключения к базе данных.
        /// </summary>
        /// <param name="connectionString">Строка подключения для проверки. Если null, используется текущая конфигурация.</param>
        /// <returns>True если подключение успешно, иначе False.</returns>
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

        /// <summary>
        /// Создаёт и возвращает новое подключение к базе данных.
        /// </summary>
        /// <returns>Открытое подключение к MySQL.</returns>
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}