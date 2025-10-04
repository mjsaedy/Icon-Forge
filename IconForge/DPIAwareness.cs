using System;
using System.Runtime.InteropServices;

public static class DPIAwareness {

        [DllImport("Shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDpiAwareness value);
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        private enum ProcessDpiAwareness {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }
        public static void SetDpiAwareness() {
            try {
                // Try Windows 8.1+ API first
                SetProcessDpiAwareness(ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware);
            }
            catch {
                try {
                    // Fall back to Windows Vista+ API
                    SetProcessDPIAware();
                }
                catch {
                    // If both fail, continue without DPI awareness
                }
            }
        }
}