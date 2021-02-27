using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TableGoal
{
    class WifiPlayer
    {
        /// <summary>
        /// Character which separates moves in the received message.
        /// </summary>
        private const string MovesSequenceDelimeter = "+";
        /// <summary>
        /// Character which separates owner and its move.
        /// </summary>
        private const string InMoveDelimeter = "?";
        /// <summary>
        /// Flag which specifies whether we have multi over WiFi game ongoing. Set to false when we do not receive message for a <code>QUIT_TIME</code> period.
        /// </summary>
        private bool active;
        /// <summary>
        /// Last made move.
        /// </summary>
        private string lastMove;
        /// <summary>
        /// Flag which specifies whether we are constantly receiving data from the opponent.
        /// </summary>
        public bool CommunicationStopped { get; set; }
        /// <summary>
        /// Time which is increasing when we are not receiving anything from the opponent.
        /// </summary>
        private double calculatedTime;
        /// <summary>
        /// Time in seconds to wait there is no move incomming. After this time game decides to abandon the match.
        /// </summary>
        private const double QUIT_TIME = 15;

        /// <summary>
        /// Delegat.
        /// </summary>
        /// <param name="opponetMovesHistoryCounts">Ca³kowita iloœæ ruchów jaka jest wykonana po stronie przeciwnika. Gdy otrzymujemy ruch od przeciwnika, iloœæ jego ruchów powinna byæ o jeden wiêksza od iloœci ruchów lokalnego gracza. Co znaczy, ¿e wykona³ on ruch.</param>
        /// <param name="opponentMove">Ruch który wykona³ przeciwnik.</param>
        public delegate void MovedFromWifiPlayerReceivedEventHandler(int opponetMovesHistoryCounts, TypeOfMove opponentMove);

        /// <summary>
        /// Wydarzenie które zostaje wywo³ane gdy otrzymany zostanie ruch od gracza WiFi.
        /// </summary>
        public event MovedFromWifiPlayerReceivedEventHandler MoveFromWifiPlayerReceived;

        public WifiPlayer()
        {
            lastMove = String.Empty;
            active = true;
            this.RegisterEvents();
            calculatedTime = 0.0;
        }

        /// <summary>
        /// Notification function which sets last move from given parameter and sends it to the opponent.
        /// Should be called when player had made move and wants to notify remote opponent about this.
        /// </summary>
        /// <param name="move"><code>TypeOfMove</code> casted to string.</param>
        public void MoveMade(string move, int movesCount)
        {
            if (!active)
                return;
            lastMove = move; 
            SafeMoveMade(move, movesCount);
        }

        /// <summary>
        /// Sends notification to the remote player about move and its counter (when it was made in the MovesHistory).
        /// </summary>
        /// <param name="move"></param>
        private void SafeMoveMade(string move, int movesCount)
        {
            TableGoal.GamePlay.Play(move, movesCount);
        }

        /// <summary>
        /// Register events for this WiFi player.
        /// </summary>
        private void RegisterEvents()
        {
            TableGoal.GamePlay.OpponentPlayed += new OpponentPlayedEventHandler(GamePlay_OpponentPlayed);
        }

        /// <summary>
        /// Notify opponent that I lost turn.
        /// </summary>
        public void LostMyTurn()
        {
            TableGoal.GamePlay.PlayerLostTurn();
        }
        
        /// <summary>
        /// Constatly notifies remote opponent about last time made move and increases calculatedTime variable,
        /// which is reseted when move is received from the remote opponent. When calculatedTime is greater than
        /// set limit then it means opponent did not send data to us for a specified periond of time and game should
        /// ended.
        /// </summary>
        /// <param name="time">Timespan which increases calculatedTime.</param>
        public void NotifyAboutMoves(double time, int movesCount)
        {
            if (!active)
                return;
            /*
             * jesli naliczony czas jest wiekszy od czasu granicznego
             * to przechodzimy w stan inactive i ustawiamy flagê ComminicationStopped na prawdê.
             * 
             */
            if (calculatedTime > QUIT_TIME)
            {
                CommunicationStopped = true;
                active = false;
            }
            calculatedTime += time;
#if DEBUG
            Debug.WriteLine(String.Format("Notify the opponent. No message for = {0} s", calculatedTime));
#endif

            if (lastMove == String.Empty)
                lastMove = TypeOfMove.UNKNOWN.ToString();
            SafeMoveMade(lastMove, movesCount);
        }

        void GamePlay_OpponentPlayed(object sender, OpponentPlayedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("Received message. Reset time.");
#endif
            // otzymaliœmy wiadomoœæ od przeciwnika to resetujemy naliczony czas.
            calculatedTime = 0.0;
            TypeOfMove opponentMove = CastStringToTypeOfMove(e.gamepiece);
            if (opponentMove != TypeOfMove.UNKNOWN)
            {
                if (MoveFromWifiPlayerReceived != null)
                {
                    MoveFromWifiPlayerReceived(e.historyAfterThisMove, opponentMove);
                }
            }
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

        private TypeOfPlayer CastStringToTypeOfPlayer(string playerString)
        {
            TypeOfPlayer player = TypeOfPlayer.First;
            switch (playerString)
            {
                case "First":
                    player = TypeOfPlayer.First;
                    break;
                case "Second":
                    player = TypeOfPlayer.Second;
                    break;
                default:
                    break;
            }
            return player;
        }

        /// <summary>
        /// Cancels activities - makes this player inactive.
        /// </summary>
        public void CancelMove()
        {
            active = false;
        }

        /// <summary>
        /// Unregister events.
        /// </summary>
        public void UnregisterEvents()
        {
            if (TableGoal.GamePlay != null)
            {
                TableGoal.GamePlay.OpponentPlayed -= new OpponentPlayedEventHandler(GamePlay_OpponentPlayed);
            }
        }

    }
}
