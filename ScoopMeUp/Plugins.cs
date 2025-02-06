using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using VoidManager;
using VoidManager.MPModChecks;

namespace ScoopMeUp
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.USERS_PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    [BepInDependency(VoidManager.MyPluginInfo.PLUGIN_GUID)]
    public class BepinPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private void Awake()
        {
            Log = Logger;
            Configs.Load(this);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }


    public class VoidManagerPlugin : VoidPlugin
    {
        public VoidManagerPlugin()
        {
            Events.Instance.LateUpdate += HostSide.CheckItemsPulled;
        }

        public static bool Enabled { get; private set; } = false;

        public override MultiplayerType MPType => MultiplayerType.Host;

        public override string Author => MyPluginInfo.PLUGIN_AUTHORS;

        public override string Description => MyPluginInfo.PLUGIN_DESCRIPTION;

        public override string ThunderstoreID => MyPluginInfo.PLUGIN_THUNDERSTORE_ID;

        public override SessionChangedReturn OnSessionChange(SessionChangedInput input)
        {
            if (Enabled != input.HostHasMod)
            {
                if (input.HostHasMod)
                {
                    Events.Instance.LateUpdate += ClientSide.CheckKeysDown;
                }
                else
                {
                    Events.Instance.LateUpdate -= ClientSide.CheckKeysDown;
                }
            }
            Enabled = input.HostHasMod;
            return new SessionChangedReturn() { SetMod_Session = true };
        }
    }
}