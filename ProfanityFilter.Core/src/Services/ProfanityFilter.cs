using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ProfanityFilterAddon.Services {
    /// <summary>
    /// High-performance rule-based profanity filter with leet speak normalization and phrase detection.
    /// Uses word lists from embedded JSON resource for content moderation.
    /// </summary>
    public static class ProfanityFilter {
        /// <summary>
        /// Embedded resource name for the profanity filter JSON configuration file.
        /// </summary>
        private const string FilterResourceName = "ProfanityFilterAddon.profanity_filter.json";
        
        /// <summary>
        /// Flag indicating whether the filter data has been loaded from resources.
        /// </summary>
        private static bool _isLoaded;
        
        /// <summary>
        /// Set of profanity words to detect (case-insensitive).
        /// </summary>
        private static HashSet<string> _profanityWords = [];
        
        /// <summary>
        /// Set of mild words that are acceptable in certain contexts (case-insensitive).
        /// </summary>
        private static HashSet<string> _mildOkWords = [];
        
        /// <summary>
        /// Set of phrases that are always acceptable even if containing profanity words (case-insensitive).
        /// </summary>
        private static HashSet<string> _okPhrases = [];
        
        /// <summary>
        /// Set of phrases that are always considered offensive/insulting (case-insensitive).
        /// </summary>
        private static HashSet<string> _badPhrases = [];
        
        /// <summary>
        /// Set of false positive words that should not trigger profanity detection (Scunthorpe problem).
        /// </summary>
        private static HashSet<string> _falsePositives = [];
        
        /// <summary>
        /// Mapping of leet speak characters to their normalized equivalents (e.g., '3' -> 'e').
        /// </summary>
        private static Dictionary<char, char> _leetMap = new();

        /// <summary>
        /// Compiled regex for extracting words from text (matches word boundaries).
        /// </summary>
        private static readonly Regex WordRegex = new(@"\b\w+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        /// <summary>
        /// Compiled regex for normalizing dotted patterns like "f.u.c.k" to "fuck" (4-letter words).
        /// </summary>
        private static readonly Regex DottedPattern4 = new(@"\b([a-z])\.([a-z])\.([a-z])\.([a-z])\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        /// <summary>
        /// Compiled regex for normalizing dotted patterns like "f.u.c" to "fuc" (3-letter words).
        /// </summary>
        private static readonly Regex DottedPattern3 = new(@"\b([a-z])\.([a-z])\.([a-z])\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Data transfer object for deserializing the JSON filter configuration.
        /// </summary>
        private sealed class FilterData {
            /// <summary>
            /// List of profanity words to detect.
            /// </summary>
            [JsonProperty("profanity_words")] public List<string>? ProfanityWords { get; set; }
            
            /// <summary>
            /// List of mild words acceptable in certain contexts.
            /// </summary>
            [JsonProperty("mild_ok_words")] public List<string>? MildOkWords { get; set; }
            
            /// <summary>
            /// List of phrases that are always acceptable.
            /// </summary>
            [JsonProperty("ok_phrases")] public List<string>? OkPhrases { get; set; }
            
            /// <summary>
            /// List of phrases that are always considered offensive.
            /// </summary>
            [JsonProperty("bad_phrases")] public List<string>? BadPhrases { get; set; }
            
            /// <summary>
            /// List of false positive words to exclude from detection.
            /// </summary>
            [JsonProperty("false_positives")] public List<string>? FalsePositives { get; set; }
            
            /// <summary>
            /// Dictionary mapping leet speak characters to normalized characters.
            /// </summary>
            [JsonProperty("leet_map")] public Dictionary<string, string>? LeetMap { get; set; }
        }

        /// <summary>
        /// Ensures filter data is loaded from embedded resources. Loads only once per application lifetime.
        /// Falls back to hardcoded word list if resource loading fails.
        /// </summary>
        private static void EnsureLoaded() {
            if (_isLoaded) return;
            _isLoaded = true;

            try {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream(FilterResourceName);

                if (stream == null) {
                    Console.WriteLine($"[ProfanityAddon] Resource not found: {FilterResourceName}");
                    InitializeFallback();
                    return;
                }

                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<FilterData>(json);

                if (data != null) {
                    _profanityWords = new HashSet<string>(data.ProfanityWords ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                    _mildOkWords = new HashSet<string>(data.MildOkWords ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                    _okPhrases = new HashSet<string>(data.OkPhrases ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                    _badPhrases = new HashSet<string>(data.BadPhrases ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                    _falsePositives = new HashSet<string>(data.FalsePositives ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                    
                    if (data.LeetMap != null) {
                        foreach (var kvp in data.LeetMap) {
                            if (kvp.Key.Length == 1 && kvp.Value.Length == 1) {
                                _leetMap[kvp.Key[0]] = kvp.Value[0];
                            }
                        }
                    }
                    
                    Console.WriteLine($"[ProfanityAddon] Loaded {_profanityWords.Count} profanity words.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"[ProfanityAddon] Failed to load: {ex.Message}");
                InitializeFallback();
            }
        }

        /// <summary>
        /// Initializes filter with minimal hardcoded word list when JSON resource fails to load.
        /// Includes common profanity and basic leet speak mappings.
        /// </summary>
        private static void InitializeFallback() {
            _profanityWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "fuck", "shit", "bitch", "cunt", "nigger", "dick", "pussy", "asshole", "cock", "faggot"
            };
            _leetMap = new Dictionary<char, char> {
                {'0', 'o'}, {'1', 'i'}, {'3', 'e'}, {'4', 'a'}, {'5', 's'},
                {'7', 't'}, {'8', 'b'}, {'@', 'a'}, {'$', 's'}, {'!', 'i'}
            };
            Console.WriteLine("[ProfanityAddon] Using fallback word list.");
        }

        /// <summary>
        /// Normalizes leet speak characters in text to standard letters.
        /// Example: "h3ll0" becomes "hello", "f@ck" becomes "fack".
        /// </summary>
        /// <param name="text">Text containing potential leet speak.</param>
        /// <returns>Normalized text with leet speak converted to standard characters.</returns>
        private static string NormalizeLeet(string text) {
            var chars = text.ToCharArray();
            for (var i = 0; i < chars.Length; i++) {
                var c = char.ToLowerInvariant(chars[i]);
                if (_leetMap.TryGetValue(c, out var normalized)) {
                    chars[i] = normalized;
                }
            }
            return new string(chars);
        }

        /// <summary>
        /// Extracts individual words from text, handling dotted obfuscation patterns.
        /// Converts patterns like "f.u.c.k" to "fuck" before word extraction.
        /// </summary>
        /// <param name="text">Input text to extract words from.</param>
        /// <returns>List of extracted words in lowercase.</returns>
        private static List<string> ExtractWords(string text) {
            var cleaned = DottedPattern4.Replace(text, "$1$2$3$4");
            cleaned = DottedPattern3.Replace(cleaned, "$1$2$3");
            
            return WordRegex.Matches(cleaned.ToLowerInvariant())
                           .Cast<Match>()
                           .Select(m => m.Value)
                           .ToList();
        }

        /// <summary>
        /// Checks if text contains profanity using multi-stage detection:
        /// 1. Bad phrases (directed insults)
        /// 2. OK phrases (acceptable contexts)
        /// 3. False positives (Scunthorpe problem)
        /// 4. Profanity words (exact and leet speak normalized)
        /// 5. Compound word detection
        /// </summary>
        /// <param name="text">Text to analyze for profanity.</param>
        /// <returns>
        /// Tuple containing:
        /// - isProfane: True if profanity detected
        /// - probability: Confidence score (0.0-1.0)
        /// - reason: Explanation of detection result
        /// </returns>
        public static (bool isProfane, double probability, string reason) Check(string text) {
            EnsureLoaded();
            
            if (string.IsNullOrWhiteSpace(text)) {
                return (false, 0.0, "Empty");
            }

            var textLower = text.ToLowerInvariant();

            // Stage 1: Check bad phrases first (highest priority)
            foreach (var phrase in _badPhrases) {
                if (textLower.Contains(phrase)) {
                    return (true, 0.95, $"Insult: {phrase}");
                }
            }

            // Stage 2: Check OK phrases (override profanity detection)
            foreach (var phrase in _okPhrases) {
                if (textLower.Contains(phrase)) {
                    return (false, 0.1, $"OK: {phrase}");
                }
            }

            // Stage 3: Extract and check words
            var wordsOriginal = ExtractWords(text);

            // Stage 4: Check false positives (Scunthorpe problem)
            if (wordsOriginal.Any(word => _falsePositives.Contains(word))) {
                var safeWord = wordsOriginal.First(word => _falsePositives.Contains(word));
                return (false, 0.05, $"Safe: {safeWord}");
            }

            // Stage 5: Leet speak normalization
            var normalizedText = NormalizeLeet(textLower);
            var wordsNormalized = ExtractWords(normalizedText);
            var allWords = wordsOriginal.Concat(wordsNormalized).Distinct().ToList();

            // Stage 6: Profanity detection
            foreach (var word in allWords) {
                if (word.Length < 3) continue;

                // Exact match - severe profanity
                if (_profanityWords.Contains(word)) {
                    return (true, 0.85, $"Profanity: {word}");
                }
                
                // Mild words - flagged with lower severity (won't be censored by default)
                if (_mildOkWords.Contains(word)) {
                    return (true, 0.40, $"Mild: {word}");
                }

                // Compound word detection (min length 5)
                if (word.Length >= 5) {
                    foreach (var profWord in _profanityWords) {
                        if (profWord.Length >= 4 && word.Contains(profWord) && !_falsePositives.Contains(word)) {
                            return (true, 0.85, $"Profanity: {word}→{profWord}");
                        }
                    }
                }
            }

            return (false, 0.05, "Clean");
        }

        /// <summary>
        /// Sanitizes input text by censoring detected profanity with asterisks.
        /// Performs word-by-word analysis and replaces offensive words with '*' characters.
        /// </summary>
        /// <param name="text">Text to sanitize.</param>
        /// <returns>Sanitized text with profanity words replaced by asterisks.</returns>
        public static string Sanitize(string text) {
            if (string.IsNullOrWhiteSpace(text)) return text;

            var (isProfane, _, _) = Check(text);
            if (!isProfane) return text;

            // Word-by-word censorship
            var words = text.Split(' ');
            for (int i = 0; i < words.Length; i++) {
                var (wordIsProfane, wordProb, _) = Check(words[i]);
                if (wordIsProfane && wordProb > 0.5) {
                    words[i] = new string('*', words[i].Length);
                }
            }

            return string.Join(" ", words);
        }
    }
}