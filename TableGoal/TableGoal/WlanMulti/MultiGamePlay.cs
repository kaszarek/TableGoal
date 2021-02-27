/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see all Code Samples for Windows Phone, visit http://go.microsoft.com/fwlink/?LinkID=219604 
  
*/
using System;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Xna.Framework;

namespace TableGoal
{
    /// <summary>
    /// This class handles all game communication. Communication for the game is made up of
    /// a number of commands that we have defined in the GameCommand.cs class. These commands 
    /// are the grammar, or set of actions, that we can transmist and receive and interpret.
    /// </summary>
    public class MultiGamePlay : IDisposable
    {
        /// <summary>
        /// All communication takes place using a UdpAnySourceMulticastChannel. 
        /// A UdpAnySourceMulticastChannel is a wrapper we create around the UdpAnySourceMulticastClient.
        /// </summary>
        /// <value>The channel.</value>
        private UdpAnySourceMulticastChannel Channel { get; set; }

        /// <summary>
        /// The IP address of the multicast group. 
        /// </summary>
        /// <remarks>
        /// A multicast group is defined by a multicast group address, which is an IP address 
        /// that must be in the range from 224.0.0.0 to 239.255.255.255. Multicast addresses in 
        /// the range from 224.0.0.0 to 224.0.0.255 inclusive are “well-known” reserved multicast 
        /// addresses. For example, 224.0.0.0 is the Base address, 224.0.0.1 is the multicast group 
        /// address that represents all systems on the same physical network, and 224.0.0.2 represents 
        /// all routers on the same physical network.The Internet Assigned Numbers Authority (IANA) is 
        /// responsible for this list of reserved addresses. For more information on the reserved 
        /// address assignments, please see the IANA website.
        /// http://go.microsoft.com/fwlink/?LinkId=221630
        /// </remarks>
        private const string GROUP_ADDRESS = "224.0.13.13";

        /// <summary>
        /// This defines the port number through which all communication with the multicast group will take place. 
        /// </summary>
        /// <remarks>
        /// The value in this example is arbitrary and you are free to choose your own.
        /// </remarks>
        private const int GROUP_PORT = 55331;
        
        /// <summary>
        /// The name of this player.
        /// </summary>
        private string _playerName;
        /// <summary>
        /// Flag which says whether this player is host for a games.
        /// </summary>
        private bool _isHost;

        public bool IsHost
        {
            get { return _isHost; }
        }

        public MultiGamePlay()
        {
            this.Channel = new UdpAnySourceMulticastChannel(GROUP_ADDRESS, GROUP_PORT);
            // Register for events on the multicast channel.
            RegisterEvents();

            // Send a message to the multicast group regularly. This is done because
            // we use UDP unicast messages during the game, sending messages directly to 
            // our opponent. This uses the BeginSendTo method on UpdAnySourceMulticastClient
            // and according to the documentation:
            // "The transmission is only allowed if the address specified in the remoteEndPoint
            // parameter has already sent a multicast packet to this receiver"
            // So, if everyone sends a message to the multicast group, we are guaranteed that this 
            // player (receiver) has been sent a multicast packet by the opponent. 
            
            StartKeepAlive();
        }

        #region Properties
        /// <summary>
        /// The player, against whom, I am currently playing.
        /// </summary>
        private PlayerInfo _currentOpponent;
        public PlayerInfo CurrentOpponent
        {
            get
            {
                return _currentOpponent;
            }
        }

        /// <summary>
        /// Set current opponent to null.
        /// </summary>
        public void IamAlone()
        {
            _currentOpponent = null;
        }

        /// <summary>
        /// Set channel to the state when exception's information is propagated.
        /// </summary>
        public void UnhookExceptionFlagForChannel()
        {
            this.Channel.ResetExceptionHandlerFlag();
        }

        /// <summary>
        /// Whether we are joined to the multicast group
        /// </summary>
        public bool IsJoined { get; private set; }
        #endregion

