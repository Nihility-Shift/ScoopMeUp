using CG.Objects;
using CG.Ship.Hull;
using CG.Ship.Modules;
using Photon.Pun;
using UnityEngine;
using VoidManager;
using VoidManager.ModMessages;

namespace ScoopMeUp
{
    internal class MessageHandler : ModMessage
    {
        private const int version = 1;

        internal enum MessageType
        {
            ButtonPressed,
            ButtonReleased,
            PullingStarted,
            PullingStopped,
            Teleport,
            Kill,
            TemporaryItem
        }

        public override void Handle(object[] arguments, Photon.Realtime.Player sender)
        {
            if (arguments.Length < 1) return;
            if (arguments[0] is not int) return;
            int versionReceived = (int)arguments[0];
            if (versionReceived != version)
            {
                BepinPlugin.Log.LogInfo($"Got version {versionReceived} from {sender.NickName}, expected version {version}");
                return;
            }
            if (arguments.Length < 2) return;
            if (arguments[1] is not int) return;
            MessageType message = (MessageType)arguments[1];
            int viewId = 0;
            if (message != MessageType.ButtonPressed && message != MessageType.ButtonReleased)
            {
                if (!sender.IsMasterClient) return;
                if (arguments.Length < 3) return;
                if (arguments[2] is not int) return;
                viewId = (int)arguments[2];
            }

            switch (message)
            {
                #region Host side
                case MessageType.ButtonPressed:
                    if (!PhotonNetwork.IsMasterClient) return;
                    HostSide.CreateItem(sender);
                    break;

                case MessageType.ButtonReleased:
                    if (!PhotonNetwork.IsMasterClient) return;
                    if (!HostSide.playerInfo.ContainsKey(sender)) break;
                    HostSide.DestroyItem(sender);
                    break;
                #endregion

                #region Client side
                case MessageType.PullingStarted:
                    ClientSide.itemToFollow = PhotonView.Find(viewId)?.gameObject?.GetComponent<CarryableObject>();
                    Events.Instance.LateUpdate += ClientSide.FollowItem;
                    break;

                case MessageType.PullingStopped:
                    if (!sender.IsMasterClient) return;
                    ClientSide.itemToFollow = null;
                    Events.Instance.LateUpdate -= ClientSide.FollowItem;
                    break;

                case MessageType.Teleport:
                    if (!sender.IsMasterClient) return;
                    ClientSide.Teleport(PhotonView.Find(viewId).gameObject.GetComponent<CarryableAttractor>());
                    break;

                case MessageType.Kill:
                    if (!sender.IsMasterClient) return;
                    ClientSide.Die(PhotonView.Find(viewId).gameObject.GetComponent<CarryablesSocket>());
                    break;

                case MessageType.TemporaryItem:
                    CarryableObject carryable = PhotonView.Find(viewId)?.gameObject?.GetComponent<CarryableObject>();
                    if (carryable == null) return; //Possible if the client briefly presses the keybind
                    ClientSide.NewItem(carryable);
                    break;
                #endregion
            }
        }

        internal static void Send(bool buttonDown)
        {
            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(MessageHandler)), PhotonNetwork.MasterClient, new object[] { version, buttonDown ? MessageType.ButtonPressed : MessageType.ButtonReleased });
        }

        internal static void Send(Photon.Realtime.Player target, MessageType message, int viewId)
        {
            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(MessageHandler)), target, new object[] { version, message, viewId });
        }

        internal static void Send(Photon.Realtime.ReceiverGroup group, MessageType message, int viewId)
        {
            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(MessageHandler)), group, new object[] { version, message, viewId });
        }
    }
}
