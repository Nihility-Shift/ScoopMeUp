using BepInEx;
using CG.Game;
using CG.Game.Player;
using CG.Objects;
using CG.Ship.Hull;
using CG.Ship.Modules;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoidManager;
using VoidManager.Utilities;

namespace ScoopMeUp
{
    internal class ClientSide
    {
        internal static CarryableObject itemToFollow = null;

        private static bool keyHeld = false;

        internal static void CheckKeysDown(object player, EventArgs e)
        {
            if (!VoidManagerPlugin.Enabled) return;
            if (Configs.ActivateScoop.Value == KeyCode.None) return;

            //Key held and player not standing in the ship
            bool keyDown = UnityInput.Current.GetKey(Configs.ActivateScoop.Value) && !LocalPlayer.Instance.IsBeingSimulated;
            if (keyDown != keyHeld)
            {
                keyHeld = !keyHeld;
                MessageHandler.Send(keyHeld);
            }
        }

        internal static void FollowItem(object sender, EventArgs e)
        {
            if (itemToFollow == null) return;
            if (!VoidManagerPlugin.Enabled)
            {
                Events.Instance.LateUpdate -= FollowItem;
                return;
            }

            LocalPlayer.Instance.Position = itemToFollow.Position - Vector3.up;
        }

        internal static void Teleport(CarryableAttractor scoop)
        {
            Vector3 inside = scoop.transform.rotation * Vector3.back;
            LocalPlayer.Instance.Position = scoop.transform.position + inside + Vector3.up;
            
            Tools.DelayDo(() => {
                if (LocalPlayer.Instance.HasJetpack)
                    ClientGame.Current.PlayerShip.gameObject.GetComponentsInChildren<JetpackEquipper>().FirstOrDefault().ToggleEquippedItem();
            }, 300);
        }

        internal static void Die(CarryablesSocket socket)
        {
            LocalPlayer.Instance.Position = socket.transform.position;
            LocalPlayer.Instance.Die(DeathCause.Collision);
        }

        internal static void NewItem(CarryableObject carryable)
        {
            //Remove icon
            carryable.Sector = null;
            carryable.showMarker = false;
            carryable.Sector = GameSessionManager.ActiveSector;

            //Remove collisions
            carryable.Colliders.Do(collider => collider.enabled = false);

            //Make invisible
            carryable.gameObject.GetComponentsInChildren<MeshRenderer>().Do(renderer => renderer.gameObject.SetActive(false));
        }
    }
}
