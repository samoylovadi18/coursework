using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;

namespace dump
{
    public static class InactivityManager
    {
        private static Timer inactivityTimer;
        private static DateTime lastActivityTime;
        private static bool isLocked = false;
        private static List<Form> registeredForms = new List<Form>();

        // Настройки блокировки
        private static bool autoLockEnabled = false;
        private static int inactivityTimeSeconds = 60;

        private const string SETTINGS_FILE = "inactivity_settings.json";

        public static event Action OnLockRequest;

        static InactivityManager()
        {
            LoadSettings();

            inactivityTimer = new Timer();
            inactivityTimer.Interval = 1000;
            inactivityTimer.Tick += InactivityTimer_Tick;

            if (autoLockEnabled)
            {
                inactivityTimer.Start();
            }
        }

        /// <summary>
        /// Загрузка настроек блокировки из файла
        /// </summary>
        private static void LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    string json = File.ReadAllText(SETTINGS_FILE);
                    var settings = JsonSerializer.Deserialize<InactivitySettings>(json);
                    if (settings != null)
                    {
                        autoLockEnabled = settings.AutoLockEnabled;
                        inactivityTimeSeconds = settings.InactivityTimeSeconds;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохранение настроек блокировки в файл
        /// </summary>
        public static void SaveSettings()
        {
            try
            {
                var settings = new InactivitySettings
                {
                    AutoLockEnabled = autoLockEnabled,
                    InactivityTimeSeconds = inactivityTimeSeconds
                };
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SETTINGS_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Установка настроек блокировки
        /// </summary>
        public static void SetSecuritySettings(bool enabled, int seconds)
        {
            autoLockEnabled = enabled;
            inactivityTimeSeconds = seconds;
            SaveSettings();

            if (autoLockEnabled)
            {
                inactivityTimer.Start();
                ResetActivity();
            }
            else
            {
                inactivityTimer.Stop();
            }
        }

        /// <summary>
        /// Получение текущих настроек
        /// </summary>
        public static bool GetAutoLockEnabled() => autoLockEnabled;
        public static int GetInactivityTime() => inactivityTimeSeconds;

        public static void RegisterForm(Form form)
        {
            if (!registeredForms.Contains(form))
            {
                registeredForms.Add(form);

                // Подписываемся на события активности формы
                form.MouseMove += OnUserActivity;
                form.KeyPress += OnUserActivity;
                form.Click += OnUserActivity;
                form.FormClosing += Form_FormClosing;
            }

            ResetActivity();
        }

        public static void UnregisterForm(Form form = null)
        {
            if (form != null)
            {
                form.MouseMove -= OnUserActivity;
                form.KeyPress -= OnUserActivity;
                form.Click -= OnUserActivity;
                form.FormClosing -= Form_FormClosing;
                registeredForms.Remove(form);
            }
        }

        private static void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sender is Form form)
            {
                UnregisterForm(form);
            }
        }

        private static void OnUserActivity(object sender, EventArgs e)
        {
            ResetActivity();
        }

        public static void ResetActivity()
        {
            if (!isLocked)
            {
                lastActivityTime = DateTime.Now;
            }
        }

        private static void InactivityTimer_Tick(object sender, EventArgs e)
        {
            if (!autoLockEnabled) return;
            if (isLocked) return;
            if (registeredForms.Count == 0) return;

            TimeSpan inactiveDuration = DateTime.Now - lastActivityTime;
            if (inactiveDuration.TotalSeconds >= inactivityTimeSeconds)
            {
                RequestLock();
            }
        }

        private static void RequestLock()
        {
            isLocked = true;
            OnLockRequest?.Invoke();
        }

        public static void Unlock()
        {
            isLocked = false;
            ResetActivity();
        }

        private class InactivitySettings
        {
            public bool AutoLockEnabled { get; set; }
            public int InactivityTimeSeconds { get; set; }
        }
    }
}