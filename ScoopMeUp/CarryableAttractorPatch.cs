using CG.Game.Player;
using CG.Objects;
using CG.Ship.Modules;
using HarmonyLib;
using VoidManager.Utilities;

namespace ScoopMeUp
{
    [HarmonyPatch(typeof(CarryableAttractor), "FinalAttractionOfObject")]
    internal class CarryableAttractorPatch
    {
        static void Prefix(CarryableAttractor __instance, CarryableAttractorLink link)
        {
            Photon.Realtime.Player player = null;
            CarryableObject carryable = null;
            foreach (Photon.Realtime.Player client in HostSide.playerInfo.Keys)
            {
                (CarryableObject item, bool pulling) = HostSide.playerInfo[client];
                if (item != link.carryable) continue;
                player = client;
                carryable = item;
            }
            if (carryable == null) return;

            switch (Configs.ScoopMode.Value)
            {
                case HostSide.ScoopModes.Stop:
                    HostSide.DestroyItem(player);
                    break;
                case HostSide.ScoopModes.Transport:
                    MessageHandler.Send(player, MessageHandler.MessageType.Teleport, __instance.photonView.ViewID);
                    Tools.DelayDo(() => { if (HostSide.playerInfo.ContainsKey(player)) HostSide.DestroyItem(player); }, 100); //Destroy the item
                    break;
                case HostSide.ScoopModes.Kill:
                    HostSide.DestroyItem(player);
                    CarryableObject payload = Player.GetByActorId(player.ActorNumber).Payload;
                    if (payload != null)
                    {
                        if (!payload.AmOwner)
                        {
                            payload.OwnerChange += putInSocket;
                            payload.photonView.RequestOwnership();
                            void putInSocket(Photon.Realtime.Player player)
                            {
                                payload.OwnerChange -= putInSocket;
                                link.socket.TryInsertCarryable(payload);
                            }
                        }
                        else
                        {
                            link.socket.TryInsertCarryable(payload);
                        }
                    }
                    MessageHandler.Send(player, MessageHandler.MessageType.Kill, link.socket.photonView.ViewID);
                    break;
            }
        }
    }
}
