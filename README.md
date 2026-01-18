# ProfanityFilterAddon

A robust, server-side profanity filter addon designed for **HKMP** (Hollow Knight Multiplayer) and **SSMP**.

## 🚀 Features

- **Multi-Framework Support**: Works seamlessly with both HKMP and SSMP.
- **High-Performance Filtering**: Minimal impact on server latency.
- **Smart Detection**:
  - **Leet Speak Normalization**: Detects obfuscated words like `h3ll0` or `f@ck`.
  - **Context Awareness**: Distinguishes between casual use (e.g., "hell yeah") and directed insults.
  - **False Positive Protection**: Handles the "Scunthorpe problem" to avoid censoring innocent words (e.g., "glass", "assess").
- **Configurable**: Uses an internal JSON-based word list that can be easily updated.

## 🧠 Model & Technology

### Current Architecture
The current version utilizes a **High-Performance Rule-Based Engine**. It employs:
- Compiled Regular Expressions for efficient pattern matching.
- Dictionary-based lookups for fast categorization.
- Heuristic algorithms for phrase context analysis.

This ensures extremely fast processing times, making it ideal for real-time chat moderation on busy servers.

### 🔮 Future Roadmap
We are planning to introduce a **Transformer-based Language Model (LLM)** in future versions. This upgrade will provide:
- **Deep Contexual Understanding**: Detecting toxicity based on sentiment and intent rather than just keywords.
- **Sarcasm Detection**: Better handling of nuanced communication.
- **Adaptive Filtering**: Learning from server-specific chat patterns.

## 📦 Dependencies

- **HKMP** (Hollow Knight Multiplayer API) - *Required for HKMP servers*
- **SSMP** (Server Side Multiplayer API) - *Required for SSMP servers*
- **Newtonsoft.Json**
- **Microsoft.ML.OnnxRuntime** (Pre-integrated for future AI models)

## 📥 Installation

1. Download the latest release from the [Releases Page](https://github.com/diamondpixel/ProfanityFilterAddon/releases).
2. Place the `ProfanityFilterAddon.dll` into your server's `Addons` or `Mods` folder.
3. Restart the server.

## 🛠️ Configuration

The addon automatically extracts its configuration. You can modify the generated JSON file to customize:
- `profanity_words`: List of banned words.
- `ok_phrases`: Whitelist of allowed phrases.
- `bad_phrases`: Blacklist of prohibited phrases.
