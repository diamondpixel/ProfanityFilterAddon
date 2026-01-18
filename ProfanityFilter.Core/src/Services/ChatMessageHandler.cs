using System;

namespace ProfanityFilterAddon.Services {
    /// <summary>
    /// Shared chat message handler for all server addon implementations.
    /// Provides a unified way to filter profanity from chat messages.
    /// </summary>
    public static class ChatMessageHandler {
        private const string LogPrefix = "[ProfanityFilter]";
        
        /// <summary>
        /// Processes a chat message and returns the sanitized version.
        /// </summary>
        /// <param name="originalMessage">The original message to process.</param>
        /// <param name="logAction">Optional logging action for filtered messages.</param>
        /// <returns>
        /// A tuple containing the sanitized message and whether it was modified.
        /// </returns>
        public static (string sanitized, bool wasFiltered) ProcessMessage(
            string originalMessage, 
            Action<string>? logAction = null) {
            
            var sanitized = ProfanityFilter.Sanitize(originalMessage);
            var wasFiltered = !string.Equals(sanitized, originalMessage, StringComparison.Ordinal);
            
            if (wasFiltered) {
                logAction?.Invoke($"{LogPrefix} Filtered message.");
            }
            
            return (sanitized, wasFiltered);
        }
    }
}