        #region Game Actions
        /// <summary>
        /// Join the multicast group.
        /// </summary>
        /// <param name="playerName">The player name I want to join as.</param>
        /// <remarks>The player name is not needed for multicast communication. it is 
        /// used in this example to identify each member of the multicast group with 
        /// a friendly name. </remarks>
        public void Join(string playerName)
        {
            if (IsJoined)
            {
                return;
            }

            // Store my player name
            _playerName = playerName;

            //Open the connection
            this.Channel.Open();
        }

        /// <summary>
        /// Join the multicast group.
        /// </summary>
        /// <param name="playerName">The player name I want to join as.</param>
        /// <param name="isHost">Flag which speficies if we are creating game or want to join other games.</param>
        /// <remarks>The player name is not needed for multicast communication. it is 
        /// used in this example to identify each member of the multicast group with 
        /// a friendly name. </remarks>
        public void Join(string playerName, bool isHost)
        {
            if (IsJoined)
            {
                return;
            }

            // Store my player name
            _playerName = playerName;

            // Store information whether this player hosting game or joining
            _isHost = isHost;
            wandering = false;

            //Open the connection
            this.Channel.Open();
            if (this.Channel.IsJoined)
                Channel_Joined(this, EventArgs.Empty);

            if (_dt != null)
                if (!_dt.IsEnabled)
                    StartKeepAlive();
        }

