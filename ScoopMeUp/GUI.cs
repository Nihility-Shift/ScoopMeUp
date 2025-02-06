using CG;
using CG.Game;
using CG.Game.Player;
using CG.Network;
using CG.Objects;
using CG.Ship.Modules;
using HarmonyLib;
using Opsive.Shared.Utility;
using ResourceAssets;
using UnityEngine;
using VoidManager.CustomGUI;
using VoidManager.Utilities;

namespace ScoopMeUp
{
    internal class GUI : ModSettingsMenu
    {
        public override string Name() => "Scoop Me Up";

        public override void Draw()
        {
            GUILayout.Label("");
            GUITools.DrawChangeKeybindButton("Change Activate Scoop Keybind", ref Configs.ActivateScoop);
            GUILayout.Label("");

            GUILayout.Label("When the Scoop finsihes (Host only):");

            if (GUILayout.Toggle(Configs.ScoopMode.Value == HostSide.ScoopModes.Stop, "Stop attracting near the ship"))
            {
                Configs.ScoopMode.Value = HostSide.ScoopModes.Stop;
            }
            if (GUILayout.Toggle(Configs.ScoopMode.Value == HostSide.ScoopModes.Transport, "Move the player into the ship"))
            {
                Configs.ScoopMode.Value = HostSide.ScoopModes.Transport;
            }
            if (GUILayout.Toggle(Configs.ScoopMode.Value == HostSide.ScoopModes.Kill, "Squash the player into the scoop (this kills the player)"))
            {
                Configs.ScoopMode.Value = HostSide.ScoopModes.Kill;
            }
        }
    }
}
