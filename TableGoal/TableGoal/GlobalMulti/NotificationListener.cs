using System;
using System.Net;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace TableGoal
{
    public class NotificationListener : com.shephertz.app42.gaming.multiplayer.client.listener.NotifyListener
    {
        public delegate void OnChallengeRejectedEventHandler(String challengedPlayer);
        public event OnChallengeRejectedEventHandler OnChallengeRejected;

        public delegate void OnNewGameMessageEventHandler(String challenger);
        public event OnNewGameMessageEventHandler OnNewGameMessage;

        public delegate void OnChallengeAcceptedEventHandler();
        public event OnChallengeAcceptedEventHandler OnChallengeAccepted;

        public delegate void OnMoveCompletedEventHandler(String move);
        public event OnMoveCompletedEventHandler OnMoveCompleted;

        public delegate void OnLostTurnEventHandler();
        public event OnLostTurnEventHandler OnLostTurn;

        public delegate void OnChatReceivedEventHandler(String chatMessage);
        public event OnChatReceivedEventHandler OnChatReceived;

        public delegate void LeftRoomEventHandler(String userName);
        public event LeftRoomEventHandler LeftRoom;

        public delegate void JoinRoomEventHandler(String roomId);
        public event JoinRoomEventHandler JoinRoom;

        public delegate void RoomDestroyedEventHandler(String roomId);
        public event RoomDestroyedEventHandler RoomDestroyed;

        public delegate void UserPausedGameEventHandler();
        public event UserPausedGameEventHandler UserPausedGame;

        public delegate void UserResumedGameEventHandler();
        public event UserResumedGameEventHandler UserResumedGame;

        public NotificationListener()
        { }

        public void onRoomCreated(RoomData eventObj)
        {
            Debug.WriteLine(String.Format("NotificationListener -> onRoomCreated : {0}", eventObj.getId()));
            WarpClient.GetInstance().GetLiveRoomInfo(eventObj.getId());
        }

        public void onRoomDestroyed(RoomData eventObj)
        {
            Debug.WriteLine(String.Format("NotificationListener -> onRoomDestroyed : {0}", eventObj.getId()));
            RoomDestroyedEventHandler handler = this.RoomDestroyed;
            if (handler != null)
            {
                handler(eventObj.getId());
            }
        }

        public void onUserLeftRoom(RoomData eventObj, String username)
        {
            Debug.WriteLine(String.Format("User ({2}) left {0} room id={1}", eventObj.getName(), eventObj.getId(), username));
            LeftRoomEventHandler handler = this.LeftRoom;
            if (handler != null)
            {
                if (username != GlobalMultiplayerContext.UniqueLocalPlayerName)
                {
                    handler(username.Split(':')[1]);
                }
            }
        }

        public void onUserJoinedRoom(RoomData eventObj, String username)
        {
            Debug.WriteLine(String.Format("User ({2}) joined {0} room id={1}", eventObj.getName(), eventObj.getId(), username));

            JoinRoomEventHandler handler = this.JoinRoom;
            if (handler != null)
            {
                handler(eventObj.getId());
            }

            if (!GlobalMultiplayerContext.UniqueLocalPlayerName.Equals(username))
            {
                GlobalMultiplayerContext.UniqueOpponentName = username;
            }
        }

        public void onUserLeftLobby(LobbyData eventObj, String username)
        { }

        public void onUserJoinedLobby(LobbyData eventObj, String username)
        { }

        public void onChatReceived(ChatEvent eventObj)
        {
            if (eventObj.getSender() != GlobalMultiplayerContext.UniqueOpponentName)
            {
                return;
            }
            OnChatReceivedEventHandler handler = this.OnChatReceived;
            if (handler != null)
            {
                handler(eventObj.getMessage());
            }
        }

        public void onUpdatePeersReceived(UpdateEvent eventObj)
        {
            try
            {
                MoveMessage msg = MoveMessage.buildMessage(eventObj.getUpdate());
                // wiadomości UpdatePeers są wysyłane do wszystkich, takze należy wykluczyć gracza który je wysłał
                if (msg.sender == GlobalMultiplayerContext.UniqueLocalPlayerName)
                {
                    return;
                }
                if (msg.type == "move")
                {
                    Debug.WriteLine(String.Format("Received move={0}", msg.move));
                    OnMoveCompletedEventHandler handler = this.OnMoveCompleted;
                    if (handler != null)
                    {
                        handler(msg.move);
                    }
                }
                else if (msg.type == "lostTurn")
                {
                    Debug.WriteLine(String.Format("Received lost turn"));
                    OnLostTurnEventHandler handler = this.OnLostTurn;
                    if (handler != null)
                    {
                        handler();
                    }
                }
                else if (msg.type == "new")
                {
                    Debug.WriteLine(String.Format("Challenge sent by {0} to {1}", msg.sender, GlobalMultiplayerContext.UniqueLocalPlayerName));
                    GameVariables.Instance.SecondPlayer.ShirtsColor = msg.color;
                    OnNewGameMessageEventHandler handler = this.OnNewGameMessage;
                    if (handler != null)
                    {
                        handler(msg.sender);
                    }
                }
                else if (msg.type == "challengeRejected")
                {
                    OnChallengeRejectedEventHandler handler = this.OnChallengeRejected;
                    if (handler != null)
                    {
                        handler(msg.sender);
                    }
                }
                else if (msg.type == "challengeAccepted")
                {
                    OnChallengeAcceptedEventHandler handler = this.OnChallengeAccepted;
                    if (handler != null)
                    {
                        GlobalMultiplayerContext.UniqueOpponentName = msg.sender;
                        handler();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Probblem in onUpdatePeersReceived - {0}", ex.Message));
            }
        }

        public void onUserChangeRoomProperty(RoomData roomData, string sender, Dictionary<string, object> properties)
        { }

        public void onUserChangeRoomProperty(RoomData roomData, string sender, Dictionary<string, object> properties, Dictionary<string, string> lockedPropertiesTable)
        { }

        public void onPrivateChatReceived(string sender, string message)
        { }

        public void onMoveCompleted(MoveEvent moveEvent)
        { }

        public void onUserPaused(string locid, bool isLobby, string username)
        {
            Debug.WriteLine(String.Format("User {0} paused the game {1}, {2}", username, locid, isLobby));
            UserPausedGameEventHandler handler = this.UserPausedGame;
            if (handler != null)
            {
                handler();
            }
        }

        public void onUserResumed(string locid, bool isLobby, string username)
        {
            Debug.WriteLine(String.Format("User {0} resumed the game {1}, {2}", username, locid, isLobby));
            UserResumedGameEventHandler handler = this.UserResumedGame;
            if (handler != null)
            {
                handler();
            }
        }

        public void onGameStarted(string sender, string roomId, string nextTurn)
        { }

        public void onGameStopped(string sender, string roomId)
        { }

        public void onPrivateUpdateReceived(string sender, byte[] update, bool fromUdp)
        { }
    }
}
