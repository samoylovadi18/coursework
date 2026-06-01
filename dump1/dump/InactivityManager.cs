using System;
using System.Windows.Forms;
using System.IO;

namespace dump
{
    public static class InactivityManager
    {
        private static Timer inactivityTimer;
        private static int inactiveSeconds = 0;
        private static int inactivityTime = 0;
        private static bool autoLockEnabled = false;
        private static string settingsFile = Application.StartupPath + "\\security_settings.txt";
        private static Form currentActiveForm = null;

        public static event Action OnLockRequest;

        static InactivityManager()
        {
            LoadSecuritySettings();

            inactivityTimer = new Timer();
            inactivityTimer.Interval = 1000;
            inactivityTimer.Tick += InactivityTimer_Tick;

            if (autoLockEnabled && inactivityTime > 0)
            {
                inactivityTimer.Start();
            }
        }

        private static void LoadSecuritySettings()
        {
            try
            {
                if (File.Exists(settingsFile))
                {
                    string[] lines = File.ReadAllLines(settingsFile);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("InactivityTime="))
                        {
                            string value = line.Replace("InactivityTime=", "");
                            inactivityTime = Convert.ToInt32(value);
                        }
                        else if (line.StartsWith("AutoLockEnabled="))
                        {
                            string value = line.Replace("AutoLockEnabled=", "");
                            autoLockEnabled = Convert.ToBoolean(value);
                        }
                    }
                }
                else
                {
                    inactivityTime = 0;
                    autoLockEnabled = false;
                }
            }
            catch
            {
                inactivityTime = 0;
                autoLockEnabled = false;
            }
        }

        public static void SaveSecuritySettings(int time, bool enabled)
        {
            inactivityTime = time;
            autoLockEnabled = enabled;

            try
            {
                string content = $"InactivityTime={inactivityTime}\nAutoLockEnabled={autoLockEnabled}";
                File.WriteAllText(settingsFile, content);
            }
            catch { }

            ResetTimer();

            if (autoLockEnabled && inactivityTime > 0)
            {
                inactivityTimer.Start();
            }
            else
            {
                inactivityTimer.Stop();
            }
        }

        public static int GetInactivityTime()
        {
            return inactivityTime;
        }

        public static bool GetAutoLockEnabled()
        {
            return autoLockEnabled;
        }

        public static void RegisterForm(Form form)
        {
            if (currentActiveForm != null)
            {
                currentActiveForm.MouseMove -= ResetTimer;
                currentActiveForm.KeyPress -= ResetTimer;
            }

            currentActiveForm = form;

            form.MouseMove += ResetTimer;
            form.KeyPress += ResetTimer;

            SubscribeControls(form.Controls);

            ResetTimer();
        }

        private static void SubscribeControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                control.MouseMove += ResetTimer;
                control.KeyPress += ResetTimer;

                if (control.HasChildren)
                {
                    SubscribeControls(control.Controls);
                }
            }
        }

        public static void UnregisterForm()
        {
            if (currentActiveForm != null)
            {
                currentActiveForm.MouseMove -= ResetTimer;
                currentActiveForm.KeyPress -= ResetTimer;
                currentActiveForm = null;
            }
        }

        private static void ResetTimer(object sender = null, EventArgs e = null)
        {
            if (autoLockEnabled && inactivityTime > 0)
            {
                inactiveSeconds = 0;
                if (!inactivityTimer.Enabled)
                {
                    inactivityTimer.Start();
                }
            }
        }

        private static void InactivityTimer_Tick(object sender, EventArgs e)
        {
            if (autoLockEnabled && inactivityTime > 0)
            {
                inactiveSeconds++;
                if (inactiveSeconds >= inactivityTime)
                {
                    inactivityTimer.Stop();
                    OnLockRequest?.Invoke();
                }
            }
        }

        public static void ResetActivity()
        {
            ResetTimer();
        }
    }
}