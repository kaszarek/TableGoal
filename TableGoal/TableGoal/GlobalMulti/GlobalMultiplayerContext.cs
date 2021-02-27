using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;

namespace TableGoal
{
    class GlobalMultiplayerContext
    {
        public static String UniqueLocalPlayerName;
        public static String UniqueOpponentName;
        
        // PAPER SOCCER ONLINE Europe Server
        public static String API_KEY = "non existing api key. You need to create one yourself";
        public static String SECRET_KEY = "non existing secret key. You need to create one yourself";

        public static String GameRoomId = String.Empty;

        internal static bool PlayerIsFirst = false;
        public static List<string> roomsIDs;

        public static WarpClient warpClient;
        public static ConnectionListener connectionListenObj;
        public static RoomReqListener roomReqListenerObj;
        public static NotificationListener notificationListenerObj;
        public static ZoneReqListener zoneListenerObj;
        public static LobbyReqListener lobbyReqListenerObj;
    }
}
