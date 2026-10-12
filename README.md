# AI Hider — Private AI Assistant

A desktop AI assistant that stays **invisible during screen sharing** on Zoom, Google Meet, Microsoft Teams, Discord, and other platforms. It listens to system audio, transcribes speech, detects questions, and answers them with an AI model — all while remaining hidden from the shared screen on Windows 11.

![status](https://img.shields.io/badge/status-active-brightgreen)
![.NET](https://img.shields.io/badge/.NET-10-purple)
![C%23](https://img.shields.io/badge/C%23-WinForms-blue)
![License](https://img.shields.io/badge/license-MIT-green)

---

## ✨ Features

- 🎤 **Voice question detection** — listens to system audio, transcribes speech, and answers automatically
- ⌨️ **Typed questions** — type any question and get an instant AI answer
- 🙈 **Privacy-first** — window is excluded from screen capture (uses Windows `SetWindowDisplayAffinity` API)
- 📌 **Always-on-top pin** — keep the window above Zoom, Meet, Teams, or any other app
- 🎨 **Dark / Light theme** with adjustable window transparency (20 % – 100 %)
- 📄 **File context** — load PDF / DOCX / TXT / MD as background context for the AI
- ❓ **Built-in Help** — full in-app usage guide
- 🔤 **Font size control** — scale text up or down with one click
- ⚡ **Fast AI replies** — powered by Groq (Llama / GPT-OSS) + Whisper for speech-to-text

---

## 🛠️ Built With

| Component | Technology |
|-----------|-----------|
| **Language** | C# |
| **Framework** | .NET 10 (WinForms) |
| **AI Backend (chat)** | Groq API — Llama / GPT-OSS |
| **AI Backend (backup)** | Google Gemini |
| **Audio Capture** | NAudio (WASAPI loopback) |
| **Speech-to-Text** | Groq Whisper (`whisper-large-v3-turbo`) |
| **Privacy API** | `SetWindowDisplayAffinity` (Windows 10 2004+ / Windows 11) |
| **Installer** | Inno Setup |

---

## 🚀 Getting Started

### Prerequisites

- **Windows 10 (build 19041 or later)** or **Windows 11**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A **Groq API key** — free tier available at [console.groq.com/keys](https://console.groq.com/keys)

### Installation (for users)

1. Go to the [Releases page](../../releases).
2. Download `AIHider_Setup.exe` from the latest release.
3. Run the installer and follow the wizard.
4. Launch **AI Hider** from the Start Menu or desktop shortcut.

### Build from Source (for developers)

```bash
# Clone the repository
git clone https://github.com/amila-sandaruwan-git/AIHider.git
cd AIHider

# Set your Groq API key as an environment variable (Windows)
setx GROQ_API_KEY "your-key-here"

# Restart your terminal and open the project in Visual Studio 2022 or later
# Press F5 to build and run
