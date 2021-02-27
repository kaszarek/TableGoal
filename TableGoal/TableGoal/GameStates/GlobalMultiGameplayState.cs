using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;

namespace TableGoal
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GlobalMultiGameplayState : GameState
    {
        int totalTime = 0;
        int actualTime = 0;
        double timePassed = 0;
        Texture2D _fingerMark;
        Texture2D _whistle;
        Texture2D _gearwheel_brainwornikg;
        Texture2D _pausedFadeOut;
        Rectangle _thinking_indicator_RightSide;
        Rectangle _thinking_indicator_LeftSide;
        Vector2 _offset_thinking_indicator;
        bool drawWhistle = true;
        float WHISTLE_COOLDOWN = 1.0f;
        float whistleTime = 1.0f;
        Vector2 _screenSize;
        SpriteFont _scoreFont;
        string _score = "0 - 0";
        Vector2 _scoreSize;
        bool gestureMoveAdded = false;
        int alphaValue = 255;
        int fadeIncrement = 10;
        double fadeDelay = .03;
        bool animationStarted = false;
        Board board;
        GlobalMultiPlayer OpponentPlayer;
        bool gameFinished = false;
        float ROTATION_ANGLE = 0;
        float _waitingRotationAngle = 0;
        OnScreenControls ControllingDPad;
        Color resultColor = Color.Black;
        Random rand = new Random(DateTime.Now.Millisecond);
        bool sureToExit = false;
        RunningTimeIndicator thinkingIndicator;
        Coach firstPlayerCoach;
        Coach secondPlayerCoach;
        SpriteFont _temporaryFont;
        bool _isPausedByRemote = false;
        bool _isConnectionProblem = false;
        /// <summary>
        /// Limit czasu na bycie roz³¹czonym od serwera
        /// </summary>
        readonly int DISCONECTED_TIMEOUT = 45 * 1000;
        /// <summary>
        /// licznik ile czasu jest siê roz³¹czonym od serwera
        /// </summary>
        int _dictonnectedCounter = 0;
        /// <summary>
        /// Indykator czy gracz wykona³ ruch
        /// </summary>
        bool _moveMade = false;
        /// <summary>
        /// Ile razy gracz w ogóle nie wykona³ ruchu
        /// </summary>
        int _skipMoveCounter = 0;
        // 4 prostaok¹ty s³u¿ace do zbudowania BLURu podczas wyboru piptalk
        RenderTarget2D[] blurredScreens;
        // Piptalks balony - wyœwietlane po klikniêciu w trenera
        PipTalkBaloon[] pickedPipTalkInBaloons;
        /// <summary>
        /// Limit czasu na myœ³enie [s]
        /// </summary>
        readonly int THINKING_LIMIT = 35;
        /// <summary>
        /// Limit czasu na czekanie na ruch [s]
        /// </summary>
        readonly int WAITING_LIMIT = 39;
        /// <summary>
        /// Flaga mówi¹ca kto zaczyna po gwizdku. Jeœli true to zaczy ¿e HOST zaczyna (pierwszy gracz), a jeœli false to znaczy, ¿e zaczyna GIEST (drugi gracz)
        /// </summary>
        bool isHostNow = false;


        public GlobalMultiGameplayState()
        {
            Field.CreateFields();
            string pathToField = Field.GetPathToBackground();
            ControllingDPad = new OnScreenControls();
            _screenSize = new Vector2(800, 480);
            this.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
            board = new Board(pathToField);
            _thinking_indicator_RightSide = new Rectangle(780, 80, 40, 40);
            _thinking_indicator_LeftSide = new Rectangle(20, 415, 40, 40);
            _offset_thinking_indicator = new Vector2(82, 82);
            thinkingIndicator = new RunningTimeIndicator(new Rectangle(0, 466, 300, 14), GameVariables.Instance.FirstPlayer.ShirtsColor, new Rectangle(500, 466, 300, 14), GameVariables.Instance.SecondPlayer.ShirtsColor, THINKING_LIMIT, false);
            thinkingIndicator.WaitingTimeLimit = WAITING_LIMIT;
            thinkingIndicator.ThinkingTimeIsUp += new RunningTimeIndicator.ThinkigTimeIsUpEventHandler(thinkingIndicator_ThinkingTimeIsUp);
            thinkingIndicator.WaitingTimeIsUp += new RunningTimeIndicator.WaitingTimeIsUpEventHandler(thinkingIndicator_WaitingTimeIsUp);

            firstPlayerCoach = new Coach(new Rectangle(10, 295, 30, 100), new Rectangle(0, 0, 50, 120), GameVariables.Instance.FirstPlayer.ShirtsColor, true);
            secondPlayerCoach = new Coach(new Rectangle(760, 100, 30, 100), new Rectangle(50, 0, 50, 120), GameVariables.Instance.SecondPlayer.ShirtsColor, false);

            thinkingIndicator.IsActive = true;

            OpponentPlayer = new GlobalMultiPlayer();
            OpponentPlayer.MoveFromGlobalMultiPlayerReceived += new GlobalMultiPlayer.MovedFromGlobalMultiPlayerReceivedEventHandler(OpponentPlayer_MoveFromWifiPlayerReceived);
            GlobalMultiplayerContext.notificationListenerObj.OnLostTurn += new NotificationListener.OnLostTurnEventHandler(notificationListenerObj_OnLostTurn);
            GlobalMultiplayerContext.notificationListenerObj.LeftRoom += new NotificationListener.LeftRoomEventHandler(notificationListenerObj_LeftRoom);
            GlobalMultiplayerContext.notificationListenerObj.UserPausedGame += new NotificationListener.UserPausedGameEventHandler(notificationListenerObj_UserPausedGame);
            GlobalMultiplayerContext.notificationListenerObj.UserResumedGame += new NotificationListener.UserResumedGameEventHandler(notificationListenerObj_UserResumedGame);
            GlobalMultiplayerContext.connectionListenObj.RecoverableConnectionProblem += new ConnectionListener.RecoverableConnectionProblemEventHandler(connectionListenObj_RecoverableConnectionProblem);
            GlobalMultiplayerContext.connectionListenObj.ConnectionRecovered += new ConnectionListener.ConnectionRecoveredEventHandler(connectionListenObj_ConnectionRecovered);
            GlobalMultiplayerContext.connectionListenObj.SeriousConnectionProblem += new ConnectionListener.SeriousConnectionProblemEventHandler(connectionListenObj_SeriousConnectionProblem);
            if (GlobalMultiplayerContext.PlayerIsFirst)
            {
                whistleTime += 0.5f; // host d³u¿ej czeka na ruch -> jako ¿e host wykonuje ruch pierwszy to goœæ musi mieæ czas na wyczyszczenie boiska przed pierwszym otrzymanym ruchem
                thinkingIndicator.IsThinking = true;
                thinkingIndicator.WarningTimeStarted += new RunningTimeIndicator.WarningTimeStartedEventHandler(firstPlayerCoach.ShowWarningCountdown);
                GlobalMultiplayerContext.notificationListenerObj.OnChatReceived += new NotificationListener.OnChatReceivedEventHandler(secondPlayerCoach.ShowCoachMessage);
                isHostNow = true;
            }
            else
            {
                whistleTime += 0.0f; // goœæ zaczyna trochê wczeœniej -> wi¹¿e siê to z tym, ¿e jak siê skoñczy gwizdek to resetowane jest boisko
                thinkingIndicator.IsWaiting = true;
                thinkingIndicator.WarningTimeStarted += new RunningTimeIndicator.WarningTimeStartedEventHandler(secondPlayerCoach.ShowWarningCountdown);
                GlobalMultiplayerContext.notificationListenerObj.OnChatReceived += new NotificationListener.OnChatReceivedEventHandler(firstPlayerCoach.ShowCoachMessage);
                isHostNow = false;
            }

            if (GameVariables.Instance.FirstPlayer.HaveMoveNow)
            {
                // to zamiast FirstPlayerCoachGo(), poniewa¿, w tym momencie nie ma wczytanych jeszcze tekstur i czcionek do zmierzenia stringów
                firstPlayerCoach.CurrentPipTalk = CoachPipTalkCodes.CheersGo;
                secondPlayerCoach.CurrentPipTalk = CoachPipTalkCodes.NoBaloon;
            }
            else
            {
                firstPlayerCoach.CurrentPipTalk = CoachPipTalkCodes.NoBaloon;
                secondPlayerCoach.CurrentPipTalk = CoachPipTalkCodes.CheersGo;
            }

            ControllingDPad.SetRefToAvailableMoves(board.ValidMovesInCurrentPosition);

            totalTime = GameVariables.Instance.TimeLeft;
            actualTime = totalTime;
            _score = GameVariables.Instance.GetScore();
            whistleTime += WHISTLE_COOLDOWN;
            AudioManager.PlaySound("whistle");

            blurredScreens = new RenderTarget2D[4];
            pickedPipTalkInBaloons = new PipTalkBaloon[4];
            PipTalkWriterReader.LoadFromIsolatedStorage();
        }

        void connectionListenObj_SeriousConnectionProblem()
        {
            _isConnectionProblem = true;
            //UnregisterEvents();
            ////GlobalMultiplayerContext.warpClient.Disconnect();
            //if (!gameFinished)
            //{
            //    gameFinished = true;
            //    DiagnosticsHelper.SafeShow("Game was interrupted by connection problems.");
            //    GameVariables.Instance.RestartGame();
            //    GameVariables.Instance.ResetVariables();
            //    GameState[] states = GameManager.GetStates();
            //    foreach (GameState state in states)
            //        GameManager.RemoveState(state);
            //    GameManager.AddState(new MainMenuState());
            //    GlobalMultiProvider.UnassignListeners();
            //}         
        }

        void connectionListenObj_ConnectionRecovered()
        {
            _isConnectionProblem = false;
            _dictonnectedCounter = 0;
        }

        void connectionListenObj_RecoverableConnectionProblem()
        {
            _isConnectionProblem = true;
        }

        void notificationListenerObj_UserResumedGame()
        {
            _isPausedByRemote = false;
            _dictonnectedCounter = 0;
        }

        void notificationListenerObj_UserPausedGame()
        {
            _isPausedByRemote = true;
        }
                
        void thinkingIndicator_WaitingTimeIsUp()
        {
            if (GameVariables.Instance.FirstPlayer.HaveMoveNow)
            {
                GameVariables.Instance.NextMoveForSecondPlayer();
                SecondPlayerCoachGo();
            }
            else if (GameVariables.Instance.SecondPlayer.HaveMoveNow)
            {
                GameVariables.Instance.NextMoveForFirstPlayer();
                FirstPlayerCoachGo();
            }
        }

        private void MakeRandomMove()
        {
            List<TypeOfMove> availableMoves = board.ValidMovesInCurrentPosition;
            int moveIndex = rand.Next(0, availableMoves.Count - 1);
            TypeOfMove selectedMove = availableMoves[moveIndex];
            board.AddMove(selectedMove);
            AudioManager.PlaySound("kick");
            OpponentPlayer.MoveMade(selectedMove.ToString());
        }

        void thinkingIndicator_ThinkingTimeIsUp()
        {
            bool isFirstPlayerTurn = GameVariables.Instance.FirstPlayer.HaveMoveNow;
            if (!_moveMade)
            {
                _skipMoveCounter++;
                switch (_skipMoveCounter)
                {
                    case 1:
                        if (isFirstPlayerTurn)
                        {
                            firstPlayerCoach.ShowStringMessage("It is not a fair play.");
                            firstPlayerCoach.ShowStringMessage("You have to make a move.");
                        }
                        else
                        {
                            secondPlayerCoach.ShowStringMessage("It is not a fair play.");
                            secondPlayerCoach.ShowStringMessage("You have to make a move.");
                        }
                        break;
                    case 2:
                        if (isFirstPlayerTurn)
                        {
                            firstPlayerCoach.ShowStringMessage("The last warning!");
                            firstPlayerCoach.ShowStringMessage("Next time I'll do a move for you.");
                        }
                        else
                        {
                            secondPlayerCoach.ShowStringMessage("The last warning!");
                            secondPlayerCoach.ShowStringMessage("Next time I'll do a move for you");
                        }
                        break;
                    default:
                        if (isFirstPlayerTurn)
                        {
                            firstPlayerCoach.ShowStringMessage("I warned you!");
                        }
                        else
                        {
                            secondPlayerCoach.ShowStringMessage("I warned you!");
                        }
                        MakeRandomMove();
                        break;
                }
            }

            if (OpponentPlayer != null)
            {
                OpponentPlayer.LostMyTurn();
            }
            if (isFirstPlayerTurn)
            {
                GameVariables.Instance.NextMoveForSecondPlayer();
                SecondPlayerCoachGo();
            }
            else
            {
                GameVariables.Instance.NextMoveForFirstPlayer();
                FirstPlayerCoachGo();
            }
        }

        void OpponentPlayer_MoveFromWifiPlayerReceived(TypeOfMove opponentMove)
        {
            _moveMade = false;
            int moveResult = board.AddMove(opponentMove);
            AudioManager.PlaySound("kick");
            if (moveResult == 1)
            {
                thinkingIndicator.IsThinking = true;
                if (GlobalMultiplayerContext.PlayerIsFirst)
                {
                    FirstPlayerCoachGo();
                }
                else
                {
                    SecondPlayerCoachGo();
                }
            }
#if DEBUG
            Debug.WriteLine(String.Format("{2} - Move {0}, added with result {1}", opponentMove, moveResult.ToString(), DateTime.Now.ToString()));
#endif
        }

        public void UnregisterEvents()
        {
            // od³¹czenie callbacka przed opuszczeniem pokoju
            if (GlobalMultiplayerContext.notificationListenerObj != null)
            {
                GlobalMultiplayerContext.notificationListenerObj.LeftRoom -= new NotificationListener.LeftRoomEventHandler(notificationListenerObj_LeftRoom);
            }
            if (!gameFinished) // jeœli gra siê nie skoñczy³a
            {
                Statistics.Instance.Przerwany();
                if (GameVariables.Instance.IsLimitedByGoals)
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TimeLeft);
                else
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TotalTime - GameVariables.Instance.TimeLeft);
                GlobalMultiplayerContext.warpClient.LeaveRoom(GlobalMultiplayerContext.GameRoomId);
                if (GlobalMultiplayerContext.PlayerIsFirst)
                {
                    GlobalMultiplayerContext.warpClient.DeleteRoom(GlobalMultiplayerContext.GameRoomId);
                }
            }            

            UnregisterEventsFromRemotePlayer();
            OpponentPlayer.MoveFromGlobalMultiPlayerReceived -= new GlobalMultiPlayer.MovedFromGlobalMultiPlayerReceivedEventHandler(OpponentPlayer_MoveFromWifiPlayerReceived);
            if (GlobalMultiplayerContext.notificationListenerObj != null)
            {
                GlobalMultiplayerContext.notificationListenerObj.OnLostTurn -= new NotificationListener.OnLostTurnEventHandler(notificationListenerObj_OnLostTurn);
                GlobalMultiplayerContext.notificationListenerObj.UserPausedGame -= new NotificationListener.UserPausedGameEventHandler(notificationListenerObj_UserPausedGame);
                GlobalMultiplayerContext.notificationListenerObj.UserResumedGame -= new NotificationListener.UserResumedGameEventHandler(notificationListenerObj_UserResumedGame);
            }

            if (GlobalMultiplayerContext.connectionListenObj != null)
            {
                GlobalMultiplayerContext.connectionListenObj.RecoverableConnectionProblem -= new ConnectionListener.RecoverableConnectionProblemEventHandler(connectionListenObj_RecoverableConnectionProblem);
                GlobalMultiplayerContext.connectionListenObj.ConnectionRecovered -= new ConnectionListener.ConnectionRecoveredEventHandler(connectionListenObj_ConnectionRecovered);
                GlobalMultiplayerContext.connectionListenObj.SeriousConnectionProblem -= new ConnectionListener.SeriousConnectionProblemEventHandler(connectionListenObj_SeriousConnectionProblem);
            }
            if (GlobalMultiplayerContext.PlayerIsFirst)
            {
                thinkingIndicator.WarningTimeStarted -= new RunningTimeIndicator.WarningTimeStartedEventHandler(firstPlayerCoach.ShowWarningCountdown);
                if (GlobalMultiplayerContext.notificationListenerObj != null)
                {
                    GlobalMultiplayerContext.notificationListenerObj.OnChatReceived -= new NotificationListener.OnChatReceivedEventHandler(secondPlayerCoach.ShowCoachMessage);
                }
            }
            else
            {
                thinkingIndicator.WarningTimeStarted -= new RunningTimeIndicator.WarningTimeStartedEventHandler(secondPlayerCoach.ShowWarningCountdown);
                if (GlobalMultiplayerContext.notificationListenerObj != null)
                {
                    GlobalMultiplayerContext.notificationListenerObj.OnChatReceived -= new NotificationListener.OnChatReceivedEventHandler(firstPlayerCoach.ShowCoachMessage);
                }
            }
        }

        void notificationListenerObj_OnLostTurn()
        {
            _moveMade = false;
            if (thinkingIndicator.IsThinking)
            {
                return;
            }
            thinkingIndicator.IsThinking = true;
            if (GameVariables.Instance.FirstPlayer.HaveMoveNow)
            {
                GameVariables.Instance.NextMoveForSecondPlayer();
                SecondPlayerCoachGo();
            }
            else if (GameVariables.Instance.SecondPlayer.HaveMoveNow)
            {
                GameVariables.Instance.NextMoveForFirstPlayer();
                FirstPlayerCoachGo();
            }
        }

        void notificationListenerObj_LeftRoom(string userName)
        {
            if (gameFinished)
            {
                return;
            }
            UnregisterEvents();
            DiagnosticsHelper.SafeShow(String.Format("{0} has quit the game...", userName));
            if (!gameFinished)
            {
                gameFinished = true;
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
                GameManager.AddState(new MainMenuState());
                GlobalMultiProvider.UnassignListeners();
            }
        }

        void UnresolvedDueToConnectionProblem(string message)
        {
            UnregisterEvents();
            DiagnosticsHelper.SafeShow(message);
            if (!gameFinished)
            {
                gameFinished = true;
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
                GameManager.AddState(new MainMenuState());
                GlobalMultiProvider.UnassignListeners();
            } 
        }

        public override void LoadContent()
        {
            board.LoadTexture(GameManager.Game.Content);
            _fingerMark = GameManager.Game.Content.Load<Texture2D>("FingerMark");
            _whistle = GameManager.Game.Content.Load<Texture2D>("whistle");
            _gearwheel_brainwornikg = GameManager.Game.Content.Load<Texture2D>("gearwheel");
            _pausedFadeOut = GameManager.Game.Content.Load<Texture2D>("halftrans4x4");
            thinkingIndicator.LoadTexture(GameManager.Game.Content);
            _scoreFont = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            _scoreSize = _scoreFont.MeasureString(_score);
            _screenSize = new Vector2(GameManager.Game.GraphicsDevice.Viewport.Width,
                                      GameManager.Game.GraphicsDevice.Viewport.Height);
            ControllingDPad.LoadTexture(GameManager.Game.Content);
            firstPlayerCoach.LoadTexture(GameManager.Game.Content);
            secondPlayerCoach.LoadTexture(GameManager.Game.Content);
            _temporaryFont = GameManager.Game.Content.Load<SpriteFont>("Fonts/Sketch15");

            blurredScreens[0] = new RenderTarget2D(GameManager.GraphicsDevice, 800, 480);
            blurredScreens[1] = new RenderTarget2D(GameManager.GraphicsDevice, 800 / 2, 480 / 2);
            blurredScreens[2] = new RenderTarget2D(GameManager.GraphicsDevice, 800 / 3, 480 / 3);
            blurredScreens[3] = new RenderTarget2D(GameManager.GraphicsDevice, 800 / 4, 480 / 4);
            int i = 0;
            pickedPipTalkInBaloons[i] = new PipTalkBaloon(new Vector2(400 - firstPlayerCoach.pipTalkLenghts[PipTalkWriterReader.ptInfo.First].X / 2, 100 + i * 80), Color.Black, firstPlayerCoach.pipTalkStrings[PipTalkWriterReader.ptInfo.First], PipTalkWriterReader.ptInfo.First);
            pickedPipTalkInBaloons[i].LoadTexture(GameManager.Game.Content);
            i = 1;
            pickedPipTalkInBaloons[i] = new PipTalkBaloon(new Vector2(400 - firstPlayerCoach.pipTalkLenghts[PipTalkWriterReader.ptInfo.Second].X / 2, 100 + i * 80), Color.Black, firstPlayerCoach.pipTalkStrings[PipTalkWriterReader.ptInfo.Second], PipTalkWriterReader.ptInfo.Second);
            pickedPipTalkInBaloons[i].LoadTexture(GameManager.Game.Content);
            i = 2;
            pickedPipTalkInBaloons[i] = new PipTalkBaloon(new Vector2(400 - firstPlayerCoach.pipTalkLenghts[PipTalkWriterReader.ptInfo.Third].X / 2, 100 + i * 80), Color.Black, firstPlayerCoach.pipTalkStrings[PipTalkWriterReader.ptInfo.Third], PipTalkWriterReader.ptInfo.Third);
            pickedPipTalkInBaloons[i].LoadTexture(GameManager.Game.Content);
            i = 3;
            pickedPipTalkInBaloons[i] = new PipTalkBaloon(new Vector2(400 - firstPlayerCoach.pipTalkLenghts[PipTalkWriterReader.ptInfo.Fourth].X / 2, 100 + i * 80), Color.Black, firstPlayerCoach.pipTalkStrings[PipTalkWriterReader.ptInfo.Fourth], PipTalkWriterReader.ptInfo.Fourth);
            pickedPipTalkInBaloons[i].LoadTexture(GameManager.Game.Content);
            base.LoadContent();
        }
        
        public void Restart()
        {
            board.Reset();
            AudioManager.StopSounds();
            AudioManager.PlaySound("whistle");
            _score = GameVariables.Instance.GetScore();
            totalTime = GameVariables.Instance.TotalTime;
            actualTime = totalTime;
            timePassed = 0.0;
            drawWhistle = true;
            gameFinished = false;
        }

        /// <summary>
        /// Callback function for MessageBox
        /// </summary>
        /// <param name="ar">Encapsulated result.</param>
        private void OnMessageBoxClosed(IAsyncResult ar)
        {
            int? buttonIndex = Guide.EndShowMessageBox(ar);
            switch (buttonIndex)
            {
                case 0:
                    sureToExit = true;
                    //GlobalMultiplayerContext.warpClient.LeaveRoom(GlobalMultiplayerContext.GameRoomId);
                    break;
                case 1:
                    sureToExit = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                if (OpponentPlayer != null)
                {
                    if (!Guide.IsVisible)
                        Guide.BeginShowMessageBox("Multiplayer restriction",
                                                  "Could not pause multiplayer game. Do you want to abandon the game?",
                                                  new string[] { "Yes", "No" },
                                                  0,
                                                  MessageBoxIcon.Warning,
                                                  new AsyncCallback(OnMessageBoxClosed),
                                                  null);
                }
                AudioManager.PlaySound("selected");
            }

            if (sureToExit)
            {
                UnregisterEvents();
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                GlobalMultiProvider.UnassignListeners();

                GameManager.RemoveState(this);
                GameManager.AddState(new MainMenuState());
            }

            if (_isPausedByRemote || _isConnectionProblem)
            {
                _dictonnectedCounter += gameTime.ElapsedGameTime.Milliseconds;
                _waitingRotationAngle += (0.03f);
                if (_waitingRotationAngle >= MathHelper.TwoPi)
                    _waitingRotationAngle -= MathHelper.TwoPi; 
                return;
            }
            #region Check if game has ended
            // jeœli gramy do limitu goli
            if (GameVariables.Instance.IsLimitedByGoals)
            {
                if (GameVariables.Instance.FirstPlayer.Goals >= GameVariables.Instance.GoalsLimit ||
                    GameVariables.Instance.SecondPlayer.Goals >= GameVariables.Instance.GoalsLimit)
                {
                    if (!gameFinished)
                    {
                        AudioManager.PlaySound("end");
                        drawWhistle = true;
                        gameFinished = true;
                    }
                    if (!drawWhistle)
                    {
                        GameManager.AddState(new GlobalMultiEndGameState());
                        this.ScreenState = global::TableGoal.ScreenState.Visible;
                    }
                    if (OpponentPlayer != null)
                        UnregisterEvents();
                }
            }
            else
            {
                if (actualTime == 0)
                {
                    if (!gameFinished)
                    {
                        AudioManager.PlaySound("end");
                        drawWhistle = true;
                        gameFinished = true;
                    }
                    if (!drawWhistle)
                    {
                        GameManager.AddState(new GlobalMultiEndGameState());
                        this.ScreenState = global::TableGoal.ScreenState.Visible;
                    }
                    if (OpponentPlayer != null)
                        UnregisterEvents();
                }
            }
            #endregion
            

            #region Sprawdzanie czy pad³ gol, albo blok
            // jeœli juz zosta³y wykonane jakieœ ruchy
            if (!drawWhistle)
            {
                if (board.MovesHistory.Count > 0)
                {
                    int i = 0;
                    foreach (Vector2 winningPoint in board.WinningPoints)
                    {
                        // jesli ktos wygra³
                        i++;
                        if (board.MovesHistory[board.MovesHistory.Count - 1].EndPosition == winningPoint)
                        {
                            // jesli pilka weszla w któreœ z pierwszych trzech pól - to jest bramka pierwszego gracza
                            // w przeciwnym wypadku punkt przyznawany jest dla gracza drugiego
                            if (i <= 3)
                            {
                                GameVariables.Instance.SecondScored();
                                FirstPlayerCoachGo();
                                isHostNow = true;
                            }
                            else
                            {
                                GameVariables.Instance.FirstScored();
                                SecondPlayerCoachGo();
                                isHostNow = false;
                            }
                            AudioManager.PlaySound("goal");
                            _score = GameVariables.Instance.GetScore();
                            _scoreSize = _scoreFont.MeasureString(_score);
                            drawWhistle = true;
                            AudioManager.PlaySound("whistle");
                            break;
                        }
                    }
                }
                /* sprawdzane po raz drugi
                 * poniewa¿ jesli pad³a bramka to teraz ruchy s¹ ju¿ wyczyszczone i trzeba sprawdziæ
                 * czy s¹ jakieœ dostepne
                 * */
                if (board.MovesHistory.Count > 0)
                {
                    foreach (Vector2 cornerPoint in board.CornerPoints)
                    {
                        // jeœli ktoœ wszed³ w róg
                        if (board.MovesHistory[board.MovesHistory.Count - 1].EndPosition == cornerPoint)
                        {
                            // jesli ostatnim kto zrobi³ ruch by³ gracz pierwszy
                            if (board.MovesHistory[board.MovesHistory.Count - 1].Player == TypeOfPlayer.First)
                            {
                                GameVariables.Instance.SecondScored();
                                FirstPlayerCoachGo();
                                isHostNow = true;
                            }
                            else
                            {
                                GameVariables.Instance.FirstScored();
                                SecondPlayerCoachGo();
                                isHostNow = false;
                            }
                            AudioManager.PlaySound("goal");
                            _score = GameVariables.Instance.GetScore();
                            _scoreSize = _scoreFont.MeasureString(_score);
                            drawWhistle = true;
                            AudioManager.PlaySound("whistle");
                            break;
                        }
                    }
                }

                // gdy gracz zosta³ zapêdzony w sytuacje bez mozliwoœci wykonania ruchu
                // to jego przeciwnik dostaje punkt
                if (board.CheckAvailableMoves().Count == 0)
                {
                    if (board.MovesHistory.Count > 0)
                    {
                        // jeœli ostatnim kto zrobi³ ruch by³ pierwszy gracz
                        if (board.MovesHistory[board.MovesHistory.Count - 1].Player == TypeOfPlayer.First)
                        {
                            GameVariables.Instance.SecondScored();
                            FirstPlayerCoachGo();
                            isHostNow = true;
                        }
                        else // ale jeœli gra jest po stronie goœcia to punkt dla HOST
                        {
                            GameVariables.Instance.FirstScored();
                            SecondPlayerCoachGo();
                            isHostNow = false;
                        }
                        
                        AudioManager.PlaySound("goal");
                        drawWhistle = true;
                        _score = GameVariables.Instance.GetScore();
                        _scoreSize = _scoreFont.MeasureString(_score);
                        AudioManager.PlaySound("whistle");
                    }
                }
            }
            #endregion

            if (animationStarted)
            {
                fadeDelay -= gameTime.ElapsedGameTime.TotalSeconds;
                if (fadeDelay <= 0)
                {
                    fadeDelay = .03;
                    alphaValue -= fadeIncrement;
                    if (alphaValue <= 110)
                    {
                        ResetAnimationValue();
                        gestureMoveAdded = false;
                        animationStarted = false;
                    }
                }
            }
            
            if (GameVariables.Instance.CurrentPlayer.Coach != TeamCoach.HUMAN)
            {
                ROTATION_ANGLE += (0.05f);
                if (ROTATION_ANGLE >= MathHelper.TwoPi)
                    ROTATION_ANGLE -= MathHelper.TwoPi; 
            }


            if (!gameFinished && GameVariables.Instance.IsLimitedByGoals)
            {
                timePassed += gameTime.ElapsedGameTime.TotalSeconds;
                actualTime = (int)timePassed;
            }
            if (!gameFinished && !GameVariables.Instance.IsLimitedByGoals)
            {
                timePassed += gameTime.ElapsedGameTime.TotalSeconds;
                actualTime = totalTime - (int)timePassed;
            }

            GameVariables.Instance.TimeLeft = actualTime;
            
            thinkingIndicator.Update(gameTime);
            firstPlayerCoach.Update(gameTime);
            secondPlayerCoach.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// Resetuje animacjê smugniêcia palcem.
        /// </summary>
        private void ResetAnimationValue()
        {
            alphaValue = 255;
            animationStarted = false;
        }


        public override void Draw(GameTime gameTime)
        {
            if (firstPlayerCoach.Active || secondPlayerCoach.Active)
            {
                GameManager.GraphicsDevice.SetRenderTarget(blurredScreens[0]);
            }
            SpriteBatch spriteBatch = GameManager.SpriteBatch;            
            spriteBatch.Begin();
            board.Draw(spriteBatch);

            // rysowanie gestu myŸniêcia palcem
            if (board.MovesHistory.Count > 0)
            {
                Team previous = GameVariables.Instance.GiveTeamFromPlayer(board.MovesHistory[board.MovesHistory.Count - 1].Player);
                if (previous.Controler == Controlling.GESTURES)
                {
                    Color markColor = Color.Gray;
                    markColor = previous.ShirtsColor;
                    if (gestureMoveAdded == true)
                        animationStarted = true;
                    if (animationStarted)
                        if (board.MovesHistory.Count > 0)
                            spriteBatch.Draw(_fingerMark,
                                             _screenSize / 2,
                                             null,
                                             new Color(markColor.R, markColor.G, markColor.B, alphaValue),
                                             Translator.TranslateDiractionToAngle(board.MovesHistory[board.MovesHistory.Count - 1].Move),
                                             new Vector2(_fingerMark.Width / 2, _fingerMark.Bounds.Height / 2),
                                             1.5f,
                                             SpriteEffects.None,
                                             0.5f);
                }
            }

            // rysowanie wyniku
            /*
             * Jesli pi³ka znajduje sie blisko wyniku to modyfikujemy kanal Alpha koloru wyniku.
             * Ustawiamy go na 100. Tym samym staje sie on przeŸroczysty i nie zas³ania boiska (przynajmniej
             * nie tak bardzo jakby nie by³ przeœroczysty)
             */
            if (board.ActualBallPosition.Y >= 400 &&
                board.ActualBallPosition.X >= _screenSize.X / 2 - _scoreSize.X - board.WallSize &&
                board.ActualBallPosition.X <= _screenSize.X / 2 + _scoreSize.X + board.WallSize)
                resultColor.A = 100;
            else
                resultColor = Color.Black;

            spriteBatch.DrawString(_scoreFont,
                                   _score,
                                   new Vector2(_screenSize.X / 2 - _scoreSize.X / 2,
                                               _screenSize.Y - _scoreSize.Y - 10.0f), // podniesienie wyniku o te 10 pixeli
                                   resultColor,
                                   0.0f,
                                   Vector2.Zero,
                                   1.0f,
                                   SpriteEffects.None,
                                   0.1f);

            spriteBatch.DrawString(_scoreFont,
                                   String.Format("{0}:{1:00}", actualTime / 60, actualTime % 60),
                                   new Vector2(20, 5),
                                   Color.Black);

            // rysowanie na ekranie przycisków do sterowania, jeœli takie zosta³y wybrane
            if (GameVariables.Instance.CurrentPlayer.Controler == Controlling.BUTTONS &&
                GameVariables.Instance.CurrentPlayer.Coach == TeamCoach.HUMAN)
            {
                ControllingDPad.SetRefToAvailableMoves(board.ValidMovesInCurrentPosition);
                ControllingDPad.Owner = GameVariables.Instance.CurrentPlayer;
                ControllingDPad.Draw(spriteBatch);
            }

            if (drawWhistle)
                DrawWhistle(spriteBatch, gameTime.ElapsedGameTime.TotalSeconds);

            if (GameVariables.Instance.FirstPlayer.Coach == TeamCoach.REMOTEOPPONENT &&
                GameVariables.Instance.FirstPlayer.HaveMoveNow)
            {
                spriteBatch.Draw(_gearwheel_brainwornikg,
                                    _thinking_indicator_LeftSide,
                                    null,
                                    Color.Black,
                                    ROTATION_ANGLE,
                                    _offset_thinking_indicator,
                                    SpriteEffects.None,
                                    0.5f);
            }
            if (GameVariables.Instance.SecondPlayer.Coach == TeamCoach.REMOTEOPPONENT &&
                GameVariables.Instance.SecondPlayer.HaveMoveNow)
            {
                spriteBatch.Draw(_gearwheel_brainwornikg,
                                    _thinking_indicator_RightSide,
                                    null,
                                    Color.Black,
                                    ROTATION_ANGLE,
                                    _offset_thinking_indicator,
                                    SpriteEffects.None,
                                    0.5f);
            }

            thinkingIndicator.FirstPlayerHaveMove = GameVariables.Instance.FirstPlayer.HaveMoveNow;
            thinkingIndicator.Draw(spriteBatch);
            spriteBatch.End();

            if (firstPlayerCoach.Active || secondPlayerCoach.Active)
            {
                GameManager.GraphicsDevice.SetRenderTarget(blurredScreens[1]);
                spriteBatch.Begin();
                spriteBatch.Draw(blurredScreens[0], GameManager.GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.End();
                GameManager.GraphicsDevice.SetRenderTarget(blurredScreens[2]);
                spriteBatch.Begin();
                spriteBatch.Draw(blurredScreens[1], GameManager.GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.End();
                GameManager.GraphicsDevice.SetRenderTarget(blurredScreens[3]);
                spriteBatch.Begin();
                spriteBatch.Draw(blurredScreens[2], GameManager.GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.End();


                GameManager.GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin();
                spriteBatch.Draw(blurredScreens[0], GameManager.GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.End();

                spriteBatch.Begin();
                spriteBatch.Draw(blurredScreens[3], GameManager.GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.End();


                spriteBatch.Begin();
                for (int i = 0; i < 4; i++)
                {
                    pickedPipTalkInBaloons[i].Draw(spriteBatch);
                }
                spriteBatch.End();
            }
            spriteBatch.Begin();
            firstPlayerCoach.Draw(spriteBatch);
            secondPlayerCoach.Draw(spriteBatch);
            if (_isPausedByRemote || _isConnectionProblem)
            {
                spriteBatch.Draw(_pausedFadeOut, new Rectangle(0, 0, 800, 480), Color.Gray);
                if (_isPausedByRemote)
                {
                    spriteBatch.DrawString(_temporaryFont, "Your opponent is having connetion problems.\n          Please wait for him to reconnect.", new Vector2(165, 60), Color.DarkBlue, 0, new Vector2(), 1, SpriteEffects.None, 0);
                    int timeLeftInSeconds = (DISCONECTED_TIMEOUT - _dictonnectedCounter) / 1000;
                    if (timeLeftInSeconds <= 0)
                    {
                        UnresolvedDueToConnectionProblem("Your opponent could not reconnect to the game.");
                    }
                    spriteBatch.DrawString(_scoreFont, String.Format("{0} s", timeLeftInSeconds), new Vector2(380, 150), Color.Yellow);
                }
                if (_isConnectionProblem)
                {
                    spriteBatch.DrawString(_temporaryFont, "You are having connetion problems.\n         Please wait to reconnect.", new Vector2(215, 60), Color.DarkBlue, 0, new Vector2(), 1, SpriteEffects.None, 0);
                    int timeLeftInSeconds = (DISCONECTED_TIMEOUT - _dictonnectedCounter) / 1000;
                    if (timeLeftInSeconds <= 0)
                    {
                        UnresolvedDueToConnectionProblem("Could not reconnect to the game.");
                    }
                    spriteBatch.DrawString(_scoreFont, String.Format("{0} s", timeLeftInSeconds), new Vector2(380, 150), Color.Yellow);
                }
                spriteBatch.Draw(_gearwheel_brainwornikg,
                                    new Rectangle(400, 280, 120, 120),
                                    null,
                                    Color.Black,
                                    _waitingRotationAngle,
                                    new Vector2(82, 82),
                                    SpriteEffects.None,
                                    0.5f);
            }
            spriteBatch.End();
        }

        
        
        private void DrawWhistle(SpriteBatch spriteBatch, double elapsedSeconds)
        {
            if (whistleTime > 0)
            {
                thinkingIndicator.PauseElapsing();
                int xMove = rand.Next(1, 15);
                int yMove = rand.Next(1, 15);
                spriteBatch.Draw(_whistle, new Rectangle(300 + xMove, 120 + yMove, 200, 200), Color.White);
                whistleTime -= (float)elapsedSeconds;
            }
            else
            {
                if (GameVariables.Instance.FirstPlayer.Goals != 0 || GameVariables.Instance.SecondPlayer.Goals != 0)
                {
                    board.Reset();
                    if (isHostNow)
                    {
                        GameVariables.Instance.NextMoveForFirstPlayer();
                        if (GlobalMultiplayerContext.PlayerIsFirst)
                        {
                            thinkingIndicator.ResetForThinking();
                        }
                        else
                        {
                            thinkingIndicator.ResetForWaiting();
                        }
                    }
                    else
                    {
                        GameVariables.Instance.NextMoveForSecondPlayer();
                        if (GlobalMultiplayerContext.PlayerIsFirst)
                        {
                            thinkingIndicator.ResetForWaiting();
                        }
                        else
                        {
                            thinkingIndicator.ResetForThinking();
                        }
                    }
                }
                whistleTime = WHISTLE_COOLDOWN;
                drawWhistle = false;
                thinkingIndicator.ResumeElapsing();
            }
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (_isPausedByRemote || _isConnectionProblem)
            {
                return;
            }
            if (drawWhistle)
            {
                return;
            }

            if (input.TouchState.Count > 0 && gestureMoveAdded == true)
            {
                return;
            }
            else
            {
                gestureMoveAdded = false;
            }
            if (input.Gestures.Count > 0)
            {
                if (animationStarted)
                {
                    ResetAnimationValue();
                }
                if (firstPlayerCoach.Active || secondPlayerCoach.Active)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (pickedPipTalkInBaloons[i].WasPressed(input.Gestures[0].Position))
                        {
                            if (GlobalMultiplayerContext.PlayerIsFirst)
                            {
                                firstPlayerCoach.SendCoachMessage(pickedPipTalkInBaloons[i].PipTalkCode);
                                firstPlayerCoach.Active = false;
                            }
                            else
                            {
                                secondPlayerCoach.SendCoachMessage(pickedPipTalkInBaloons[i].PipTalkCode);
                                secondPlayerCoach.Active = false;
                            }
                            return;
                        }
                    }
                    firstPlayerCoach.Active = false;
                    secondPlayerCoach.Active = false;
                    return;
                }

                if (GlobalMultiplayerContext.PlayerIsFirst)
                {
                    firstPlayerCoach.HandleInput(input.Gestures[0].Position);
                }
                else
                {
                    secondPlayerCoach.HandleInput(input.Gestures[0].Position);
                }

                if (GameVariables.Instance.CurrentPlayer.Coach == TeamCoach.HUMAN)
                    if (GameVariables.Instance.CurrentPlayer.Controler == Controlling.BUTTONS)
                    {
                        if (input.Gestures[0].GestureType == GestureType.Tap)
                        {
                            TypeOfMove move = ControllingDPad.HandleInput(input.Gestures[0].Position);
                            if (move != TypeOfMove.UNKNOWN)
                            {
                                int moveResult = board.AddMove(input.Gestures[0].Position, move);
                                if (moveResult != 0)
                                {
                                    NotificatyOtherPlayer(move.ToString(), moveResult);
                                    AudioManager.PlaySound("kick");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (input.Gestures[0].GestureType == GestureType.FreeDrag)
                        {
                            if (input.Gestures[0].Delta.Length() < 15.0f)
                                return;
                            if (input.Gestures[0].Delta.X == 0 && input.Gestures[0].Delta.Y == 0)
                                return;
#if DEBUG
                            Debug.WriteLine(String.Format("Detla = {0}", input.Gestures[0].Delta.Length().ToString()));
#endif
                            TypeOfMove move = GameRules.DetermineTypeOfMoveFromDelta(input.Gestures[0].Delta);
                            int moveResult = board.AddMove(input.Gestures[0].Position, move);
                            if (moveResult != 0)
                            {
                                NotificatyOtherPlayer(move.ToString(), moveResult);
                                gestureMoveAdded = true;
                                AudioManager.PlaySound("kick");
                            }
                        }
                    }
            }
        }

        public void NotificatyOtherPlayer(string move, int moveResult)
        {
            if (OpponentPlayer != null)
            {
                _moveMade = true;
                OpponentPlayer.MoveMade(move);                
                if (moveResult == 1)
                {
                    thinkingIndicator.IsWaiting = true;
                    if (GlobalMultiplayerContext.PlayerIsFirst)
                    {
                        SecondPlayerCoachGo();
                    }
                    else
                    {
                        FirstPlayerCoachGo();
                    }
                }
            }
            else
            {
                if (moveResult == 1)
                {
                    SecondPlayerCoachGo();
                }
            }
        }

        public void UnregisterEventsFromRemotePlayer()
        {
            if (OpponentPlayer != null)
            {
                OpponentPlayer.UnregisterEvents();
            }
        }

        private void FirstPlayerCoachGo()
        {
            firstPlayerCoach.ShowCoachMessage(CoachPipTalkCodes.CheersGo);
            secondPlayerCoach.ShowCoachMessage(CoachPipTalkCodes.NoBaloon);
        }

        private void SecondPlayerCoachGo()
        {
            firstPlayerCoach.ShowCoachMessage(CoachPipTalkCodes.NoBaloon);
            secondPlayerCoach.ShowCoachMessage(CoachPipTalkCodes.CheersGo);
        }
    }
}
