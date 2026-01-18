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
I am planning to introduce a **Transformer-based Language Model (LLM)** in future versions. This upgrade will provide:
- **Deep Contexual Understanding**: Detecting toxicity based on sentiment and intent rather than just keywords.
- **Sarcasm Detection**: Better handling of nuanced communication.
- **Adaptive Filtering**: Learning from server-specific chat patterns.

## 📦 Dependencies

- **HKMP** (Hollow Knight Multiplayer API) - *Required for HKMP servers*
- **SSMP** (Server Side Multiplayer API) - *Required for SSMP servers*
- **Newtonsoft.Json**
- **Microsoft.ML.OnnxRuntime** (Pre-integrated for future AI models)

## 📥 Installation

> [!NOTE]
> All versions use the unified filename `ProfanityFilterAddon.dll`. Ensure you download the correct ZIP for your platform.

### Option 1: Client (Lumafly)
1. Download `ProfanityFilterAddon-Client.zip`.
2. Extract to your `Mods/` folder (normally handled by Lumafly).

### Option 2: HKMP Standalone Server
1. Download `ProfanityFilterAddon-Server-HKMP.zip`.
2. Extract to `Mods/HKMP/` (or your server's mods folder).

### Option 3: SSMP Standalone Server
1. Download `ProfanityFilterAddon-Server-SSMP.zip`.
2. Place `ProfanityFilterAddon.dll` next to your SSMP Server executable.

## 🛠️ Configuration

The addon automatically extracts its configuration. You can modify the generated JSON file to customize:
- `profanity_words`: List of banned words.
- `ok_phrases`: Whitelist of allowed phrases.
- `bad_phrases`: Blacklist of prohibited phrases.
