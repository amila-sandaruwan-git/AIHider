// TestForm.cs — no auto-Help, no resize cursor (splitter locked).
using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AIHider
{
    public class TestForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
        private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        // ⭐ Cursor forcing
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);
        private const int WM_SETCURSOR = 0x0020;
        private static readonly IntPtr IDC_ARROW = LoadCursor(IntPtr.Zero, 32512);

        private bool _lightMode = false;
        private bool _pinned = true;
        private bool _hasCheckedReadiness = false;

        // ---- Colors ----
        private Color BgMain => _lightMode ? Color.FromArgb(245, 245, 248) : Color.FromArgb(30, 30, 30);
        private Color BgPanel => _lightMode ? Color.FromArgb(245, 245, 248) : Color.FromArgb(40, 40, 45);
        private Color BgCard => _lightMode ? Color.FromArgb(250, 250, 252) : Color.FromArgb(50, 50, 55);
        private Color BgInput => _lightMode ? Color.FromArgb(245, 245, 250) : Color.FromArgb(55, 55, 65);
        private Color BgAnswer => _lightMode ? Color.FromArgb(245, 245, 250) : Color.FromArgb(35, 35, 42);
        private Color FgText => _lightMode ? Color.FromArgb(20, 20, 20) : Color.White;
        private Color FgSub => _lightMode ? Color.FromArgb(90, 90, 90) : Color.LightGray;
        private Color FgTranscript => _lightMode ? Color.FromArgb(0, 90, 120) : Color.LightCyan;

        private Color AccentGreen => Color.FromArgb(60, 140, 60);
        private Color AccentRed => Color.FromArgb(150, 60, 60);
        private Color AccentBlue => Color.FromArgb(60, 90, 140);
        private Color AccentPurple => Color.FromArgb(120, 70, 160);
        private Color AccentNeutral => Color.FromArgb(80, 80, 90);
        private Color AccentPinned => Color.FromArgb(40, 130, 220);

        // ---- UI ----
        private Panel _topPanel;
        private FlowLayoutPanel _leftButtons;
        private Button _loadFileButton;
        private Button _startStopButton;
        private Button _pinButton;
        private Button _fontMinusButton;
        private Button _fontPlusButton;
        private Button _themeButton;
        private Button _helpButton;
        private Button _aboutButton;
        private Label _statusLabel;

        private Label _glassLabel;
        private TrackBar _glassSlider;

        private SplitContainer _split;
        private RichTextBox _transcriptBox;
        private FlowLayoutPanel _answerPanel;

        private Panel _inputPanel;
        private TextBox _inputBox;
        private Button _sendButton;

        private string _projectContext = "";
        private float _transcriptFontSize = 10f;
        private float _answerFontSize = 10f;

        private const int MaxTranscriptLines = 400;
        private const int TrimToTranscriptLines = 250;
        private const int MaxAnswerCards = 40;

        public TestForm()
        {
            this.Text = "AIHider v1.0 (private)";
            this.Size = new Size(950, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgMain;
            this.ForeColor = FgText;
            this.MinimumSize = new Size(700, 460);
            this.DoubleBuffered = true;
            this.ShowInTaskbar = false;   // ⭐ hide the taskbar button

            this.Cursor = Cursors.Default;

            this.TopMost = true;
            this.Opacity = 0.85;

            // Window icon
            try
            {
                this.Icon = new System.Drawing.Icon(
                    System.IO.Path.Combine(AppContext.BaseDirectory, "app.ico"));
            }
            catch { }

            // ================= Top panel =================
            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = BgPanel,
                Cursor = Cursors.Default
            };

            _leftButtons = new FlowLayoutPanel
            {
                Left = 6,
                Top = 8,
                Height = 32,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };

            _loadFileButton = MakeButton("Load File", 100, 30, AccentBlue);
            _loadFileButton.Click += LoadFile_Click;

            _startStopButton = MakeButton("Start", 90, 30, AccentGreen);
            _startStopButton.Click += async (s, e) => await StartStop_ClickAsync();

            _pinButton = MakeButton("📌 Pin", 80, 30, AccentPinned);
            _pinButton.Click += (s, e) => TogglePin();

            _fontMinusButton = MakeButton("F-", 36, 30, AccentNeutral);
            _fontMinusButton.Click += (s, e) => AdjustFont(-1);

            _fontPlusButton = MakeButton("F+", 36, 30, AccentNeutral);
            _fontPlusButton.Click += (s, e) => AdjustFont(+1);

            _themeButton = MakeButton("Light", 80, 30, AccentNeutral);
            _themeButton.Click += (s, e) => ToggleTheme();

            _leftButtons.Controls.Add(_loadFileButton);
            _leftButtons.Controls.Add(_startStopButton);
            _leftButtons.Controls.Add(_pinButton);
            _leftButtons.Controls.Add(_fontMinusButton);
            _leftButtons.Controls.Add(_fontPlusButton);
            _leftButtons.Controls.Add(_themeButton);

            _helpButton = MakeButton("❓ Help", 80, 30, AccentPurple);
            _helpButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _helpButton.Click += (s, e) =>
            {
                using var help = new HelpForm();
                help.ShowDialog(this);
            };

            _aboutButton = MakeButton("About", 80, 30, AccentBlue);
            _aboutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _aboutButton.Click += (s, e) =>
            {
                using var about = new AboutForm();
                about.ShowDialog(this);
            };

            _topPanel.Controls.Add(_leftButtons);
            _topPanel.Controls.Add(_helpButton);
            _topPanel.Controls.Add(_aboutButton);

            _glassLabel = new Label
            {
                Text = "Glass: 85%",
                Left = 6,
                Top = 52,
                Width = 90,
                Height = 20,
                ForeColor = FgSub,
                BackColor = Color.Transparent,
                Cursor = Cursors.Default,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _glassSlider = new TrackBar
            {
                Minimum = 20,
                Maximum = 100,
                Value = 85,
                Left = 96,
                Top = 46,
                Width = 260,
                Height = 32,
                TickFrequency = 10,
                Cursor = Cursors.Default,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _glassSlider.Scroll += (s, e) =>
            {
                this.Opacity = _glassSlider.Value / 100.0;
                _glassLabel.Text = $"Glass: {_glassSlider.Value}%";
            };

            _topPanel.Controls.Add(_glassLabel);
            _topPanel.Controls.Add(_glassSlider);

            _statusLabel = new Label
            {
                Left = 370,
                Top = 52,
                Height = 20,
                Width = _topPanel.ClientSize.Width - 460,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = FgSub,
                BackColor = Color.Transparent,
                Text = "No file loaded.  |  Listening: OFF",
                Cursor = Cursors.Default
            };
            _topPanel.Controls.Add(_statusLabel);

            _topPanel.SizeChanged += (s, e) =>
            {
                int aboutX = _topPanel.ClientSize.Width - _aboutButton.Width - 10;
                if (aboutX < 0) aboutX = 0;
                _aboutButton.Left = aboutX;
                _aboutButton.Top = 8;

                int helpX = aboutX - _helpButton.Width - 6;
                if (helpX < 0) helpX = 0;
                _helpButton.Left = helpX;
                _helpButton.Top = 8;

                int w = _topPanel.ClientSize.Width - 460;
                if (w < 100) w = 100;
                _statusLabel.Width = w;
            };

            // ================= Bottom input =================
            _inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = BgPanel,
                Padding = new Padding(6),
                Cursor = Cursors.Default
            };

            var inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
            inputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _inputBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = BgInput,
                ForeColor = FgText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 4, 6, 4),
                Cursor = Cursors.Default
            };
            _inputBox.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await SendTypedQuestionAsync();
                }
            };

            _sendButton = MakeButton("Send", 80, 30, AccentGreen);
            _sendButton.Dock = DockStyle.Fill;
            _sendButton.Margin = new Padding(0, 4, 0, 4);
            _sendButton.Click += async (s, e) => await SendTypedQuestionAsync();

            inputLayout.Controls.Add(_inputBox, 0, 0);
            inputLayout.Controls.Add(_sendButton, 1, 0);
            _inputPanel.Controls.Add(inputLayout);

            // ================= Split view (splitter locked) =================
            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = BgMain,
                Cursor = Cursors.Default,
                IsSplitterFixed = true,           // ⭐ no resize cursor
                SplitterWidth = 4
            };

            _transcriptBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = _lightMode ? Color.FromArgb(245, 245, 250) : Color.FromArgb(25, 25, 30),
                ForeColor = FgTranscript,
                Font = new Font("Segoe UI", _transcriptFontSize),
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Default,
                DetectUrls = false,
                WordWrap = true
            };

            _answerPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = _lightMode ? Color.FromArgb(240, 240, 245) : Color.FromArgb(35, 35, 40),
                Padding = new Padding(4),
                Cursor = Cursors.Default
            };

            _answerPanel.SizeChanged += (s, e) =>
            {
                foreach (Control c in _answerPanel.Controls)
                    c.Width = _answerPanel.ClientSize.Width - 24;
            };

            _split.Panel1.Controls.Add(_transcriptBox);
            _split.Panel2.Controls.Add(_answerPanel);

            this.Controls.Add(_split);
            this.Controls.Add(_inputPanel);
            this.Controls.Add(_topPanel);

            this.Shown += (s, e) =>
            {
                try { _split.SplitterDistance = 360; } catch { }
                // ⭐ Auto-Help removed — Help opens only via the ❓ Help button
            };

            AudioCaptureTest.OnTranscriptReceived += OnTranscriptReceived;
        }

        // ⭐ Hide this window from screen capture (Zoom, Meet, Teams, etc.)
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            try
            {
                SetWindowDisplayAffinity(this.Handle, WDA_EXCLUDEFROMCAPTURE);
            }
            catch { }
        }

        // ⭐ Force the standard arrow cursor over the whole form + resize edges
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SETCURSOR)
            {
                SetCursor(IDC_ARROW);
                m.Result = (IntPtr)1;
                return;
            }
            base.WndProc(ref m);
        }

        // Disable Windows resize on all borders — window stays at the size you set
        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);
        }

        // ================= Readiness check =================
        private async System.Threading.Tasks.Task<bool> RunReadinessCheckAsync()
        {
            _statusLabel.Text = "Running readiness check…";
            _statusLabel.ForeColor = Color.Orange;
            _statusLabel.Refresh();
            Application.DoEvents();

            try
            {
                string reply = await GroqClient.AskAsync(
                    "You are a diagnostic assistant.",
                    "Reply with the single word: READY");

                if (!string.IsNullOrWhiteSpace(reply) &&
                    reply.ToUpperInvariant().Contains("READY"))
                {
                    _statusLabel.Text = "✅ Ready — Listening now";
                    _statusLabel.ForeColor = Color.LightGreen;
                    return true;
                }

                _statusLabel.Text = "⚠ Unexpected reply: " + reply;
                _statusLabel.ForeColor = Color.OrangeRed;
                return false;
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "⚠ Check failed: " + ex.Message;
                _statusLabel.ForeColor = Color.OrangeRed;
                return false;
            }
        }

        private void TogglePin()
        {
            _pinned = !_pinned;
            this.TopMost = _pinned;
            _pinButton.Text = _pinned ? "📌 Pin" : "Pin";
            _pinButton.BackColor = _pinned ? AccentPinned : AccentNeutral;
        }

        // ================= Button factory =================
        private Button MakeButton(string text, int width, int height, Color bg)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Default,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 4, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = LightenColor(bg, 25);
            btn.FlatAppearance.MouseDownBackColor = DarkenColor(bg, 20);
            btn.Region = RoundedRegion(btn.Width, btn.Height, 6);
            btn.Resize += (s, e) => btn.Region = RoundedRegion(btn.Width, btn.Height, 6);
            return btn;
        }

        private static Region RoundedRegion(int w, int h, int radius)
        {
            if (w <= 0 || h <= 0) return new Region();
            var path = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(w, h));
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(w - d, 0, d, d, 270, 90);
            path.AddArc(w - d, h - d, d, d, 0, 90);
            path.AddArc(0, h - d, d, d, 90, 90);
            path.CloseAllFigures();
            return new Region(path);
        }

        private static Color LightenColor(Color c, int amount)
            => Color.FromArgb(c.A,
                Math.Min(255, c.R + amount),
                Math.Min(255, c.G + amount),
                Math.Min(255, c.B + amount));

        private static Color DarkenColor(Color c, int amount)
            => Color.FromArgb(c.A,
                Math.Max(0, c.R - amount),
                Math.Max(0, c.G - amount),
                Math.Max(0, c.B - amount));

        // ================= Theme =================
        private void ToggleTheme()
        {
            _lightMode = !_lightMode;
            _themeButton.Text = _lightMode ? "Dark" : "Light";
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = BgMain;
            this.ForeColor = FgText;

            _topPanel.BackColor = BgPanel;
            _inputPanel.BackColor = BgPanel;
            _statusLabel.ForeColor = FgSub;
            _glassLabel.ForeColor = FgSub;

            _transcriptBox.BackColor = _lightMode ? Color.FromArgb(245, 245, 250) : Color.FromArgb(25, 25, 30);
            _transcriptBox.ForeColor = FgTranscript;

            _answerPanel.BackColor = _lightMode ? Color.FromArgb(240, 240, 245) : Color.FromArgb(35, 35, 40);

            _inputBox.BackColor = BgInput;
            _inputBox.ForeColor = FgText;

            _pinButton.BackColor = _pinned ? AccentPinned : AccentNeutral;

            foreach (Control c in _answerPanel.Controls)
                ApplyThemeRecursive(c);
        }

        private void ApplyThemeRecursive(Control c)
        {
            if (c is RichTextBox rtb)
            {
                rtb.BackColor = BgAnswer;
                rtb.ForeColor = FgText;
            }
            else if (c is Label lbl)
            {
                lbl.ForeColor = lbl.Font.Bold
                    ? (_lightMode ? Color.DarkGoldenrod : Color.Gold)
                    : FgText;
            }
            else if (c is Panel p)
            {
                p.BackColor = BgCard;
                p.Region = RoundedRegion(p.Width, p.Height, 8);
            }

            foreach (Control child in c.Controls)
                ApplyThemeRecursive(child);
        }

        // ================= Font size =================
        private void AdjustFont(int delta)
        {
            _transcriptFontSize = Math.Max(8f, Math.Min(22f, _transcriptFontSize + delta));
            _answerFontSize = Math.Max(8f, Math.Min(22f, _answerFontSize + delta));
            _transcriptBox.Font = new Font("Segoe UI", _transcriptFontSize);

            foreach (Control c in _answerPanel.Controls)
                RelayoutCard(c);
        }

        private void RelayoutCard(Control card)
        {
            if (card is not Panel panel) return;
            int innerWidth = panel.Width - 24;
            int y = 10;

            Label? lblTime = null;
            Label? lblText = null;
            RichTextBox? answer = null;

            foreach (Control c in panel.Controls)
            {
                if (c is Label l1 && l1.Height == 14 && lblTime == null) lblTime = l1;
                else if (c is Label l2 && l2 != lblTime && lblText == null) lblText = l2;
                else if (c is RichTextBox r) answer = r;
            }

            if (lblTime != null) { lblTime.Width = innerWidth; lblTime.Top = y; y += 16; }

            if (lblText != null)
            {
                lblText.Width = innerWidth;
                lblText.Top = y;
                lblText.Font = new Font("Segoe UI", _answerFontSize);
                lblText.Height = TextRenderer.MeasureText(lblText.Text, lblText.Font,
                                    new Size(innerWidth, int.MaxValue),
                                    TextFormatFlags.WordBreak).Height;
                y += lblText.Height + 6;
            }

            if (answer != null)
            {
                answer.Width = innerWidth;
                answer.Top = y;
                answer.Font = new Font("Segoe UI", _answerFontSize);

                string existing = answer.Text;
                if (!string.IsNullOrEmpty(existing) && !existing.StartsWith("🤖 thinking"))
                    RenderAnswer(answer, existing);

                int newHeight = MeasureRichTextHeight(answer, innerWidth);
                if (newHeight > 380) newHeight = 380;
                answer.Height = newHeight;
                y += newHeight + 8;
            }

            panel.Height = y + 10;
        }

        // ================= File loading =================
        private void LoadFile_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select project file",
                Filter = "All supported|*.txt;*.md;*.pdf;*.docx|" +
                         "Text|*.txt|Markdown|*.md|PDF|*.pdf|Word|*.docx"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                _projectContext = FileLoader.LoadText(dlg.FileName);
                _statusLabel.Text = $"Loaded: {System.IO.Path.GetFileName(dlg.FileName)} " +
                                    $"({_projectContext.Length:N0} chars)";
            }
            catch (Exception ex)
            {
                _projectContext = "";
                _statusLabel.Text = "Load failed: " + ex.Message;
            }
        }

        // ================= Start / Stop =================
        private async System.Threading.Tasks.Task StartStop_ClickAsync()
        {
            if (AudioCaptureTest.IsRunning)
            {
                AudioCaptureTest.Stop();
                _startStopButton.Text = "Start";
                _startStopButton.BackColor = AccentGreen;
                _statusLabel.Text = "Listening: OFF";
                _statusLabel.ForeColor = FgSub;
                return;
            }

            _startStopButton.Text = "Stop";
            _startStopButton.BackColor = AccentRed;
            _statusLabel.Text = "🎤 Listening… speak or play audio now";
            _statusLabel.ForeColor = Color.LightGreen;
            _statusLabel.Refresh();
            _startStopButton.Refresh();
            Application.DoEvents();

            if (!_hasCheckedReadiness)
            {
                _hasCheckedReadiness = true;

                _startStopButton.Enabled = false;
                bool ok = await RunReadinessCheckAsync();
                _startStopButton.Enabled = true;

                if (!ok)
                {
                    _startStopButton.Text = "Start";
                    _startStopButton.BackColor = AccentGreen;
                    return;
                }

                _statusLabel.Text = "🎤 Listening… speak or play audio now";
                _statusLabel.ForeColor = Color.LightGreen;
                _statusLabel.Refresh();
                Application.DoEvents();
            }

            AudioCaptureTest.Start();
        }

        // ================= Typed question =================
        private async System.Threading.Tasks.Task SendTypedQuestionAsync()
        {
            string q = _inputBox.Text.Trim();
            if (q.Length == 0) return;

            _inputBox.Clear();
            string time = DateTime.Now.ToString("HH:mm:ss");
            _transcriptBox.AppendText($"[{time}] (typed) {q}\n\n");
            TrimTranscriptBox();
            _transcriptBox.ScrollToCaret();

            await CreateAnswerCardAsync(time, q, isTyped: true);
        }

        // ================= Voice transcript =================
        private void OnTranscriptReceived(string transcript)
        {
            if (this.IsDisposed) return;

            this.BeginInvoke(new Action(async () =>
            {
                if (QuestionDetector.IsFiller(transcript)) return;

                string time = DateTime.Now.ToString("HH:mm:ss");
                _transcriptBox.AppendText($"[{time}] {transcript}\n\n");
                TrimTranscriptBox();
                _transcriptBox.ScrollToCaret();

                if (!QuestionDetector.IsMeaningful(transcript)) return;

                await CreateAnswerCardAsync(time, transcript, isTyped: false);
            }));
        }

        private void TrimTranscriptBox()
        {
            try
            {
                if (_transcriptBox.Lines.Length > MaxTranscriptLines)
                {
                    var lines = _transcriptBox.Lines;
                    var keep = lines.Skip(lines.Length - TrimToTranscriptLines).ToArray();
                    _transcriptBox.Lines = keep;
                }
            }
            catch { }
        }

        private void TrimAnswerPanel()
        {
            try
            {
                while (_answerPanel.Controls.Count > MaxAnswerCards)
                {
                    var old = _answerPanel.Controls[0];
                    _answerPanel.Controls.RemoveAt(0);
                    old.Dispose();
                }
            }
            catch { }
        }

        // ================= Answer card =================
        private async System.Threading.Tasks.Task CreateAnswerCardAsync(
            string time, string transcript, bool isTyped)
        {
            var card = new Panel
            {
                Width = _answerPanel.ClientSize.Width - 24,
                AutoSize = false,
                BackColor = BgCard,
                Padding = new Padding(10),
                Margin = new Padding(4, 3, 4, 6),
                Cursor = Cursors.Default
            };
            card.Region = RoundedRegion(card.Width, 80, 8);
            card.Resize += (s, e) => card.Region = RoundedRegion(card.Width, card.Height, 8);

            int innerWidth = card.Width - 20;
            int y = 10;

            var lblTime = new Label
            {
                Text = isTyped ? $"[{time}]  (typed)" : $"[{time}]",
                ForeColor = FgSub,
                Font = new Font("Segoe UI", 8),
                AutoSize = false,
                Width = innerWidth,
                Height = 14,
                Left = 10,
                Top = y,
                Cursor = Cursors.Default,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTime);
            y += 16;

            var lblText = new Label
            {
                Text = transcript,
                ForeColor = FgText,
                Font = new Font("Segoe UI", _answerFontSize),
                AutoSize = false,
                Width = innerWidth,
                Left = 10,
                Top = y,
                Cursor = Cursors.Default,
                BackColor = Color.Transparent
            };
            lblText.Height = TextRenderer.MeasureText(lblText.Text, lblText.Font,
                                new Size(innerWidth, int.MaxValue),
                                TextFormatFlags.WordBreak).Height;
            card.Controls.Add(lblText);
            y += lblText.Height + 6;

            var answerBox = new RichTextBox
            {
                ReadOnly = true,
                BackColor = BgAnswer,
                ForeColor = FgText,
                Font = new Font("Segoe UI", _answerFontSize),
                BorderStyle = BorderStyle.None,
                Width = innerWidth,
                Left = 10,
                Top = y,
                Height = 36,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Text = "🤖 thinking...",
                Cursor = Cursors.Default,
                DetectUrls = false,
                WordWrap = true
            };
            card.Controls.Add(answerBox);

            card.Height = y + 44;
            _answerPanel.Controls.Add(card);
            TrimAnswerPanel();
            _answerPanel.ScrollControlIntoView(card);

            try
            {
                bool isQuestion = isTyped || QuestionDetector.IsQuestion(transcript);
                string systemPrompt = BuildSystemPrompt(isQuestion);
                string rawAnswer = await GroqClient.AskAsync(systemPrompt, transcript);

                RenderAnswer(answerBox, rawAnswer);

                int newHeight = MeasureRichTextHeight(answerBox, innerWidth);
                if (newHeight > 380) newHeight = 380;
                answerBox.Height = newHeight;
                card.Height = y + newHeight + 10;
            }
            catch (Exception ex)
            {
                answerBox.Clear();
                answerBox.SelectionColor = Color.OrangeRed;
                answerBox.AppendText("⚠ Error: " + ex.Message);
                answerBox.Height = 50;
                card.Height = y + 60;
            }

            _answerPanel.PerformLayout();
            _answerPanel.ScrollControlIntoView(card);
        }

        private void RenderAnswer(RichTextBox box, string rawAnswer)
        {
            box.Clear();
            box.Cursor = Cursors.Default;

            string text = rawAnswer.Trim();
            int firstBreak = text.IndexOfAny(new[] { '\n', '\r' });
            string mainLine = firstBreak < 0 ? text : text.Substring(0, firstBreak).Trim();
            string rest = firstBreak < 0 ? "" : text.Substring(firstBreak).TrimStart('\r', '\n');

            box.SelectionFont = new Font("Segoe UI", _answerFontSize, FontStyle.Bold);
            box.SelectionColor = _lightMode ? Color.DarkGoldenrod : Color.Gold;
            box.AppendText(mainLine + "\n\n");

            box.SelectionFont = new Font("Segoe UI", _answerFontSize, FontStyle.Regular);
            box.SelectionColor = FgText;
            box.AppendText(rest);
        }

        private int MeasureRichTextHeight(RichTextBox box, int width)
        {
            using var probe = new RichTextBox
            {
                WordWrap = true,
                Width = width,
                Font = box.Font,
                ScrollBars = RichTextBoxScrollBars.None,
                BorderStyle = BorderStyle.None
            };
            probe.Rtf = box.Rtf;
            var size = probe.GetPreferredSize(new Size(width, int.MaxValue));
            return size.Height + 10;
        }

        private string BuildSystemPrompt(bool isQuestion)
        {
            string formatRule =
                "Reply in this exact format:\n" +
                "Line 1: The main answer as a single short sentence.\n" +
                "Line 2: blank.\n" +
                "Then numbered steps (1. 2. 3.) giving details, or a short explanation. " +
                "If the answer involves code, include the code as plain text.";

            string baseRule = isQuestion
                ? "You are a helpful assistant. The user asks a question. Give a clear, detailed answer.\n\n" + formatRule
                : "You are a helpful assistant. The user says a statement. Give a short helpful response.\n\n" + formatRule;

            if (_projectContext.Length > 0)
                return baseRule + "\n\nUse the following project document as context when relevant:\n\n" + _projectContext;

            return baseRule;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AudioCaptureTest.Stop();
            AudioCaptureTest.OnTranscriptReceived -= OnTranscriptReceived;
            base.OnFormClosing(e);
        }
    }
}