        /// <summary>
        /// Send a message to the given opponent to challenge him to a game.
        /// </summary>
        /// <param name="opponentName">The identifier for the opponent to challenge.</param>
        public void Challenge(string opponentName)
        {
            // Look for this opponent in the list of oppoennts in the group
            PlayerInfo opponent = TableGoal.Players.Where(player => player.PlayerName == opponentName).SingleOrDefault();
            if (opponent != null)
            {
                _currentOpponent = opponent;
                for (int i = 0; i < 3; i++)
                    this.Channel.SendTo(opponent.PlayerEndPoint, GameCommands.ChallengeFormat, _playerName);
            }
            else
            {
                DiagnosticsHelper.SafeShow("Could not find host.");
                if (ChallangeBack != null)
                    ChallangeBack(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Inform the given opponent that we accept his challenge to play.
        /// </summary>
        /// <param name="opponent">The opponent</param>
        public void AcceptChallenge(PlayerInfo opponent)
        {
            if (opponent != null)
            {
                _currentOpponent = opponent;
                for (int i = 0; i < 10; i++)
                    this.Channel.SendTo(_currentOpponent.PlayerEndPoint, GameCommands.AcceptChallengeFormat, _playerName);
            }
        }

        /// <summary>
        /// Reject the challenge from the given opponent.
        /// </summary>
        /// <param name="opponent">The opponent</param>
        public void RejectChallenge(PlayerInfo opponent)
        {
            if (opponent != null)
            {
                for (int i = 0; i < 5; i++)
                    this.Channel.SendTo(opponent.PlayerEndPoint, GameCommands.RejectChallengeFormat, _playerName);
            }
        }

        /// <summary>
        /// Leave the multicast group. We will not show up in the list of opponents on any
        /// other client devices.
        /// </summary>
        public void Leave(bool disconnect)
        {
            if (this.Channel != null)
            {
                // Tell everyone we have left
                this.Channel.Send(GameCommands.LeaveFormat, _playerName);
                // Only close the underlying communications channel to the multicast group
                // if disconnect == true.
                if (disconnect)
                {
                    StopkeepAlive();
                    UnregisterEvents();
                    this.Channel.Close();
                    this.Channel = null;
                }
            }

            // Clear the opponent
            _currentOpponent = null;
            wandering = false;
            _isHost = false;
            this.IsJoined = false;
        }

        public void ArtificialSilentLeave()
        {
            // Clear the opponent
            _currentOpponent = null;
            wandering = false;
            _isHost = false;
            this.IsJoined = false; 
        }

        /// <summary>
        /// Leave the current game
        /// </summary>
        public void LeaveGame()
        {
            if (this.Channel != null && _currentOpponent != null)
            {
                // Tell the opponent
                this.Channel.SendTo(_currentOpponent.PlayerEndPoint, GameCommands.LeaveGameFormat, _playerName);
                _currentOpponent = null;
                wandering = false;
            }
        }

        /// <summary>
        /// Tell the opponent what our choice is for this current game
        /// </summary>
        /// <param name="gameMove">Game move, one of: N, NE, E, SE, S, SW, W, NW.</param>
        public void Play(string gameMove)
        {
            if (this.Channel != null)
            {
                // Tell the opponent
                this.Channel.SendTo(_currentOpponent.PlayerEndPoint, GameCommands.PlayFormat, _playerName, gameMove);
            }
        }

        /// <summary>
        /// Tell the opponent what our choice is for this current game
        /// </summary>
        /// <param name="gameMove">Game move, one of: N, NE, E, SE, S, SW, W, NW.</param>
        /// <param name="movesMade">Number of moves already made.</param>
        public void Play(string gameMove, int movesMade)
        {
            if (this.Channel != null && _currentOpponent != null)
            {
                // Tell the opponent
                this.Channel.SendTo(_currentOpponent.PlayerEndPoint, GameCommands.ExtendedPlayFormat, _playerName, gameMove, movesMade);
            }
        }

        /// <summary>
        /// Tell the opponent we want to start a new game
        /// </summary>
        public void NewGame()
        {
            if (this.Channel != null && _currentOpponent != null)
            {
                // Tell the opponent
                this.Channel.SendTo(_currentOpponent.PlayerEndPoint, GameCommands.NewGameFormat, _playerName);
            }
        }

        /// <summary>
        /// Tell the opponent it took to long for me to move and I lost move
        /// </summary>
        public void PlayerLostTurn()
        {
            if (this.Channel != null && _currentOpponent != null)
            {
                // Tell the opponent
                for (int i = 0; i < 10; i++)
                    this.Channel.SendTo(_currentOpponent.PlayerEndPoint, GameCommands.LostTurnFormat, _playerName);
            } 
        }

        /// <summary>
        /// Sends <code>CoachPipTalkCodes</code> converted to string to the opponent in WiFi multiplayer game.
        /// </summary>
        /// <param name="message"><code>CoachPipTalkCodes</code> converted to string</param>
        public void SendTrashTalkMessage(string message)
        {
            if (this.Channel != null && _currentOpponent != null)
            {
                // Tell the opponent
                for (int i = 0; i < 5; i++)
                    this.Channel.SendTo(_currentOpponent.PlayerEndPoint, GameCommands.TrashTalkMessageFormat, message);
            } 
        }

        /// <summary>
        /// Send request to the specific player about his full game details.
        /// </summary>
        /// <param name="opponent">Player info.</param>
        public void RequestFullInfoDetails(PlayerInfo opponent)
        {
            if (this.Channel != null && opponent != null)
            {
                this.Channel.SendTo(opponent.PlayerEndPoint, GameCommands.InfoRequestFormat, _playerName);
            }
        }
        /// <summary>
        /// Tell everyone about started game details and picked color.
        /// </summary>
        public void SendFullInfoDetails()
        {
            if (this.Channel != null)
            {
                this.Channel.Send(GameCommands.InfoDetailsFormat,
                                  _playerName,
                                  GameVariables.Instance.ColorOfWifiGame().ToString(),
                                  GameVariables.Instance.TypeOfField.ToString(), 
                                  GameVariables.Instance.IsLimitedByGoals.ToString(),
                                  GameVariables.Instance.Limitation().ToString());
            }
        }
        /// <summary>
        /// Tell opponent about used color.
        /// </summary>
        /// <param name="opponent">Player info.</param>
        public void SendColorDetails(PlayerInfo opponent)
        {
            if (this.Channel != null && opponent != null)
            {
                this.Channel.SendTo(opponent.PlayerEndPoint, 
                                    GameCommands.ColorDetailsFormat, 
                                    _playerName, 
                                    GameVariables.Instance.ColorOfWifiGame().ToString());
            }
        }

        /// <summary>
        /// Tell opponent about used color.
        /// </summary>
        /// <param name="opponent">Player's name.</param>
        public void SendColorDetails(string opponentName)
        {
            PlayerInfo opponent = TableGoal.Players.Where(player => player.PlayerName == opponentName).SingleOrDefault();
            if (this.Channel != null && opponent != null)
            {
                this.Channel.SendTo(opponent.PlayerEndPoint,
                                    GameCommands.ColorDetailsFormat,
                                    _playerName,
                                    GameVariables.Instance.ColorOfWifiGame().ToString());
            }
        }
        /// <summary>
        /// Request color of a shirt of a specified player.
        /// </summary>
        /// <param name="opponent">Player info.</param>
        public void RequestColorDetails(PlayerInfo opponent)
        {
            if (this.Channel != null && opponent != null)
            {
                this.Channel.SendTo(opponent.PlayerEndPoint, GameCommands.ColorRequestFormat, _playerName);
            }
        }
        #endregion

        #region Multicast Communication
        /// <summary>
        /// Register for events on the multicast channel.
        /// </summary>
        private void RegisterEvents()
        {
            // Register for events from the multicast channel
            this.Channel.Joined += new EventHandler(Channel_Joined);
            this.Channel.BeforeClose += new EventHandler(Channel_BeforeClose);
            this.Channel.PacketReceived += new EventHandler<UdpPacketReceivedEventArgs>(Channel_PacketReceived);
            this.Channel.ConnectionException += new EventHandler(Channel_ConnectionException);
        }

        /// <summary>
        /// Handles any kind of exception to the connectiovity failures.
        /// </summary>
        /// <param name="sender">This isntance of MultiGamePlay.</param>
        /// <param name="e">Empty EventArgs.</param>
        void Channel_ConnectionException(object sender, EventArgs e)
        {
            if (ConnectionProblem != null)
                ConnectionProblem(this, EventArgs.Empty);
        }

        /// <summary>
        /// Unregister for events on the multicast channel
        /// </summary>
        private void UnregisterEvents()
        {
            if (this.Channel != null)
            {
                // Register for events from the multicast channel
                this.Channel.Joined -= new EventHandler(Channel_Joined);
                this.Channel.BeforeClose -= new EventHandler(Channel_BeforeClose);
                this.Channel.PacketReceived -= new EventHandler<UdpPacketReceivedEventArgs>(Channel_PacketReceived);
                this.Channel.ConnectionException -= new EventHandler(Channel_ConnectionException);
            }
        }
        /// <summary>
        /// Handles the BeforeClose event of the Channel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Channel_BeforeClose(object sender, EventArgs e)
        {
            if (this.Channel != null)
            {
                this.Channel.Send(String.Format(GameCommands.Leave, _playerName));
            }
        }

        /// <summary>
        /// Handles the Joined event of the Channel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Channel_Joined(object sender, EventArgs e)
        {
            this.IsJoined = true;

            /*
             * Przez to, że to jest UDP to czasami jeden raz wysłana wiadomość po prostu nie dochodzi.
             * Wysyłamy informację 10 razy (z nadzieją, że choć jedna przejdzie :))
             */
            for (int i = 0; i < 10; i++)
            {
                this.Channel.Send(GameCommands.JoinFormat, _playerName);
                if (_isHost)
                    this.SendFullInfoDetails();
            }
        }

        /// <summary>
        /// Handles the PacketReceived event of the Channel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SilverlightPlayground.UDPMulticast.UdpPacketReceivedEventArgs"/> instance containing the event data.</param>
        void Channel_PacketReceived(object sender, UdpPacketReceivedEventArgs e)
        {
            string message = e.Message.Trim('\0');
            string[] messageParts = message.Split(GameCommands.CommandDelimeter.ToCharArray());

            if (messageParts.Length == 2)
            {
                switch (messageParts[0])
                {
                    case GameCommands.Join:
                        OnPlayerJoined(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.AcceptChallenge:
                        OnChallengeAccepted(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.Challenge:
                        OnChallengeReceived(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.NewGame:
                        OnNewGame(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.Leave:
                        OnPlayerLeft(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.LeaveGame:
                        OnLeaveGame(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.RejectChallenge:
                        OnChallengeRejected(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.InfoRequest:
                        OnFullGameDetailsRequested(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.ColorRequest:
                        OnColorRequest(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.LostTurn:
                        OnOpponentLostMove(new PlayerInfo(messageParts[1], e.Source));
                        break;
                    case GameCommands.TrashTalkMessage:
                        OnTrashTalkMessageReceived(messageParts[1]);
                        break;
                    case GameCommands.Ready:
#if DEBUG
                        Debug.WriteLine("Ready received");
#endif
                        break;
                    default:
                        break;
                }
            }
            else if (messageParts.Length == 4 && messageParts[0] == GameCommands.Play)
            {
                OpponentPlayedEventArgs args = new OpponentPlayedEventArgs();
                args.gamepiece = messageParts[2];
                args.playerInfo = new PlayerInfo(messageParts[1], e.Source);
                args.historyAfterThisMove = int.Parse(messageParts[3]);
                if (OpponentPlayed != null)
                {
                    OpponentPlayed(this, args);
                }
            }
            else if (messageParts.Length == 3 && messageParts[0] == GameCommands.ColorDetails)
            {
                OnColorReveived(new PlayerInfo(messageParts[1], e.Source, ExportColor(messageParts[2])));
            }
            else if (messageParts.Length == 6 && messageParts[0] == GameCommands.InfoDetails)
            {
                OnFullGameDetailsReceived(new PlayerInfo(messageParts[1], e.Source, ExportColor(messageParts[2])),
                                          messageParts[3],
                                          messageParts[4],
                                          messageParts[5]);
            }
            else
            {
                // Ignore messages that don't have the expected number of parts.
            }
        }
        #endregion

        /// <summary>
        /// Exports <code>Color</code> from string (RGB format).
        /// </summary>
        /// <param name="encapsulatedColor">Color in RGB format.</param>
        /// <returns><code>Color</code> extracted from the string.</returns>
        private Color ExportColor(string encapsulatedColor)
        {
            string sep = ":RGBA";
            string[] RGB = encapsulatedColor.Split(sep.ToCharArray());
            int r = int.Parse(RGB[2]);
            int g = int.Parse(RGB[4]);
            int b = int.Parse(RGB[6]);
            return new Color(r, g, b);
        }

        #region Command Handlers - methods to handle each command that we receive
        /// <summary>
        /// Handle a player joining the multicast group.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnPlayerJoined(PlayerInfo playerInfo)
        {
            bool add = true;
            int numberAdded = 0;

            foreach (PlayerInfo pi in TableGoal.Players)
            {
                if (pi.PlayerName == playerInfo.PlayerName)
                {

                    pi.PlayerEndPoint = playerInfo.PlayerEndPoint;

                    add = false;
                    break;
                }
            }

            if (add)
            {
                numberAdded++;
                TableGoal.Players.Add(playerInfo);
            }

            // If any new players have been added, send out our join message again
            // to make sure we are added to their player list.
            if (numberAdded > 0)
            {
                /*
                 * Przez to, że to jest UDP to czasami jeden raz wysłana wiadomość po prostu nie dochodzi.
                 * Wysyłamy informację 10 razy (z nadzieją, że choć jedna przejdzie :))
                 */
                for (int i = 0; i < 10; i++)
                {
                    this.Channel.Send(GameCommands.JoinFormat, _playerName);
                    if (_isHost)
                        this.SendFullInfoDetails();
                }
            }

#if DEBUG
            Debug.WriteLine(" =========   PLAYERS =============");
            foreach (PlayerInfo pi in TableGoal.Players)
            {
                Debug.WriteLine(string.Format("{1} [{0}]", pi.PlayerName, pi.PlayerEndPoint));
            }
#endif
        }

        /// <summary>
        /// Handles request for a full game details - from joining player.
        /// </summary>
        /// <param name="playerInfo">Irrelevant player info - information is sent to everyone.</param>
        private void OnFullGameDetailsRequested(PlayerInfo playerInfo)
        {
            SendFullInfoDetails();
        }

        /// <summary>
        /// Handles color request.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnColorRequest(PlayerInfo playerInfo)
        {
            SendColorDetails(playerInfo);
        }
        
        private void OnFullGameDetailsReceived(PlayerInfo playerInfo, string field, string isGoalLimited, string limit)
        {
            bool isGoal = bool.Parse(isGoalLimited);
            int limitation = int.Parse(limit);
            PlayField f = PlayField.classic;
            switch (field)
            {
                case "classic":
                    f = PlayField.classic;
                    break;
                case "large":
                    f = PlayField.large;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < TableGoal.Players.Count; i++)
            {
                if (TableGoal.Players[i].PlayerName == playerInfo.PlayerName)
                {
                    TableGoal.Players[i].PlayerColor = playerInfo.PlayerColor;
                    TableGoal.Players[i].PlayerEndPoint = playerInfo.PlayerEndPoint;
                    TableGoal.Players[i].Field = f;

                    if (isGoal)
                    {
                        TableGoal.Players[i].IsGoalLimited = true;
                        TableGoal.Players[i].GameLimit = limitation;
                    }
                    else
                    {
                        TableGoal.Players[i].IsGoalLimited = false;
                        TableGoal.Players[i].GameLimit = limitation;
                    } 
                    break;
                }
            }
        }


        /// <summary>
        /// Handle a player sharing his color.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnColorReveived(PlayerInfo playerInfo)
        {
            for (int i = 0; i < TableGoal.Players.Count; i++)
            {
                if (TableGoal.Players[i].PlayerName == playerInfo.PlayerName)
                {
                    TableGoal.Players[i].PlayerColor = playerInfo.PlayerColor;
                    TableGoal.Players[i].PlayerEndPoint = playerInfo.PlayerEndPoint;
                    GameVariables.Instance.SecondPlayer.ShirtsColor = playerInfo.PlayerColor;

                    break;
                }
            }
        }
        
        /// <summary>
        /// Handle a host game properties like: is game limited by goals, limitation (goals, or seconds), and type of field.
        /// </summary>
        /// <param name="isGoalLimited">Is game limited by time, otherwise it is goal limited.</param>
        /// <param name="limit">Game limitation.</param>
        /// <param name="field">Type of the field/</param>
        private void OnGameDetailsReceived(string isGoalLimited, string limit, string field)
        {
            SetGameDetails(isGoalLimited, limit, field);
        }

        /// <summary>
        /// Set game details regarding field, limitation and its type.
        /// </summary>
        /// <param name="isGoalLimited">Is game limited bu the time, otherwise game is limited by goals.</param>
        /// <param name="limit">Game limitation.</param>
        /// <param name="field">Type of the field.</param>
        private void SetGameDetails(string isGoalLimited, string limit, string field)
        {
            bool isGoal = bool.Parse(isGoalLimited);
            int limitation = int.Parse(limit);
            PlayField f = PlayField.classic;
            switch (field)
            {
                case "classic":
                    f = PlayField.classic;
                    break;
                case "large":
                    f = PlayField.large;
                    break;
                default:
                    break;
            }
            GameVariables.Instance.TypeOfField = f;
            if (isGoal)
            {
                GameVariables.Instance.IsLimitedByGoals = true;
                GameVariables.Instance.GoalsLimit = limitation;
            }
            else
            {
                GameVariables.Instance.IsLimitedByGoals = false;
                GameVariables.Instance.TotalTime = limitation;
            } 
        }
        
        /// <summary>
        /// Handle a player leaving the multicast group.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnPlayerLeft(PlayerInfo playerInfo)
        {
            if (playerInfo.PlayerName != _playerName)
            {
                foreach (PlayerInfo pi in TableGoal.Players)
                {
                    if (pi.PlayerName == playerInfo.PlayerName)
                    {
                        TableGoal.Players.Remove(pi);
                        break;
                    }
                }
            }

            OnLeaveGame(playerInfo);
        }

        /// <summary>
        /// Flag which specifies whether player is up to a game or is still ready for accepting challenges.
        /// </summary>
        bool wandering = false;

        /// <summary>
        /// Handle a challenge from a player.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnChallengeReceived(PlayerInfo playerInfo)
        {
            // jeśli myślimy już nad jakimś wyzwaniem, to nie rozważamy innych
            if (wandering)
                return;
            if (playerInfo.PlayerName == _playerName)
                return;
            
            if (!_isHost)
                return;

            // zaczynamy rozważać wyzwanie
            wandering = true;
            if (_currentOpponent != null)
            {
                // if we receive more challange requests from same player - do nothing -> stop considering it
                if (playerInfo.PlayerName == _currentOpponent.PlayerName)
                {
                    wandering = false;
                    return;
                }
                // Automatically reject incoming challenge if I am already in a game
                RejectChallenge(playerInfo);
            }
            else
            {

                MessageBoxResult result = DiagnosticsHelper.SafeShow(String.Format("'{0}' wants to beat you." + Environment.NewLine + "Ok to accept, Cancel to reject", playerInfo.PlayerName), "Match request", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    AcceptChallenge(playerInfo);
                    // START NEW GAME
                    this.StartGame(this, EventArgs.Empty);
                    StopkeepAlive();
                }
                else
                {
                    RejectChallenge(playerInfo);
                }
            }
            wandering = false;
        }

        /// <summary>
        /// Handle a player accepting our challenge.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnChallengeAccepted(PlayerInfo playerInfo)
        {
            StartGameHandler handler = this.StartGame;
            if (handler != null)
            {
                _currentOpponent = playerInfo;
                // START NEW GAME
                handler(this, EventArgs.Empty);
                StopkeepAlive();
            }
        }

        /// <summary>
        /// Handle a player rejecting our challenge.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnChallengeRejected(PlayerInfo playerInfo)
        {
            if (_currentOpponent != null)
            {
                _currentOpponent = null;
                DiagnosticsHelper.SafeShow(String.Format("'{0}' rejected your match request", playerInfo.PlayerName));
                ChallangeBackEventHandler handler = this.ChallangeBack;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handle an opponent requesting a new game.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnNewGame(PlayerInfo playerInfo)
        {
            // Is the current opponent requesting a new game?
            if (_currentOpponent != null && playerInfo.PlayerName == _currentOpponent.PlayerName)
            {
                PlayerEventArgs args = new PlayerEventArgs();
                args.playerInfo = playerInfo;
                NewGameRequestedHandler handler = this.NewGameRequested;
                if (handler != null)
                {
                    handler(this, args);
                }
            }
        }

        /// <summary>
        /// Handle a player leaving a game.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnLeaveGame(PlayerInfo playerInfo)
        {
            // Kill game if I am playing against this opponent
            if (_currentOpponent != null && playerInfo.PlayerName == _currentOpponent.PlayerName)
            {
                PlayerEventArgs args = new PlayerEventArgs();
                args.playerInfo = playerInfo;
                LeftGameHandler handler = this.LeftGame;
                if (handler != null)
                {
                    handler(this, args);
                }
            }
        }

        /// <summary>
        /// Handle an opponent losing turn.
        /// </summary>
        /// <param name="playerInfo">The player.</param>
        private void OnOpponentLostMove(PlayerInfo playerInfo)
        {
            if (_currentOpponent != null && playerInfo.PlayerName == _currentOpponent.PlayerName)
            {
                OpponentLostMoveEventHandler handler = this.OpponentLostMove;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Handles an opponent sending trash talk.
        /// </summary>
        /// <param name="message">Message's code that was sent.</param>
        private void OnTrashTalkMessageReceived(string message)
        {
            if (_currentOpponent != null)
            {
                TrashTalkMessageReceivedEventHandler handler = this.TrashTalkReceived;
                StringEventArs strArg = new StringEventArs();
                strArg.InternalString = message;
                if (handler != null)
                {
                    handler(this, strArg);
                }
            }
        }

        /// <summary>
        /// This event is raised when host rejects guest challenge.
        /// </summary>
        public event ChallangeBackEventHandler ChallangeBack;
        /// <summary>
        /// This event is raised when received a move from an opponent.
        /// </summary>
        public event OpponentPlayedEventHandler OpponentPlayed;
        public event NewGameRequestedHandler NewGameRequested;
        public event LeftGameHandler LeftGame;
        public event StartGameHandler StartGame;
        public event ConnectionProblemEventHandler ConnectionProblem;
        public event OpponentLostMoveEventHandler OpponentLostMove;
        public event TrashTalkMessageReceivedEventHandler TrashTalkReceived;
        
        #endregion

        #region Keep-Alive
        DispatcherTimer _dt;
        private void StartKeepAlive()
        {
            //if (_playerName == null)
            //    return;
            if (_dt == null)
            {
                _dt = new DispatcherTimer();
                _dt.Interval = new TimeSpan(0, 0, 1);
                _dt.Tick +=
                            delegate(object s, EventArgs args)
                            {
                                if (this.Channel != null && IsJoined)
                                {
                                    this.Channel.Send(GameCommands.ReadyFormat, _playerName);
                                }
                            };
            }
            _dt.Start();
        }


        private void StopkeepAlive()
        {
            if (_dt != null)
                _dt.Stop();
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            UnregisterEvents();
            StopkeepAlive();
        }
        #endregion

    }

    // A delegate type for hooking up change notifications.
    public delegate void ChallangeBackEventHandler (object sender, EventArgs e);
    public delegate void OpponentPlayedEventHandler(object sender, OpponentPlayedEventArgs e);
    public delegate void NewGameRequestedHandler(object sender, PlayerEventArgs e);
    public delegate void LeftGameHandler(object sender, PlayerEventArgs e);
    public delegate void StartGameHandler(object sender, EventArgs e);
    public delegate void PauseGameHandler(object sender, EventArgs e);
    public delegate void ConnectionProblemEventHandler (object sender, EventArgs e);
    public delegate void OpponentLostMoveEventHandler(object sender, EventArgs e);
    public delegate void TrashTalkMessageReceivedEventHandler(object sender, StringEventArs e);
    
    /// <summary>
    /// Parameter for the events encapsulating <code>PlayerInfo</code>.
    /// </summary>
    public class PlayerEventArgs : EventArgs
    {
        public PlayerInfo playerInfo { get; set; }
    }

    /// <summary>
    /// Parameter for the events ancapsulating String.
    /// </summary>
    public class StringEventArs : EventArgs
    {
        public String InternalString { get; set; }
    }

    /// <summary>
    /// When we receive a message that the opponent has played, we pass
    /// on the playerInfo and the gamepiece they played.
    /// </summary>
    public class OpponentPlayedEventArgs : EventArgs
    {
        public PlayerInfo playerInfo { get; set; }
        public string gamepiece { get; set; }
        public int historyAfterThisMove { get; set; }
    }
}
