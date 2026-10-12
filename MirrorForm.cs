using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AIHider
{
    public class MirrorForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
        private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        // GDI capture
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h,
                                          IntPtr hdcSrc, int xSrc, int ySrc, int rop);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int w, int h);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        // PrintWindow for capturing a specific window even if obscured
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        private const int SRCCOPY = 0x00CC0020;
        private const uint PW_RENDERFULLCONTENT = 0x00000002;

        private PictureBox _preview;
        private TrackBar _opacitySlider;
        private Label _opacityLabel;
        private System.Windows.Forms.Timer _captureTimer;

        private IntPtr _targetWindow;

        public MirrorForm(IntPtr targetWindow)
        {
            _targetWindow = targetWindow;

            this.Text = "App Mirror";
            this.Size = new Size(720, 560);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(60, 60);
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;
            this.TopMost = true;
            this.MinimumSize = new Size(360, 320);

            _preview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black,
                Cursor = Cursors.Default
            };

            var bottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(30, 30, 30),
                Cursor = Cursors.Default
            };

            _opacityLabel = new Label
            {
                Text = "Opacity: 80%",
                Left = 10,
                Top = 10,
                Width = 120,
                Height = 20,
                ForeColor = Color.LightGray,
                Cursor = Cursors.Default
            };

            _opacitySlider = new TrackBar
            {
                Minimum = 20,
                Maximum = 100,
                Value = 80,
                Left = 130,
                Top = 0,
                Width = 400,
                Height = 36,
                TickFrequency = 10,
                Cursor = Cursors.Default
            };
            _opacitySlider.Scroll += (s, e) =>
            {
                this.Opacity = _opacitySlider.Value / 100.0;
                _opacityLabel.Text = $"Opacity: {_opacitySlider.Value}%";
            };

            bottom.Controls.Add(_opacityLabel);
            bottom.Controls.Add(_opacitySlider);

            this.Controls.Add(_preview);
            this.Controls.Add(bottom);

            _captureTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _captureTimer.Tick += (s, e) => CaptureTarget();
            _captureTimer.Start();

            this.Opacity = 0.80;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetWindowDisplayAffinity(this.Handle, WDA_EXCLUDEFROMCAPTURE);
        }

        /// <summary>
        /// Captures the target window using PrintWindow, which works even if
        /// the target window is hidden from screen capture (WDA_EXCLUDEFROMCAPTURE).
        /// </summary>
        private void CaptureTarget()
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            if (_targetWindow == IntPtr.Zero) return;

            try
            {
                if (!GetWindowRect(_targetWindow, out RECT rect)) return;

                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                if (width <= 0 || height <= 0) return;

                IntPtr hdcWindow = GetWindowDC(_targetWindow);
                IntPtr hdcMem = CreateCompatibleDC(hdcWindow);
                IntPtr hBmp = CreateCompatibleBitmap(hdcWindow, width, height);
                IntPtr hOld = SelectObject(hdcMem, hBmp);

                // PW_RENDERFULLCONTENT = 2 to force full render even when hidden from capture
                PrintWindow(_targetWindow, hdcMem, PW_RENDERFULLCONTENT);

                var bmp = Image.FromHbitmap(hBmp);

                SelectObject(hdcMem, hOld);
                DeleteObject(hBmp);
                DeleteDC(hdcMem);
                ReleaseDC(_targetWindow, hdcWindow);

                var old = _preview.Image;
                _preview.Image = bmp;
                old?.Dispose();
            }
            catch
            {
                // Silent
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }

            _captureTimer?.Stop();
            _captureTimer?.Dispose();
            _preview.Image?.Dispose();

            base.OnFormClosing(e);
        }
    }
}