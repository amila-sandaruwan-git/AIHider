using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIHider
{
    public static class GeminiClient
    {
        // ⚠️ Paste your Gemini API key here
        private const string ApiKey = "";

        private const string Model = "gemini-3.8-flash";

        // Two different endpoints are needed
        private const string UploadEndpoint = "https://generativelanguage.googleapis.com/upload/v1beta/files";
        private const string InteractionEndpoint = "https://generativelanguage.googleapis.com/v1beta/interactions";

        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)  // uploads can be large
        };

        // ------------------------------------------------------------------
        // FILE UPLOAD
        // Returns a "file URI" you can reference in later questions.
        // ------------------------------------------------------------------
        public static async Task<string> UploadFileAsync(string localFilePath)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("File not found: " + localFilePath);

            string fileName = Path.GetFileName(localFilePath);
            string mimeType = GuessMimeType(localFilePath);

            // Step 1: Start a resumable upload session
            // We tell the server the total size and get back a session URL.
            long fileSize = new FileInfo(localFilePath).Length;

            var startBody = new
            {
                file = new { display_name = fileName }
            };

            using var startReq = new HttpRequestMessage(HttpMethod.Post, UploadEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(startBody),
                                            Encoding.UTF8, "application/json")
            };
            startReq.Headers.Add("x-goog-api-key", ApiKey);
            startReq.Headers.Add("X-Goog-Upload-Protocol", "resumable");
            startReq.Headers.Add("X-Goog-Upload-Command", "start");
            startReq.Headers.Add("X-Goog-Upload-Header-Content-Length", fileSize.ToString());
            startReq.Headers.Add("X-Goog-Upload-Header-Content-Type", mimeType);

            using var startResp = await _http.SendAsync(startReq);
            string startRespText = await startResp.Content.ReadAsStringAsync();

            if (!startResp.IsSuccessStatusCode)
                throw new Exception($"Upload start failed: HTTP {(int)startResp.StatusCode}: {startRespText}");

            // The session URL is returned in a header
            if (!startResp.Headers.TryGetValues("X-Goog-Upload-URL", out var uploadUrls))
                throw new Exception("No upload URL returned. Response: " + startRespText);

            string uploadUrl = null;
            foreach (var u in uploadUrls) { uploadUrl = u; break; }

            // Step 2: Upload the actual bytes
            byte[] fileBytes = await File.ReadAllBytesAsync(localFilePath);

            using var uploadReq = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
            {
                Content = new ByteArrayContent(fileBytes)
            };
            uploadReq.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            uploadReq.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
            uploadReq.Headers.Add("X-Goog-Upload-Offset", "0");

            using var uploadResp = await _http.SendAsync(uploadReq);
            string uploadRespText = await uploadResp.Content.ReadAsStringAsync();

            if (!uploadResp.IsSuccessStatusCode)
                throw new Exception($"Upload failed: HTTP {(int)uploadResp.StatusCode}: {uploadRespText}");

            // The response JSON contains file.uri — the ID we use later
            using var doc = JsonDocument.Parse(uploadRespText);
            var uri = doc.RootElement.GetProperty("file").GetProperty("uri").GetString();

            return uri;  // e.g. "https://generativelanguage.googleapis.com/v1beta/files/abc123"
        }

        // ------------------------------------------------------------------
        // ASK A QUESTION (with optional file reference)
        // ------------------------------------------------------------------
        public static async Task<string> AskAsync(string prompt, string fileUri = null)
        {
            object input;

            if (string.IsNullOrEmpty(fileUri))
            {
                // Text-only question
                input = prompt;
            }
            else
            {
                // Question that references an uploaded file
                input = new object[]
                {
                    new { type = "text", text = prompt },
                    new { type = "file", uri = fileUri }
                };
            }

            var body = new { model = Model, input = input };
            string json = JsonSerializer.Serialize(body);

            using var req = new HttpRequestMessage(HttpMethod.Post, InteractionEndpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("x-goog-api-key", ApiKey);

            using var resp = await _http.SendAsync(req);
            string respText = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)resp.StatusCode}: {respText}");

            // Parse the response for the first model_output text
            using var doc = JsonDocument.Parse(respText);
            if (!doc.RootElement.TryGetProperty("steps", out var steps))
                throw new Exception("No 'steps' in response: " + respText);

            foreach (var step in steps.EnumerateArray())
            {
                if (!step.TryGetProperty("type", out var t)) continue;
                if (t.GetString() != "model_output") continue;

                if (!step.TryGetProperty("content", out var contentEl)) continue;

                foreach (var part in contentEl.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var textEl))
                    {
                        var text = textEl.GetString();
                        if (!string.IsNullOrEmpty(text)) return text;
                    }
                }
            }

            return "(No text found in response)";
        }

        // ------------------------------------------------------------------
        // Helper: guess MIME type from file extension
        // ------------------------------------------------------------------
        private static string GuessMimeType(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".pdf": return "application/pdf";
                case ".txt": return "text/plain";
                case ".md": return "text/markdown";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".mp3": return "audio/mp3";
                case ".wav": return "audio/wav";
                default: return "application/octet-stream";
            }
        }
    }
}
