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
    /// <summary>
    /// Статический класс для управления автоматической блокировкой системы при бездействии пользователя.
    /// Отслеживает активность пользователя на зарегистрированных формах и инициирует блокировку по истечении заданного времени.
    /// </summary>
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

        /// <summary>
        /// Событие, возникающее при запросе блокировки системы.
        /// </summary>
        public static event Action OnLockRequest;

        /// <summary>
        /// Статический конструктор, инициализирующий таймер и загружающий настройки.
        /// </summary>
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
        /// Загружает настройки блокировки из JSON-файла.
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
        /// Сохраняет текущие настройки блокировки в JSON-файл.
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
        /// Устанавливает настройки автоматической блокировки.
        /// </summary>
        /// <param name="enabled">True если блокировка включена.</param>
        /// <param name="seconds">Время бездействия в секундах до блокировки.</param>
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
        /// Возвращает состояние автоматической блокировки.
        /// </summary>
        public static bool GetAutoLockEnabled() => autoLockEnabled;

        /// <summary>
        /// Возвращает время бездействия в секундах до блокировки.
        /// </summary>
        public static int GetInactivityTime() => inactivityTimeSeconds;

        /// <summary>
        /// Регистрирует форму для отслеживания активности пользователя.
        /// </summary>
        /// <param name="form">Форма для регистрации.</param>
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

        /// <summary>
        /// Отменяет регистрацию формы и отписывается от её событий.
        /// </summary>
        /// <param name="form">Форма для отмены регистрации. Если null, отменяет регистрацию всех форм.</param>
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

        /// <summary>
        /// Обработчик закрытия формы. Автоматически отменяет регистрацию формы.
        /// </summary>
        private static void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sender is Form form)
            {
                UnregisterForm(form);
            }
        }

        /// <summary>
        /// Обработчик активности пользователя. Сбрасывает таймер бездействия.
        /// </summary>
        private static void OnUserActivity(object sender, EventArgs e)
        {
            ResetActivity();
        }

        /// <summary>
        /// Сбрасывает время последней активности пользователя.
        /// </summary>
        public static void ResetActivity()
        {
            if (!isLocked)
            {
                lastActivityTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Обработчик тика таймера. Проверяет время бездействия и инициирует блокировку при необходимости.
        /// </summary>
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

        /// <summary>
        /// Инициирует запрос на блокировку системы.
        /// </summary>
        private static void RequestLock()
        {
            isLocked = true;
            OnLockRequest?.Invoke();
        }

        /// <summary>
        /// Разблокирует систему после ввода правильного пароля.
        /// </summary>
        public static void Unlock()
        {
            isLocked = false;
            ResetActivity();
        }

        /// <summary>
        /// Внутренний класс для сериализации настроек блокировки в JSON.
        /// </summary>
        private class InactivitySettings
        {
            public bool AutoLockEnabled { get; set; }
            public int InactivityTimeSeconds { get; set; }
        }
    }
}