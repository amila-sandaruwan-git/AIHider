// AudioCaptureTest.cs — captures system audio, sends to Groq Whisper.
// Fixes: static HttpClient, single long-running capture, faster thresholds.
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using NAudio.Wave;

namespace AIHider
{
    public static class AudioCaptureTest
    {
        // ⚠️ Your Groq key
        private const string GroqApiKey = "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";
        private const string WhisperModel = "whisper-large-v3-turbo";

        // ---- Timing / thresholds (Fix 4 + Fix 5) ----
        private const float RmsThreshold = 0.02f;
        private const int SilenceMsToEndChunk = 1200;   // was 2000 — faster chunk end
        private const int MinSpeechMs = 1000;           // was 2000 — shorter blips accepted
        private const int MaxChunkSeconds = 15;

        // ⭐ Fix 1 — single shared HttpClient for the whole session
        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        // ⭐ Fix 2 — single long-running capture device
        private static WasapiLoopbackCapture? _capture;
        private static WaveFormat? _captureFormat;

        private static bool _running = false;

        public static event Action<string>? OnTranscriptReceived;

        public static bool IsRunning => _running;

        public static void Start()
        {
            if (_running) return;
            _running = true;

            var thread = new System.Threading.Thread(RunLoop);
            thread.IsBackground = true;
            thread.Priority = System.Threading.ThreadPriority.AboveNormal;
            thread.Start();
        }

        public static void Stop()
        {
            _running = false;

            // Stop the shared capture as well
            try
            {
                _capture?.StopRecording();
            }
            catch { }
        }

