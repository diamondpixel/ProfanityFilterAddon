#if NETSTANDARD2_1
using System;
using SSMP.Api.Server;
using SSMP.Api.Eventing.ServerEvents;

namespace ProfanityFilterAddon {
    /// <summary>
    /// Profanity Filter addon for SSMP.
    /// </summary>
    public class SSMPAddon : ServerAddon {
        protected override string Name => "Profanity Filter";
        protected override string Version => "1.0.0";
        public override bool NeedsNetwork => false;
        public override uint ApiVersion => 1;

        public override void Initialize(IServerApi serverApi) {
            Logger.Info("[ProfanityFilterAddon] SSMP Addon initialized.");
            serverApi.ServerManager.PlayerChatEvent += OnPlayerChat;
        }

        private void OnPlayerChat(IPlayerChatEvent chatEvent) {
            var original = chatEvent.Message;
            var sanitized = ProfanityFilter.Sanitize(original);
            
            if (!string.Equals(sanitized, original, StringComparison.Ordinal)) {
                chatEvent.Message = sanitized;
                Logger.Info("[ProfanityAddon] Filtered message.");
            }
        }
    }
}
#endif
