using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AIHider
{
    public static class GlassHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMSBT_TRANSIENTWINDOW = 3;  // Acrylic
        private const int DWMSBT_MAINWINDOW = 2;       // Mica

        // Windows 10 acrylic
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        private const int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4;
        private const int WCA_ACCENT_POLICY = 19;

        /// <summary>
        /// Applies a Windows 11 acrylic backdrop, or Windows 10 acrylic fallback.
        /// Returns true if a Win11 backdrop was applied.
        /// </summary>
        public static bool ApplyGlass(IntPtr hwnd, bool dark)
        {
            int darkFlag = dark ? 1 : 0;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkFlag, sizeof(int));

            // Try Windows 11 system backdrop
            try
            {
                int backdrop = DWMSBT_TRANSIENTWINDOW; // acrylic
                int hr = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
                if (hr == 0) return true;
            }
            catch { }

            // Fall back to Windows 10 acrylic
            ApplyAcrylic10(hwnd, dark);
            return false;
        }

        private static void ApplyAcrylic10(IntPtr hwnd, bool dark)
        {
            // Tint: AABBGGRR. Dark = near black, Light = off-white
            int tint = dark
                ? unchecked((int)0x99000000)   // A=153, RGB=0
                : unchecked((int)0x99F0F0F0);  // A=153, RGB=light grey

            var accent = new AccentPolicy
            {
                AccentState = ACCENT_ENABLE_ACRYLICBLURBEHIND,
                AccentFlags = 2,
                GradientColor = tint,
                AnimationId = 0
            };

            int size = Marshal.SizeOf(accent);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(accent, ptr, false);
                var data = new WindowCompositionAttributeData
                {
                    Attribute = WCA_ACCENT_POLICY,
                    Data = ptr,
                    SizeOfData = size
                };
                SetWindowCompositionAttribute(hwnd, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}