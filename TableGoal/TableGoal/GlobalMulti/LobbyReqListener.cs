using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client;
using System.Diagnostics;

namespace TableGoal
{
    class LobbyReqListener : com.shephertz.app42.gaming.multiplayer.client.listener.LobbyRequestListener
    {
        public void onJoinLobbyDone(LobbyEvent eventObj)
        {
            Debug.WriteLine("onJoinLobbyDone");
        }
        public void onLeaveLobbyDone(LobbyEvent eventObj)
        {
            Debug.WriteLine("onLeaveLobbyDone");
        }
        public void onSubscribeLobbyDone(LobbyEvent eventObj)
        {
            Debug.WriteLine("onSubscribeLobbyDone");
        }
        public void onUnSubscribeLobbyDone(LobbyEvent eventObj)
        {
            Debug.WriteLine("onUnsubscribeLobbyDone");
        }
        public void onGetLiveLobbyInfoDone(LiveRoomInfoEvent eventObj)
        {
            Debug.WriteLine("onGetLiveLobbyInfoDone");
        }
    }
}
