using CG;
using CG.Game;
using CG.Game.Player;
using CG.Network;
using CG.Objects;
using CG.Ship.Modules;
using HarmonyLib;
using Photon.Pun;
using ResourceAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace ScoopMeUp
{
    internal class HostSide
    {
        internal static Dictionary<Photon.Realtime.Player, (CarryableObject item, bool pulling)> playerInfo = new();

        internal enum ScoopModes
        {
            Stop,
            Transport,
            Kill
        }

        internal static void CheckItemsPulled(object sender, EventArgs e)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Photon.Realtime.Player[] keys = playerInfo.Keys.ToArray(); //Stops collection modified error
            foreach (Photon.Realtime.Player client in keys)
            {
                bool beingPulled = false;
                (CarryableObject item, bool lastPulled) = playerInfo[client];
                foreach (CarryableAttractor scoop in ClientGame.Current.PlayerShip.gameObject.GetComponentsInChildren<CarryableAttractor>())
                {
                    foreach (CarryableAttractorLink link in scoop.AttractorLinks)
                    {
                        if (link.carryable == item)
                        {
                            beingPulled = true;
                            break;
                        }
                    }
                }

                if (beingPulled == lastPulled)
                {
                    if (!beingPulled)
                    {
                        item.Position = Player.GetByActorId(client.ActorNumber).Position + Vector3.up;
                    }
                    continue;
                }

                if (beingPulled)
                {
                    MessageHandler.Send(client, MessageHandler.MessageType.PullingStarted, item.photonView.ViewID);
                }
                else
                {
                    MessageHandler.Send(client, MessageHandler.MessageType.PullingStopped, item.photonView.ViewID);
                }

                playerInfo[client] = (item, beingPulled);
            }
        }

        internal static void CreateItem(Photon.Realtime.Player client)
        {
            //Spawn item
            Player player = Player.GetByActorId(client.ActorNumber);
            CloneStarObjectDef assetDefByName = ResourceAssetContainer<CloneStarObjectContainer, AbstractCloneStarObject, CloneStarObjectDef>.Instance.GetAssetDefByName("Item_Biomass_01");
            AbstractCloneStarObject abstractCloneStarObject = ObjectFactory.InstantiateSpaceObjectByGUID(assetDefByName.AssetGuid, player.transform.position, Quaternion.identity, null);
            CarryableObject carryable = abstractCloneStarObject as CarryableObject;

            //remove icon
            MessageHandler.Send(Photon.Realtime.ReceiverGroup.All, MessageHandler.MessageType.TemporaryItem, carryable.photonView.ViewID);

            //Store data
            playerInfo.Add(client, (carryable, false));
        }

        internal static void DestroyItem(Photon.Realtime.Player client)
        {
            CarryableObject item = playerInfo[client].item;
            if (item != null)
            {
                MessageHandler.Send(client, MessageHandler.MessageType.PullingStopped, item.photonView.ViewID);

                //Remove item from gravity scoop link before deleting
                foreach (CarryableAttractor scoop in ClientGame.Current.PlayerShip.gameObject.GetComponentsInChildren<CarryableAttractor>())
                {
                    for (int i = 0; i < scoop.AttractorLinks.Count; i++)
                    {
                        if (scoop.AttractorLinks[i].carryable == item)
                        {
                            scoop.RemoveLink(i);
                        }
                    }
                }
                item.sector = null; //Prevents object destroyed message in chat
                item.Destroy();
            }
            playerInfo.Remove(client);
        }
    }
}
