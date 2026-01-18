#if NET472
using System;
using Hkmp.Api.Server;
using Hkmp.Api.Eventing.ServerEvents;

namespace ProfanityFilterAddon {
    /// <summary>
    /// Profanity Filter addon for HKMP.
    /// </summary>
    public class HKMPAddon : ServerAddon {
        /// <inheritdoc />
        protected override string Name => "Profanity Filter";
        
        /// <inheritdoc />
        protected override string Version => "1.0.0";
        
        /// <inheritdoc />
        public override bool NeedsNetwork => false;

        /// <inheritdoc />
        public override void Initialize(IServerApi serverApi) {
            Logger.Info("[ProfanityFilterAddon] HKMP Addon initialized.");
            serverApi.ServerManager.PlayerChatEvent += OnPlayerChat;
        }

        /// <summary>
        /// Event handler for player chat messages.
        /// Filters profanity from the message content.
        /// </summary>
        /// <param name="chatEvent">The chat event arguments.</param>
        private void OnPlayerChat(IPlayerChatEvent chatEvent) {
            var original = chatEvent.Message;
            var sanitized = ProfanityFilter.Sanitize(original);
            
            if (!string.Equals(sanitized, original, StringComparison.Ordinal)) {
                chatEvent.Message = sanitized;
                Logger.Info("[ProfanityFilterAddon] Filtered message.");
            }
        }
    }
}
#endif
