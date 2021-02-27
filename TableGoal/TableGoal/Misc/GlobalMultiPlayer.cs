using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TableGoal
{
    class GlobalMultiPlayer
    {
        /// <summary>
        /// Delegat.
        /// </summary>
        /// <param name="opponentMove">Ruch który wykona³ przeciwnik.</param>
        public delegate void MovedFromGlobalMultiPlayerReceivedEventHandler(TypeOfMove opponentMove);

        /// <summary>
        /// Wydarzenie które zostaje wywo³ane gdy otrzymany zostanie ruch od gracza WiFi.
        /// </summary>
        public event MovedFromGlobalMultiPlayerReceivedEventHandler MoveFromGlobalMultiPlayerReceived;

        public GlobalMultiPlayer()
        {
            this.RegisterEvents();
        }

        /// <summary>
        /// Notification function which sets last move from given parameter and sends it to the opponent.
        /// Should be called when player had made move and wants to notify remote opponent about this.
        /// </summary>
        /// <param name="move"><code>TypeOfMove</code> casted to string.</param>
        public void MoveMade(string move)
        {
            GlobalMultiplayerContext.warpClient.SendUpdatePeers(MoveMessage.buildMessageBytes(move));
        }

        /// <summary>
        /// Register events for this WiFi player.
        /// </summary>
        private void RegisterEvents()
        {
            GlobalMultiplayerContext.notificationListenerObj.OnMoveCompleted += new NotificationListener.OnMoveCompletedEventHandler(notificationListenerObj_OnMoveCompleted);
        }

        void notificationListenerObj_OnMoveCompleted(string move)
        {
            TypeOfMove opponentMove = CastStringToTypeOfMove(move);
            if (opponentMove != TypeOfMove.UNKNOWN)
            {
                if (MoveFromGlobalMultiPlayerReceived != null)
                {
                    MoveFromGlobalMultiPlayerReceived(opponentMove);
                }
            }
        }

        /// <summary>
        /// Notify opponent that I lost turn.
        /// </summary>
        public void LostMyTurn()
        {
            GlobalMultiplayerContext.warpClient.SendUpdatePeers(MoveMessage.buildLostTurnMessageBytes());
        }
        
        private TypeOfMove CastStringToTypeOfMove(string move)
        {
            TypeOfMove opponentMove = TypeOfMove.UNKNOWN;
            switch (move)
            {
                case "N":
                    opponentMove = TypeOfMove.N;
                    break;
                case "NE":
                    opponentMove = TypeOfMove.NE;
                    break;
                case "E":
                    opponentMove = TypeOfMove.E;
                    break;
                case "SE":
                    opponentMove = TypeOfMove.SE;
                    break;
                case "S":
                    opponentMove = TypeOfMove.S;
                    break;
                case "SW":
                    opponentMove = TypeOfMove.SW;
                    break;
                case "W":
                    opponentMove = TypeOfMove.W;
                    break;
                case "NW":
                    opponentMove = TypeOfMove.NW;
                    break;
                default:
                    break;
            }
            return opponentMove;
        }

        /// <summary>
        /// Unregister events.
        /// </summary>
        public void UnregisterEvents()
        {
            if (GlobalMultiplayerContext.notificationListenerObj != null)
            {
                GlobalMultiplayerContext.notificationListenerObj.OnMoveCompleted -= new NotificationListener.OnMoveCompletedEventHandler(notificationListenerObj_OnMoveCompleted); // TODO : null
            }
        }

    }
}
