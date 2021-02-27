using System;
using System.Net;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client;
using System.Text;
using System.Diagnostics;

namespace TableGoal
{
    public class ZoneReqListener : com.shephertz.app42.gaming.multiplayer.client.listener.ZoneRequestListener
    {
        public delegate void GotAllRoomsEventHandler();
        public event GotAllRoomsEventHandler GotAllRooms;

        public ZoneReqListener()
        { }

        public void onDeleteRoomDone(RoomEvent eventObj)
        { }

        public void onGetAllRoomsDone(AllRoomsEvent eventObj)
        { }

        public void onCreateRoomDone(RoomEvent eventObj)
        {
            if (eventObj.getResult() == WarpResponseResultCode.SUCCESS)
            {
                Debug.WriteLine(String.Format("Room \"{0}\" created.", eventObj.getData().getName()));
                GlobalMultiplayerContext.warpClient.JoinRoom(eventObj.getData().getId());
                GlobalMultiplayerContext.GameRoomId = eventObj.getData().getId();
                GlobalMultiplayerContext.warpClient.SubscribeRoom(eventObj.getData().getId());
            }
        }

        public void onGetOnlineUsersDone(AllUsersEvent eventObj)
        { }

        public void onGetLiveUserInfoDone(LiveUserInfoEvent eventObj)
        { }

        public void onSetCustomUserDataDone(LiveUserInfoEvent eventObj)
        { }

        public void onGetMatchedRoomsDone(MatchedRoomsEvent matchedRoomsEvent)
        {
            if (matchedRoomsEvent.getResult() == WarpResponseResultCode.SUCCESS)
            {
                Debug.WriteLine("Got all rooms");
                RoomData[] rooms = matchedRoomsEvent.getRoomsData();
                foreach (RoomData rd in rooms)
                {
                    if (!GlobalMultiplayerContext.roomsIDs.Contains(rd.getId()))
                    {
                        GlobalMultiplayerContext.roomsIDs.Add(rd.getId());
                    }
                }
                GotAllRoomsEventHandler handler = this.GotAllRooms;
                if (handler != null)
                {
                    GotAllRooms();
                }
            }
            else
            {
                DiagnosticsHelper.SafeShow("Could not get rooms' list. Please try again later."); // TODO: make this info meaningful
            }
        }
    }
}
