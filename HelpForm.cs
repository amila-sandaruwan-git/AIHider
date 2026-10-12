using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AIHider
{
    public class HelpForm : Form
    {
        public HelpForm()
        {
            this.Text = "Help — How to Use";
            this.Size = new Size(620, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Cursor = Cursors.Default;

            // ================= Scrollable container =================
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(24, 20, 24, 20),
                Cursor = Cursors.Default
            };

            int W = 540;
            int y = 0;

            // Helper: add a section header
            Label Header(string text)
            {
                var lbl = new Label
                {
                    Text = text,
                    Left = 0,
                    Top = y,
                    Width = W,
                    Height = 28,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Default
                };
                y += 32;
                return lbl;
            }

            // Helper: add body text
            Label Body(string text, int height)
            {
                var lbl = new Label
                {
                    Text = text,
                    Left = 0,
                    Top = y,
                    Width = W,
                    Height = height,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.LightGray,
                    TextAlign = ContentAlignment.TopLeft,
                    Cursor = Cursors.Default
                };
                y += height + 6;
                return lbl;
            }

            // Helper: horizontal separator
            Panel Sep()
            {
                var sep = new Panel
                {
                    Left = 0,
                    Top = y,
                    Width = W,
                    Height = 1,
                    BackColor = Color.FromArgb(70, 70, 75),
                    Cursor = Cursors.Default
                };
                y += 12;
                return sep;
            }

            // ================= Title =================
            var lblTitle = new Label
            {
                Text = "AI Hider — Help & Guide",
                Left = 0,
                Top = 0,
                Width = W,
                Height = 34,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.Gold,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Default
            };
            y += 44;

            // ================= 1. What it does =================
            scrollPanel.Controls.Add(lblTitle);
            scrollPanel.Controls.Add(Header("1.  What This App Does"));
            scrollPanel.Controls.Add(Body(
                "AI Hider is a private AI assistant that stays invisible when you " +
                "share your screen on Zoom, Google Meet, Microsoft Teams, Discord, " +
                "and other platforms.\n\n" +
                "It listens to system audio (the voices coming through your speakers), " +
                "transcribes speech, detects questions, and answers them using an AI " +
                "model — while remaining hidden from the screen you are sharing.",
                110));
            scrollPanel.Controls.Add(Sep());

            // ================= 2. Voice questions =================
            scrollPanel.Controls.Add(Header("2.  Voice Questions (Hands-Free)"));
            scrollPanel.Controls.Add(Body(
                "1.  Click  ▶ Start  in the top bar.\n" +
                "2.  The button turns red and the status shows \"🎤 Listening…\".\n" +
                "3.  When someone speaks through your speakers, the app captures it.\n" +
                "4.  After ~1.2 seconds of silence, the audio is sent to the AI.\n" +
                "5.  The transcript appears on the left, and the answer on the right.\n\n" +
                "Click  ■ Stop  to pause listening at any time.",
                130));
            scrollPanel.Controls.Add(Sep());

            // ================= 3. Typed questions =================
            scrollPanel.Controls.Add(Header("3.  Typed Questions"));
            scrollPanel.Controls.Add(Body(
                "Type any question in the bottom text box and press  Enter  (or click " +
                "Send). The app answers using the same AI engine.\n\n" +
                "Use this when you want to ask something silently.",
                70));
            scrollPanel.Controls.Add(Sep());

            // ================= 4. Button guide =================
            scrollPanel.Controls.Add(Header("4.  Button Guide"));

            var btnTable = new TableLayoutPanel
            {
                Left = 0,
                Top = y,
                Width = W,
                Height = 240,
                ColumnCount = 2,
                RowCount = 8,
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };
            btnTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
            btnTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390f));
            for (int i = 0; i < 8; i++)
                btnTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));

            void AddBtnRow(int row, string name, string desc)
            {
                var lblName = new Label
                {
                    Text = name,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(180, 200, 230),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Default,
                    BackColor = Color.Transparent
                };
                var lblDesc = new Label
                {
                    Text = desc,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.LightGray,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Default,
                    BackColor = Color.Transparent
                };
                btnTable.Controls.Add(lblName, 0, row);
                btnTable.Controls.Add(lblDesc, 1, row);
            }

            AddBtnRow(0, "Load File", "Load a project document (.txt / .md / .pdf / .docx) as context");
            AddBtnRow(1, "Start / Stop", "Begin or pause system-audio listening");
            AddBtnRow(2, "📌 Pin", "Keep the window always on top of other apps");
            AddBtnRow(3, "F-  /  F+", "Decrease or increase text size");
            AddBtnRow(4, "Light", "Switch between dark and light theme");
            AddBtnRow(5, "❓ Help", "Open this guide");
            AddBtnRow(6, "About", "Project info, credits, and GitHub link");
            AddBtnRow(7, "Glass slider", "Adjust window transparency (20%–100%)");

            scrollPanel.Controls.Add(btnTable);
            y += 240 + 10;
            scrollPanel.Controls.Add(Sep());

            // ================= 5. Privacy =================
            scrollPanel.Controls.Add(Header("5.  Privacy — How It Stays Hidden"));
            scrollPanel.Controls.Add(Body(
                "The app uses a Windows API called  SetWindowDisplayAffinity  with the " +
                "WDA_EXCLUDEFROMCAPTURE  flag. This tells Windows to exclude this " +
                "window from any screen-capture pipeline — including Zoom, Teams, " +
                "Meet, Discord, and OBS.\n\n" +
                "You still see the window normally. Other participants see nothing.\n\n" +
                "⚠  Only works when you share your ENTIRE screen. If you share a " +
                "single window, other windows aren't captured anyway.",
                130));
            scrollPanel.Controls.Add(Sep());

            // ================= 6. Troubleshooting =================
            scrollPanel.Controls.Add(Header("6.  Troubleshooting"));

            scrollPanel.Controls.Add(Body(
                "•  No answers appearing?  Check that the status shows \"🎤 Listening…\" " +
                "and that audio is playing through your speakers.\n\n" +
                "•  Answers delayed?  The AI needs ~1.2 s of silence to detect the end " +
                "of a sentence. Speak clearly and pause briefly after questions.\n\n" +
                "•  Window visible during share?  Make sure you are sharing the entire " +
                "screen, not a single window.\n\n" +
                "•  \"Check failed\" on Start?  Your internet or API key may be down. " +
                "Try again in a minute.",
                200));
            scrollPanel.Controls.Add(Sep());

            // ================= 7. Close button =================
            var btnClose = new Button
            {
                Text = "Close",
                Left = (W - 120) / 2,
                Top = y + 10,
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