        // ============================================================
        //  MAIN LOOP  (Fix 2 — single capture reused across chunks)
        // ============================================================
        private static void RunLoop()
        {
            try
            {
                // ⭐ One device for the whole session
                _capture = new WasapiLoopbackCapture();
                _captureFormat = _capture.WaveFormat;
                _capture.StartRecording();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Capture init error: " + ex.Message);
                _running = false;
                return;
            }

            while (_running)
            {
                try
                {
                    byte[] chunkPcm = CaptureOneChunk();
                    if (chunkPcm == null || chunkPcm.Length == 0)
                        continue;

                    byte[] wavBytes = BuildWav(chunkPcm, _captureFormat!);
                    string transcript = TranscribeAsync(wavBytes, "chunk.wav")
                                        .GetAwaiter().GetResult();

                    if (IsRealSpeech(transcript))
                    {
                        OnTranscriptReceived?.Invoke(transcript.Trim());
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Capture error: " + ex.Message);
                }
            }

            try { _capture?.StopRecording(); } catch { }
            try { _capture?.Dispose(); } catch { }
            _capture = null;
        }

        // ============================================================
        //  CAPTURE ONE CHUNK  — reads from the SHARED capture
        // ============================================================
        private static byte[] CaptureOneChunk()
        {
            var fmt = _captureFormat!;

            bool inSpeech = false;
            DateTime speechStart = DateTime.MinValue;
            DateTime lastLoudTime = DateTime.Now;
            var speechBuffer = new MemoryStream();
            var lockObj = new object();

            // Temporary handler — attached only while collecting this chunk
            EventHandler<WaveInEventArgs> handler = (sender, e) =>
            {
                float rms = ComputeRms(e.Buffer, e.BytesRecorded, fmt);
                bool isLoud = rms >= RmsThreshold;
                DateTime now = DateTime.Now;

                lock (lockObj)
                {
                    if (isLoud)
                    {
                        if (!inSpeech)
                        {
                            inSpeech = true;
                            speechStart = now;
                            speechBuffer.SetLength(0);
                        }
                        lastLoudTime = now;
                        speechBuffer.Write(e.Buffer, 0, e.BytesRecorded);
                    }
                    else if (inSpeech)
                    {
                        speechBuffer.Write(e.Buffer, 0, e.BytesRecorded);

                        double silenceSec = (now - lastLoudTime).TotalSeconds;
                        double speechSec = (now - speechStart).TotalSeconds;

                        bool silenceEnded = silenceSec >= SilenceMsToEndChunk / 1000.0;
                        bool tooLong = speechSec >= MaxChunkSeconds;

                        if (silenceEnded || tooLong)
                            inSpeech = false;
                    }
                }
            };

            _capture!.DataAvailable += handler;

            // Wait until a chunk completes, or _running is false
            var hardTimeout = DateTime.Now.AddSeconds(MaxChunkSeconds + 5);
            while (_running && DateTime.Now < hardTimeout)
            {
                System.Threading.Thread.Sleep(30);

                lock (lockObj)
                {
                    if (!inSpeech && speechBuffer.Length > 0)
                        break;
                }
            }

            _capture.DataAvailable -= handler;

            double durationSec;
            lock (lockObj)
            {
                durationSec = speechBuffer.Length /
                    (double)(fmt.AverageBytesPerSecond == 0 ? 1 : fmt.AverageBytesPerSecond);

                if (durationSec * 1000 < MinSpeechMs)
                    return Array.Empty<byte>();

                return speechBuffer.ToArray();
            }
        }

        // ============================================================
        //  RMS (volume) CALCULATION
        // ============================================================
        private static float ComputeRms(byte[] buffer, int bytesRecorded, WaveFormat fmt)
        {
            if (bytesRecorded == 0) return 0f;

            double sumSq = 0;
            int samples = 0;

            if (fmt.Encoding == WaveFormatEncoding.IeeeFloat && fmt.BitsPerSample == 32)
            {
                int count = bytesRecorded / 4;
                for (int i = 0; i < count; i++)
                {
                    float s = BitConverter.ToSingle(buffer, i * 4);
                    sumSq += (double)s * s;
                    samples++;
                }
            }
            else if (fmt.BitsPerSample == 16)
            {
                int count = bytesRecorded / 2;
                for (int i = 0; i < count; i++)
                {
                    short s = BitConverter.ToInt16(buffer, i * 2);
                    double f = s / 32768.0;
                    sumSq += f * f;
                    samples++;
                }
            }
            else
            {
                return 0f;
            }

            if (samples == 0) return 0f;
            return (float)Math.Sqrt(sumSq / samples);
        }

        // ============================================================
        //  FILTER
        // ============================================================
        private static bool IsRealSpeech(string? transcript)
        {
            if (string.IsNullOrWhiteSpace(transcript)) return false;

            string t = transcript.Trim();
            string lower = t.ToLowerInvariant();

            string[] junk = { "[music]", "[blank_audio]", "[silence]", "[no speech]",
                              "(music)", "(silence)", "[inaudible]", "[applause]" };
            foreach (var j in junk)
                if (lower.Contains(j)) return false;

            if (t.Length < 3) return false;

            int letters = 0;
            foreach (char c in t) if (char.IsLetter(c)) letters++;
            if (letters < 3) return false;

            return true;
        }

        // ============================================================
        //  WAV BUILDER
        // ============================================================
        private static byte[] BuildWav(byte[] pcm, WaveFormat format)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            ushort audioFormat = (ushort)(
                format.Encoding == WaveFormatEncoding.IeeeFloat ? 3 : 1);

            int channels = format.Channels;
            int sampleRate = format.SampleRate;
            int bitsPerSample = format.BitsPerSample;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);

            bw.Write(new[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + pcm.Length);
            bw.Write(new[] { 'W', 'A', 'V', 'E' });

            bw.Write(new[] { 'f', 'm', 't', ' ' });
            bw.Write(16);
            bw.Write(audioFormat);
            bw.Write((short)channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write((short)bitsPerSample);

            bw.Write(new[] { 'd', 'a', 't', 'a' });
            bw.Write(pcm.Length);
            bw.Write(pcm);

            return ms.ToArray();
        }

        // ============================================================
        //  GROQ WHISPER  (Fix 1 — uses shared Http)
        // ============================================================
        private static async System.Threading.Tasks.Task<string> TranscribeAsync(
            byte[] wavBytes, string fileName)
        {
            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(wavBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
            form.Add(fileContent, "file", fileName);

            form.Add(new StringContent(WhisperModel), "model");
            form.Add(new StringContent("en"), "language");

            using var req = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/audio/transcriptions");
            req.Headers.Add("Authorization", "Bearer " + GroqApiKey);
            req.Content = form;

            using var resp = await Http.SendAsync(req);       // ⭐ shared
            string text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)resp.StatusCode}: {text}");

            using var doc = JsonDocument.Parse(text);
            return doc.RootElement.GetProperty("text").GetString() ?? "";
        }
    }
}