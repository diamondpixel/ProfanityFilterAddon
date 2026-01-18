#if NET472
using System;
using Hkmp.Api.Server;
using Hkmp.Api.Eventing.ServerEvents;
using ProfanityFilterAddon.Services;

namespace ProfanityFilterAddon.Addons {
    /// <summary>
    /// HKMP Server Addon that filters profanity from player chat messages.
    /// Can be auto-discovered in Mods/HKMP or manually registered via <see cref="ProfanityFilterMod"/>.
    /// </summary>
    public class HkmpServerAddon : ServerAddon {
        /// <summary>
        /// Display name shown in HKMP addon list.
        /// </summary>
        protected override string Name => "Profanity Filter";
        
        /// <summary>
        /// Current addon version.
        /// </summary>
        protected override string Version => "1.1.0";
        
        /// <summary>
        /// This addon doesn't require network communication.
        /// </summary>
        public override bool NeedsNetwork => false;

        /// <summary>
        /// Initializes the addon by subscribing to chat events.
        /// </summary>
        /// <param name="serverApi">The HKMP server API instance.</param>
        public override void Initialize(IServerApi serverApi) {
            Logger.Info("[ProfanityFilter] HKMP addon initialized.");
            serverApi.ServerManager.PlayerChatEvent += OnPlayerChat;
        }

        private void OnPlayerChat(IPlayerChatEvent chatEvent) {
            var (sanitized, wasFiltered) = ChatMessageHandler.ProcessMessage(
                chatEvent.Message, 
                msg => Logger.Info(msg));
            
            if (wasFiltered) {
                chatEvent.Message = sanitized;
            }
        }
    }
}
#endif
