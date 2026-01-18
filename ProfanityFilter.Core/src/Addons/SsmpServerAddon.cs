#if NETSTANDARD2_1
using System;
using SSMP.Api.Server;
using SSMP.Api.Eventing.ServerEvents;
using ProfanityFilterAddon.Services;

namespace ProfanityFilterAddon.Addons {
    /// <summary>
    /// SSMP Server Addon that filters profanity from player chat messages.
    /// Auto-discovered when placed alongside the SSMP server executable.
    /// </summary>
    public class SsmpServerAddon : ServerAddon {
        /// <summary>
        /// Display name shown in SSMP addon list.
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
        /// SSMP API version compatibility.
        /// </summary>
        public override uint ApiVersion => 1;

        /// <summary>
        /// Initializes the addon by subscribing to chat events.
        /// </summary>
        /// <param name="serverApi">The SSMP server API instance.</param>
        public override void Initialize(IServerApi serverApi) {
            Logger.Info("[ProfanityFilter] SSMP addon initialized.");
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
