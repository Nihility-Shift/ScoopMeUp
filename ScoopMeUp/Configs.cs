using BepInEx.Configuration;
using UnityEngine;

namespace ScoopMeUp
{
    internal class Configs
    {
        internal static ConfigEntry<KeyCode> ActivateScoop;

        internal static ConfigEntry<HostSide.ScoopModes> ScoopMode;

        internal static void Load(BepinPlugin plugin)
        {
            ActivateScoop = plugin.Config.Bind("ScoopMeUp", "ActivateScoop", KeyCode.R);
            ScoopMode = plugin.Config.Bind("ScoopMeUp", "ScoopMode", HostSide.ScoopModes.Transport);
        }
    }
}
