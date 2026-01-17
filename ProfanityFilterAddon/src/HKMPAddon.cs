#if NET472
using System;
using Hkmp.Api.Server;
using Hkmp.Api.Eventing.ServerEvents;

namespace ProfanityFilterAddon {
    /// <summary>
    /// Profanity Filter addon for HKMP.
    /// Requires manual registration in your mod initialization.
    /// </summary>
    public class HKMPAddon : ServerAddon {
        protected override string Name => "Profanity Filter";
        protected override string Version => "1.0.0";
        public override bool NeedsNetwork => false;

        public override void Initialize(IServerApi serverApi) {
            Logger.Info("[ProfanityFilterAddon] HKMP Addon initialized.");
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

        /// <summary>
        /// Call this from your HKMP mod initialization to register this addon.
        /// </summary>
        public static void Register() {
            RegisterAddon(new HKMPAddon());
        }
    }
}
#endif
