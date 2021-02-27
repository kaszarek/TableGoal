using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;

namespace TableGoal
{
    class GlobalMultiProvider
    {
        private static bool isInitialized = false;
        private static bool areListenersAssigned = false;
        private static bool isConnectedToAppWarp = false;

        public static bool IsConnected
        {
            get
            {
                return isConnectedToAppWarp;
            }
            set { GlobalMultiProvider.isConnectedToAppWarp = value; }
        }

        public static void InitializeWarp42()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                WarpClient.initialize(GlobalMultiplayerContext.API_KEY, GlobalMultiplayerContext.SECRET_KEY);
                WarpClient.setRecoveryAllowance(60);
                GlobalMultiplayerContext.roomsIDs = new List<string>();
            }
        }

        public static void AssignListeners()
        {
            if (!areListenersAssigned)
            {
                areListenersAssigned = true;
                GlobalMultiplayerContext.warpClient = WarpClient.GetInstance();
                GlobalMultiplayerContext.connectionListenObj = new ConnectionListener();
                GlobalMultiplayerContext.warpClient.AddConnectionRequestListener(GlobalMultiplayerContext.connectionListenObj);
                                
                GlobalMultiplayerContext.notificationListenerObj = new NotificationListener();
                GlobalMultiplayerContext.warpClient.AddNotificationListener(GlobalMultiplayerContext.notificationListenerObj);

                GlobalMultiplayerContext.roomReqListenerObj = new RoomReqListener();
                GlobalMultiplayerContext.warpClient.AddRoomRequestListener(GlobalMultiplayerContext.roomReqListenerObj);

                GlobalMultiplayerContext.lobbyReqListenerObj = new LobbyReqListener();
                GlobalMultiplayerContext.warpClient.AddLobbyRequestListener(GlobalMultiplayerContext.lobbyReqListenerObj);

                GlobalMultiplayerContext.zoneListenerObj = new ZoneReqListener();
                GlobalMultiplayerContext.warpClient.AddZoneRequestListener(GlobalMultiplayerContext.zoneListenerObj);
            }
        }

        public static void UnassignListeners()
        {
            if (areListenersAssigned)
            {
                if (GlobalMultiplayerContext.warpClient != null)
                {
                    GlobalMultiplayerContext.warpClient.Disconnect();

                    GlobalMultiplayerContext.warpClient.RemoveNotificationListener(GlobalMultiplayerContext.notificationListenerObj);
                    GlobalMultiplayerContext.notificationListenerObj = null;

                    GlobalMultiplayerContext.warpClient.RemoveRoomRequestListener(GlobalMultiplayerContext.roomReqListenerObj);
                    GlobalMultiplayerContext.roomReqListenerObj = null;

                    GlobalMultiplayerContext.warpClient.RemoveLobbyRequestListener(GlobalMultiplayerContext.lobbyReqListenerObj);
                    GlobalMultiplayerContext.lobbyReqListenerObj = null;

                    GlobalMultiplayerContext.warpClient.RemoveZoneRequestListener(GlobalMultiplayerContext.zoneListenerObj);
                    GlobalMultiplayerContext.zoneListenerObj = null;

                    GlobalMultiplayerContext.warpClient.RemoveConnectionRequestListener(GlobalMultiplayerContext.connectionListenObj);
                    GlobalMultiplayerContext.connectionListenObj = null;

                    GlobalMultiplayerContext.warpClient = null;
                }
                areListenersAssigned = false;
                isConnectedToAppWarp = false;
                isInitialized = false;
            }
        }

        public static void Connect(String userNameForConnection)
        {
            switch (WarpClient.GetInstance().GetConnectionState())
            {
                case WarpConnectionState.CONNECTED:
                    Debug.WriteLine("State - Already connected");
                    break;
                case WarpConnectionState.CONNECTING:
                    Debug.WriteLine("State - Connecting...");
                    break;
                case WarpConnectionState.DISCONNECTED:
                    Debug.WriteLine("State - Disconnected");
                    GlobalMultiplayerContext.UniqueLocalPlayerName = userNameForConnection;
                    GlobalMultiplayerContext.warpClient.Connect(userNameForConnection);
                    Debug.WriteLine("Called Connect");
                    break;
                case WarpConnectionState.DISCONNECTING:
                    Debug.WriteLine("State - Disconnecting...");
                    break;
                case WarpConnectionState.RECOVERING:
                    Debug.WriteLine("State - Recovering...");
                    break;
                default:
                    break;
            }
        }
    }
}
