using System;
using System.Net;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace TableGoal
{
    public class RoomReqListener : com.shephertz.app42.gaming.multiplayer.client.listener.RoomRequestListener
    {
        public delegate void GotLiveRoomInfoEventHandler(LiveRoomInfoEvent roomData);
        public event GotLiveRoomInfoEventHandler GotLiveRoomInfo;

        public delegate void JoinedRoomEventHandler();
        public event JoinedRoomEventHandler JoinedRoom;

        public RoomReqListener()
        { }

        public void onSubscribeRoomDone(RoomEvent eventObj)
        { }

        public void onUnSubscribeRoomDone(RoomEvent eventObj)
        { }

        public void onJoinRoomDone(RoomEvent eventObj)
        {
            if (eventObj.getResult() == WarpResponseResultCode.BAD_REQUEST)
            {
                DiagnosticsHelper.SafeShow(String.Format("Room has been destroyed in the meanwhile. Please try a different room."));
                return;
            }
            if (eventObj.getResult() == WarpResponseResultCode.SUCCESS)
            {
                Debug.WriteLine(String.Format("Joined {1} room with id={0}", eventObj.getData().getId(), eventObj.getData().getName()));
                WarpClient.GetInstance().SubscribeRoom(GlobalMultiplayerContext.GameRoomId);
                WarpClient.GetInstance().GetLiveRoomInfo(GlobalMultiplayerContext.GameRoomId);
                if (JoinedRoom != null)
                {
                    JoinedRoom();
                }
            }
            else if (eventObj.getResult() == WarpResponseResultCode.RESOURCE_NOT_FOUND) // pokój został usunięty w między czasie
            { }
            else
            {
                DiagnosticsHelper.SafeShow(String.Format("Could not join the room {0}. Please try another one.", eventObj.getData().getName()));
            }
        }

        public void onLeaveRoomDone(RoomEvent eventObj)
        { }

        public void onGetLiveRoomInfoDone(LiveRoomInfoEvent eventObj)
        {
            if (eventObj.getResult() == WarpResponseResultCode.BAD_REQUEST)
            {
                return;
            }
            if (eventObj.getResult() == WarpResponseResultCode.RESOURCE_NOT_FOUND)
            {
                return;
            }
            if (eventObj.getJoinedUsers() == null)
            {
                Debug.WriteLine(String.Format("Room {0}:{1} has no users. Checking its state again.", eventObj.getData().getName(), eventObj.getData().getId()));
                WarpClient.GetInstance().GetLiveRoomInfo(eventObj.getData().getId());
                return;
            }
            GotLiveRoomInfoEventHandler handler = this.GotLiveRoomInfo;
            if (handler != null)
            {
                handler(eventObj);
            }
            string joinedUsers = String.Empty;
            foreach (String S in eventObj.getJoinedUsers())
                joinedUsers += S + "  |  ";
            string roomProperties = this.GetType() + "\n" + "ID = " + eventObj.getData().getId() + "\n" +
                           "Max U = " + eventObj.getData().getMaxUsers() + "\n" +
                           "Joined U = " + joinedUsers + "\n" +
                           "Name = " + eventObj.getData().getName() + "\n" +
                           "Room own = " + eventObj.getData().getRoomOwner() + "\n" +
                           "Properties = {0} \n";

            string props = String.Empty;
            if (eventObj.getProperties() != null)
            {
                foreach (string key in eventObj.getProperties().Keys)
                {
                    props += "\n" + key + " : " + eventObj.getProperties()[key];
                }
            }
            Debug.WriteLine(String.Format(roomProperties, props));
        }

        public void onSetCustomRoomDataDone(LiveRoomInfoEvent eventObj)
        { }

        public void onUpdatePropertyDone(LiveRoomInfoEvent lifeLiveRoomInfoEvent)
        { }

        public void onLockPropertiesDone(byte result)
        { }

        public void onUnlockPropertiesDone(byte result)
        { }
    }
}
