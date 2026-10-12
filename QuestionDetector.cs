using System;

namespace AIHider
{
    public static class QuestionDetector
    {
        // Filler words — if the WHOLE transcript is one of these, skip it entirely
        private static readonly string[] Fillers = new[]
        {
            "okay", "ok", "yeah", "yep", "yes", "no", "nope",
            "hmm", "hmmm", "uh", "uhh", "um", "umm",
            "hi", "hello", "hey", "thanks", "thank you",
            "bye", "goodbye", "right", "sure", "alright", "cool"
        };

        // Starting words that indicate a question
        private static readonly string[] QuestionStarters = new[]
        {
            "what", "how", "why", "when", "where", "who", "which",
            "can you", "could you", "would you", "will you",
            "is ", "are ", "am ", "do ", "does ", "did ",
            "should ", "shall ", "may ", "might ",
            "tell me", "explain", "describe", "define",
            "give me", "show me", "list "
        };

        /// <summary>
        /// True if the text is ONLY a filler word ("okay", "yeah", etc.).
        /// Those should not even show a Yes/No button.
        /// </summary>
        public static bool IsFiller(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;

            string cleaned = text.Trim().TrimEnd('.', '!', ',', '?', ' ').ToLowerInvariant();

            foreach (var f in Fillers)
                if (cleaned == f) return true;

            // 1–2 word phrases that are just fillers
            if (cleaned.Length <= 4 && cleaned.IndexOf(' ') < 0)
                return true;

            return false;
        }

        /// <summary>
        /// True if the text looks like a question — gets a longer, detailed answer.
        /// </summary>
        public static bool IsQuestion(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            string trimmed = text.Trim();
            if (trimmed.EndsWith("?")) return true;

            string lower = trimmed.ToLowerInvariant();
            foreach (var starter in QuestionStarters)
                if (lower.StartsWith(starter)) return true;

            return false;
        }

        /// <summary>
        /// True if the text has enough words to be worth asking about.
        /// Filler-only text returns false.
        /// </summary>
        public static bool IsMeaningful(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (IsFiller(text)) return false;

            int wordCount = text.Split(new[] { ' ', '\t', '\n' },
                                       StringSplitOptions.RemoveEmptyEntries).Length;
            return wordCount >= 2;
        }
    }
}