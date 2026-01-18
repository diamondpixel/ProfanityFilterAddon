#if NET472
using System;
using System.IO;
using System.Reflection;
using Modding;
using Hkmp.Api.Server;
using ProfanityFilterAddon.Addons;

namespace ProfanityFilterAddon {
    /// <summary>
    /// Hollow Knight Modding API entry point for Lumafly installation.
    /// Manually registers the HKMP addon when installed in a separate mod folder.
    /// </summary>
    /// <remarks>
    /// When installed via Lumafly, mods are placed in their own folders (e.g., Mods/ProfanityFilter/).
    /// HKMP only auto-discovers addons in Mods/HKMP/, so this class handles manual registration.
    /// If the DLL is placed directly in Mods/HKMP/, registration is skipped to prevent duplication.
    /// </remarks>
    public class ProfanityFilterMod : Mod {
        /// <summary>
        /// Current mod version.
        /// </summary>
        public override string GetVersion() => "1.1.0";

        /// <summary>
        /// Initializes the mod and registers the HKMP addon if needed.
        /// </summary>
        public override void Initialize() {
            Log("Initializing...");
            
            if (IsInHkmpDirectory()) {
                Log("Located in Mods/HKMP - HKMP will auto-discover the addon.");
                return;
            }
            
            RegisterHkmpAddon();
        }
        
        /// <summary>
        /// Checks if the DLL is located in the Mods/HKMP directory.
        /// </summary>
        private static bool IsInHkmpDirectory() {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(assemblyPath) ?? "";
            var parentDir = Path.GetFileName(directory);
            return parentDir.Equals("HKMP", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Manually registers the HKMP addon with the server.
        /// </summary>
        private void RegisterHkmpAddon() {
            ServerAddon.RegisterAddon(new HkmpServerAddon());
            Log("HKMP addon registered.");
        }
    }
}
#endif
