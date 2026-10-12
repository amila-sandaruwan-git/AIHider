using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AIHider
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            this.Text = "About";
            this.Size = new Size(540, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Cursor = Cursors.Default;

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(20, 18, 20, 18),
                Cursor = Cursors.Default
            };

            int W = 460;

            // =================== Section 1: About the project ===================
            var lblProjHeader = new Label
            {
                Text = "About This Project",
                Left = 0,
                Top = 0,
                Width = W,
                Height = 30,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.Gold,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Default
            };

            var lblProjName = new Label
            {
                Text = "AI Hider — Private AI Assistant",
                Left = 0,
                Top = 32,
                Width = W,
                Height = 22,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Default
            };

            var lblProjDesc = new Label
            {
                Text = "A desktop AI assistant that stays invisible during screen " +
                       "sharing on Zoom, Google Meet, Microsoft Teams, Discord, and " +
                       "similar platforms. It listens to system audio, transcribes " +
                       "speech, detects questions, and answers them using a large " +
                       "language model — all while remaining hidden from the shared " +
                       "screen on Windows 11.",
                Left = 0,
                Top = 58,
                Width = W,
                Height = 120,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.TopCenter,
                Cursor = Cursors.Default
            };

            var sep1 = new Panel
            {
                Left = 0,
                Top = 186,
                Width = W,
                Height = 1,
                BackColor = Color.FromArgb(70, 70, 75),
                Cursor = Cursors.Default
            };

            // =================== Section 2: Built With (two-column, centered) ===================
            var lblTechHeader = new Label
            {
                Text = "Built With",
                Left = 0,
                Top = 200,
                Width = W,
                Height = 26,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Gold,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Default
            };

            // ⭐ Two columns: labels | values — both left-aligned inside, whole block centered
            var techTable = new TableLayoutPanel
            {
                Left = 0,
                Top = 230,
                Width = W,
                Height = 160,
                ColumnCount = 2,
                RowCount = 7,
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };
            techTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160f));
            techTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240f));
            for (int i = 0; i < 7; i++)
                techTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));

            void AddTechRow(int row, string label, string value)
            {
                var lbl = new Label
                {
                    Text = label,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(180, 200, 230),
                    TextAlign = ContentAlignment.MiddleRight,
                    Padding = new Padding(0, 0, 10, 0),
                    Cursor = Cursors.Default,
                    BackColor = Color.Transparent
                };
                var val = new Label
                {
                    Text = value,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.LightGray,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Default,
                    BackColor = Color.Transparent
                };
                techTable.Controls.Add(lbl, 0, row);
                techTable.Controls.Add(val, 1, row);
            }

            AddTechRow(0, "Language", "C#");
            AddTechRow(1, "Framework", ".NET 10 (WinForms)");
            AddTechRow(2, "AI API (chat)", "Groq — Llama / GPT-OSS");
            AddTechRow(3, "AI API (backup)", "Google Gemini");
            AddTechRow(4, "Audio Capture", "NAudio (system loopback)");
            AddTechRow(5, "Speech-to-Text", "Groq Whisper (large-v3-turbo)");
            AddTechRow(6, "Privacy API", "SetWindowDisplayAffinity");

            var sep2 = new Panel
            {
                Left = 0,
                Top = 396,
                Width = W,
                Height = 1,
                BackColor = Color.FromArgb(70, 70, 75),
                Cursor = Cursors.Default
            };

            // =================== Section 3: About Me ===================
            var lblMeHeader = new Label
            {
                Text = "About Me",
                Left = 0,
                Top = 410,
                Width = W,
                Height = 26,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Gold,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Default
            };

            var picBox = new PictureBox
            {
                Left = (W - 100) / 2,
                Top = 442,
                Width = 100,
                Height = 100,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(50, 50, 55),
                Cursor = Cursors.Default
            };

            string imgPath = System.IO.Path.Combine(
                AppContext.BaseDirectory, "resources", "profile.png");
            if (System.IO.File.Exists(imgPath))
            {
                try { picBox.Image = Image.FromFile(imgPath); } catch { }
            }

            picBox.Paint += (s, e) =>
            {
                if (picBox.Image == null)
                {
                    using var pen = new Pen(Color.FromArgb(120, 120, 120), 2);
                    e.Graphics.DrawEllipse(pen, 1, 1, picBox.Width - 3, picBox.Height - 3);
                    using var brush = new SolidBrush(Color.FromArgb(120, 120, 120));
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString("No image", this.Font, brush,
                        new RectangleF(0, 0, picBox.Width, picBox.Height), sf);
                }
            };
            picBox.Region = RoundedRegion(picBox.Width, picBox.Height, picBox.Width / 2);

            var lblName = new Label
            {
                Text = "W.K. Amila Sandaruwan",
                Left = 0,
                Top = 552,
                Width = W,
                Height = 24,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Default
            };

            var lblHandle = new Label
            {
                Text = "amila-sandaruwan-git",
                Left = 0,
                Top = 578,
                Width = W,
                Height = 18,
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Default
            };

            var lblBio = new Label
            {
                Text = "Software Engineering undergraduate building full-stack apps " +
                       "with React, Next.js, and TypeScript. 5+ projects shipped. " +
                       "Passionate about clean code and performance.",
                Left = 0,
                Top = 600,
                Width = W,
                Height = 60,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.TopCenter,
                Cursor = Cursors.Default
            };

            var link = new LinkLabel
            {
                Text = "github.com/amila-sandaruwan-git",
                Left = 0,
                Top = 662,
                Width = W,
                Height = 20,
                Font = new Font("Segoe UI", 9f, FontStyle.Underline),
                LinkColor = Color.FromArgb(120, 180, 255),
                ActiveLinkColor = Color.FromArgb(200, 220, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            link.LinkClicked += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/amila-sandaruwan-git",
                        UseShellExecute = true
                    });
                }
                catch { }
            };

            var btnClose = new Button
            {
                Text = "Close",
                Left = (W - 120) / 2,
                Top = 692,
                Width = 120,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 90, 140),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Default
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Region = RoundedRegion(btnClose.Width, btnClose.Height, 6);
            btnClose.Click += (s, e) => this.Close();

            scrollPanel.Controls.Add(lblProjHeader);
            scrollPanel.Controls.Add(lblProjName);
            scrollPanel.Controls.Add(lblProjDesc);
            scrollPanel.Controls.Add(sep1);
            scrollPanel.Controls.Add(lblTechHeader);
            scrollPanel.Controls.Add(techTable);
            scrollPanel.Controls.Add(sep2);
            scrollPanel.Controls.Add(lblMeHeader);
            scrollPanel.Controls.Add(picBox);
            scrollPanel.Controls.Add(lblName);
            scrollPanel.Controls.Add(lblHandle);
            scrollPanel.Controls.Add(lblBio);
            scrollPanel.Controls.Add(link);
            scrollPanel.Controls.Add(btnClose);

            this.Controls.Add(scrollPanel);
        }

        private static Region RoundedRegion(int w, int h, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(w - d, 0, d, d, 270, 90);
            path.AddArc(w - d, h - d, d, d, 0, 90);
            path.AddArc(0, h - d, d, d, 90, 90);
            path.CloseAllFigures();
            return new Region(path);
        }
    }
}