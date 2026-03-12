using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace dump
{
    public static class SettingsBD
    {
        private static readonly List<string> _connectionStrings = new List<string>
        {
            "server=localhost;username=root;password=;database=da;",
            "server=10.207.106.12;username=user98;password=kn70;database=db98;"
        };

        private static string _activeConnectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_activeConnectionString))
                {
                    _activeConnectionString = FindWorkingConnection();
                }
                return _activeConnectionString;
            }
        }

        private static string FindWorkingConnection()
        {
            foreach (var connectionString in _connectionStrings)
            {
                if (TestConnection(connectionString))
                {
                    return connectionString;
                }
            }

            throw new InvalidOperationException("Не удалось подключиться ни к одной из баз данных");
        }

        private static bool TestConnection(string connectionString)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
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