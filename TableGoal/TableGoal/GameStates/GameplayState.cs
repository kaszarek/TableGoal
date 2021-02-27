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
    public class GameplayState : GameState
    {
        int totalTime = 0;
        int actualTime = 0;
        double timePassed = 0;
        Texture2D _fingerMark;
        Texture2D _whistle;
        Texture2D _gearwheel_brainwornikg;
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
        bool moveAdded = false;
        int alphaValue = 255;
        int fadeIncrement = 10;
        double fadeDelay = .03;
        bool animationStarted = false;
        Board board;
        bool isResuming = false;
        AIPlayer AI;
        WifiPlayer OpponentPlayer;
        double moveTimeForAi = 0.2;
        bool gameFinished = false;
        float ROTATION_ANGLE = 0;
        OnScreenControls ControllingDPad;
        Color resultColor = Color.Black;
        Random rand = new Random(DateTime.Now.Millisecond);
        bool sureToExit = false;
        RunningTimeIndicator thinkingIndicator;
        Coach firstPlayerCoach;
        Coach secondPlayerCoach;

        public GameplayState(bool resuming)
        {
            Field.CreateFields();
            string pathToField = Field.GetPathToBackground();
            ControllingDPad = new OnScreenControls();
            _screenSize = new Vector2(800, 480);
            this.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
            board = new Board(pathToField);
            if (!resuming)
                AudioManager.PlaySound("whistle");
            _thinking_indicator_RightSide = new Rectangle(780, 80, 40, 40);
            _thinking_indicator_LeftSide = new Rectangle(20, 415, 40, 40);
            _offset_thinking_indicator = new Vector2(82, 82);
            thinkingIndicator = new RunningTimeIndicator(new Rectangle(0, 466, 300, 14), GameVariables.Instance.FirstPlayer.ShirtsColor, new Rectangle(500, 466, 300, 14), GameVariables.Instance.SecondPlayer.ShirtsColor, 35, false);
            thinkingIndicator.WaitingTimeLimit = 38;
            thinkingIndicator.ThinkingTimeIsUp += new RunningTimeIndicator.ThinkigTimeIsUpEventHandler(thinkingIndicator_ThinkingTimeIsUp);
            thinkingIndicator.WaitingTimeIsUp += new RunningTimeIndicator.WaitingTimeIsUpEventHandler(thinkingIndicator_WaitingTimeIsUp);

            firstPlayerCoach = new Coach(new Rectangle(10, 295, 30, 100), new Rectangle(0, 0, 50, 120), GameVariables.Instance.FirstPlayer.ShirtsColor, true);
            secondPlayerCoach = new Coach(new Rectangle(760, 100, 30, 100), new Rectangle(50, 0, 50, 120), GameVariables.Instance.SecondPlayer.ShirtsColor, false);

            if (!GameVariables.Instance.IsWiFiGame())
            {
                AI = new AIPlayer(ref board);
                AI.AiFinishedTurn += new AIPlayer.AiFinishedTurnEventHandler(AI_AiFinishedTurn);
            }
            else
            {
                thinkingIndicator.IsActive = true;
                OpponentPlayer = new WifiPlayer();
                OpponentPlayer.MoveFromWifiPlayerReceived += new WifiPlayer.MovedFromWifiPlayerReceivedEventHandler(OpponentPlayer_MoveFromWifiPlayerReceived);
                TableGoal.GamePlay.LeftGame += new LeftGameHandler(GamePlay_LeftGame);
                TableGoal.GamePlay.ConnectionProblem += new ConnectionProblemEventHandler(GamePlay_ConnectionProblem);
                TableGoal.GamePlay.OpponentLostMove += new OpponentLostMoveEventHandler(GamePlay_OpponentLostMove);
                if (TableGoal.GamePlay.IsHost)
                {
                    WHISTLE_COOLDOWN += 0.7f;
                    thinkingIndicator.IsThinking = true;
                    thinkingIndicator.WarningTimeStarted += new RunningTimeIndicator.WarningTimeStartedEventHandler(firstPlayerCoach.ShowWarningCountdown);
                    TableGoal.GamePlay.TrashTalkReceived += new TrashTalkMessageReceivedEventHandler(secondPlayerCoach.ShowCoachMessage);
                }
                else
                {
                    thinkingIndicator.IsWaiting = true;
                    thinkingIndicator.WarningTimeStarted += new RunningTimeIndicator.WarningTimeStartedEventHandler(secondPlayerCoach.ShowWarningCountdown);
                    TableGoal.GamePlay.TrashTalkReceived += new TrashTalkMessageReceivedEventHandler(firstPlayerCoach.ShowCoachMessage);
                }
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

            isResuming = resuming;
            totalTime = GameVariables.Instance.TimeLeft;
            actualTime = totalTime;
            if (isResuming)
            {
                this.ScreenState = global::TableGoal.ScreenState.Visible;
                drawWhistle = false;
            }
            _score = GameVariables.Instance.GetScore();
            whistleTime = WHISTLE_COOLDOWN;
        }

        void AI_AiFinishedTurn()
        {
            FirstPlayerCoachGo();
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

        void thinkingIndicator_ThinkingTimeIsUp()
        {
            if (OpponentPlayer != null)
            {
                OpponentPlayer.LostMyTurn();
            }
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

        void OpponentPlayer_MoveFromWifiPlayerReceived(int opponetMovesHistoryCounts, TypeOfMove opponentMove)
        {
            if (board.MovesHistory.Count == opponetMovesHistoryCounts - 1)
            {
                int moveResult = board.AddMove(opponentMove);
                AudioManager.PlaySound("kick");
                if (moveResult == 1)
                {
                    thinkingIndicator.IsThinking = true;
                    if (TableGoal.GamePlay.IsHost)
                    {
                        FirstPlayerCoachGo();
                    }
                    else
                    {
                        SecondPlayerCoachGo();
                    }
                }
#if DEBUG
                Debug.WriteLine(String.Format("Move {0}, added with result {1}", opponentMove, moveResult.ToString()));
#endif
            }
        }

        private void UnregisterEvents()
        {
            UnregisterEventsFromRemotePlayer();
            TableGoal.GamePlay.LeftGame -= new LeftGameHandler(GamePlay_LeftGame);
            TableGoal.GamePlay.ConnectionProblem -= new ConnectionProblemEventHandler(GamePlay_ConnectionProblem);
            TableGoal.GamePlay.OpponentLostMove -= new OpponentLostMoveEventHandler(GamePlay_OpponentLostMove);
            OpponentPlayer.MoveFromWifiPlayerReceived -= new WifiPlayer.MovedFromWifiPlayerReceivedEventHandler(OpponentPlayer_MoveFromWifiPlayerReceived);
            if (TableGoal.GamePlay.IsHost)
            {
                thinkingIndicator.WarningTimeStarted -= new RunningTimeIndicator.WarningTimeStartedEventHandler(firstPlayerCoach.ShowWarningCountdown);
                TableGoal.GamePlay.TrashTalkReceived -= new TrashTalkMessageReceivedEventHandler(secondPlayerCoach.ShowCoachMessage);
            }
            else
            {
                thinkingIndicator.WarningTimeStarted -= new RunningTimeIndicator.WarningTimeStartedEventHandler(secondPlayerCoach.ShowWarningCountdown);
                TableGoal.GamePlay.TrashTalkReceived -= new TrashTalkMessageReceivedEventHandler(firstPlayerCoach.ShowCoachMessage);
            }
        }

        void GamePlay_OpponentLostMove(object sender, EventArgs e)
        {
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

        void GamePlay_ConnectionProblem(object sender, EventArgs e)
        {
            UnregisterEvents();
            Statistics.Instance.Przerwany();
            if (!gameFinished)
            {
                gameFinished = true;
                TableGoal.GamePlay.ArtificialSilentLeave();
                if (!GameVariables.Instance.IsLimitedByGoals)
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TotalTime);
                else
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TimeLeft);
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
                GameManager.AddState(new MainMenuState());
                TableGoal.GamePlay.UnhookExceptionFlagForChannel();
            }
        }

        void GamePlay_LeftGame(object sender, PlayerEventArgs e)
        {
            UnregisterEvents();
            Statistics.Instance.Przerwany();
            if (!gameFinished)
            {
                gameFinished = true;
                DiagnosticsHelper.SafeShow(String.Format("{0} has quit the game...", e.playerInfo.PlayerName));
                TableGoal.GamePlay.Leave(false);
                if (GameVariables.Instance.IsLimitedByGoals)
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TimeLeft);
                else
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TotalTime - GameVariables.Instance.TimeLeft);
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
                GameManager.AddState(new MainMenuState());
            }
        }

        private void InternalOpponentLeave(PlayerInfo pi)
        {
            PlayerEventArgs args = new PlayerEventArgs();
            args.playerInfo = pi;
            GamePlay_LeftGame(this, args);
        }

        public override void LoadContent()
        {
            board.LoadTexture(GameManager.Game.Content);
            _fingerMark = GameManager.Game.Content.Load<Texture2D>("FingerMark");
            _whistle = GameManager.Game.Content.Load<Texture2D>("whistle");
            _gearwheel_brainwornikg = GameManager.Game.Content.Load<Texture2D>("gearwheel");
            thinkingIndicator.LoadTexture(GameManager.Game.Content);
            _scoreFont = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            _scoreSize = _scoreFont.MeasureString(_score);
            _screenSize = new Vector2(GameManager.Game.GraphicsDevice.Viewport.Width,
                                      GameManager.Game.GraphicsDevice.Viewport.Height);
            ControllingDPad.LoadTexture(GameManager.Game.Content);
            firstPlayerCoach.LoadTexture(GameManager.Game.Content);
            secondPlayerCoach.LoadTexture(GameManager.Game.Content);
            base.LoadContent();
        }

        public void StopAI()
        {
            if (AI != null)
                AI.CancelMove();
        }

        public void Restart()
        {
            StopAI();
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
            #region Check if game has ended
            // jeœli gramy do limitu goli
            if (GameVariables.Instance.IsLimitedByGoals)
            {
                if (GameVariables.Instance.FirstPlayer.Goals >= GameVariables.Instance.GoalsLimit ||
                    GameVariables.Instance.SecondPlayer.Goals >= GameVariables.Instance.GoalsLimit)
                {
                    StopAI();
                    if (OpponentPlayer != null)
                        UnregisterEvents();
                    if (!isResuming && !gameFinished)
                    {
                        AudioManager.PlaySound("end");
                        drawWhistle = true;
                        gameFinished = true;
                    }
                    if (!drawWhistle)
                    {
                        if (GameVariables.Instance.IsWiFiGame())
                            GameManager.AddState(new WifiEndGameState());
                        else
                            GameManager.AddState(new GameFinishedState(false));
                        this.ScreenState = global::TableGoal.ScreenState.Visible;
                        /*
                         * To jest po to, ¿e gdy wracaj¹c do gry, która jest roztrzygnieta i wciskaj¹c raz dodatkowo
                         * klawisz Back to mieliœmy FinishedGameState a na nim PausedState.
                         */
                        goto skippedBackButtonHandling;
                    }
                }
            }
            else
            {
                if (actualTime == 0)
                {
                    StopAI();
                    if (OpponentPlayer != null)
                        UnregisterEvents();
                    if (!isResuming && !gameFinished)
                    {
                        AudioManager.PlaySound("end");
                        drawWhistle = true;
                        gameFinished = true;
                    } 
                    if (!drawWhistle)
                    {
                        if (GameVariables.Instance.IsWiFiGame())
                            GameManager.AddState(new WifiEndGameState());
                        else
                            GameManager.AddState(new GameFinishedState(false));
                        this.ScreenState = global::TableGoal.ScreenState.Visible;
                        goto skippedBackButtonHandling;
                    }
                }
            }
            #endregion

            if (OpponentPlayer != null)
                if (OpponentPlayer.CommunicationStopped)
                    InternalOpponentLeave(TableGoal.GamePlay.CurrentOpponent);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                StopAI();
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
                else
                {
                    GameManager.AddState(new PauseState(false));
                    this.ScreenState = global::TableGoal.ScreenState.Visible;
                }
                AudioManager.PlaySound("selected");                
            }

            if (sureToExit)
            {
                if (GameVariables.Instance.IsLimitedByGoals)
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TimeLeft);
                else
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TotalTime - GameVariables.Instance.TimeLeft);
                Statistics.Instance.Przerwany();
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                UnregisterEventsFromRemotePlayer();
                TableGoal.GamePlay.LeaveGame();
                TableGoal.GamePlay.Leave(false);

                GameManager.RemoveState(this);
                GameManager.AddState(new MainMenuState());
            }

            skippedBackButtonHandling:

            #region Sprawdzanie czy pad³ gol, albo blok
            // jeœli juz zosta³y wykonane jakieœ ruchy
            if (!drawWhistle)
            {
                if (board.MovesHistory.Count > 0)// && !isResuming)// TODO: In case goals are scored double while resuming
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
                                GameVariables.Instance.SecondScored();
                            else
                                GameVariables.Instance.FirstScored();
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
                if (board.MovesHistory.Count > 0)// && !isResuming) // TODO: In case goals are scored double while resuming
                {
                    foreach (Vector2 cornerPoint in board.CornerPoints)
                    {
                        // jeœli ktoœ wszed³ w róg
                        if (board.MovesHistory[board.MovesHistory.Count - 1].EndPosition == cornerPoint)
                        {
                            // jesli ostatnim kto zrobi³ ruch by³ gracz pierwszy to punkt jest dla gracza drugiego
                            if (board.MovesHistory[board.MovesHistory.Count - 1].Player == TypeOfPlayer.First)
                                GameVariables.Instance.SecondScored();
                            else // w przeciwnym razie dla gracza pierwszego
                                GameVariables.Instance.FirstScored();
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
                if (board.CheckAvailableMoves().Count == 0)// && !isResuming) // TODO: In case goals are scored double while resuming
                {
                    if (board.MovesHistory.Count > 0)
                    {
                        if (board.MovesHistory[board.MovesHistory.Count - 1].Player == TypeOfPlayer.First)
                            GameVariables.Instance.SecondScored();
                        else
                            GameVariables.Instance.FirstScored();
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
                        moveAdded = false;
                        animationStarted = false;
                    }
                }
            }

            // cpu moves
            if (GameVariables.Instance.CurrentPlayer.Coach == TeamCoach.CPU)
            {
                if (!AI.isThinking() && !drawWhistle)
                {
                    if (moveTimeForAi <= 0)
                    {
                        AI.MakeMove();
                        moveTimeForAi = 0.3;
                    }
                    else
                        moveTimeForAi -= gameTime.ElapsedGameTime.TotalSeconds;
                }
                ROTATION_ANGLE += (0.05f);// * (float)gameTime.TotalGameTime.TotalSeconds);
                if (ROTATION_ANGLE >= MathHelper.TwoPi)
                    ROTATION_ANGLE -= MathHelper.TwoPi;
            }

            if (GameVariables.Instance.CurrentPlayer.Coach != TeamCoach.HUMAN)
            {
                ROTATION_ANGLE += (0.05f);// * (float)gameTime.TotalGameTime.TotalSeconds);
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
            
            /*
             * Aby podczas wznawiania nie (podczas gry na limit goli) nie zresetowa³o licznika do zera
             */
            if (!isResuming)
                // zapamiêtanie ile czasu do koñca
                GameVariables.Instance.TimeLeft = actualTime;

            if (OpponentPlayer != null)
            {
                if (!gameFinished)
                    OpponentPlayer.NotifyAboutMoves(gameTime.ElapsedGameTime.TotalSeconds, board.MovesHistory.Count);
            }

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
            if (isResuming)
            {
                board.LoadExtraFieldTexture(GameManager.Game.Content);
                board.Resuming();
                if (AI != null)
                    AI.CheckDifficultyLevel();
                _score = GameVariables.Instance.GetScore();
                _scoreSize = _scoreFont.MeasureString(_score);
                actualTime = GameVariables.Instance.TimeLeft;
                if (GameVariables.Instance.IsLimitedByGoals)
                {
                    timePassed = actualTime;
                    totalTime = 0;
                }
                else
                {
                    totalTime = actualTime;
                }
                isResuming = false;
            }

            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            board.Draw(spriteBatch);

            // rusyje trenerów - wskaŸnik, kto ma teraz ruch 
            firstPlayerCoach.Draw(spriteBatch);
            secondPlayerCoach.Draw(spriteBatch);

            // rysowanie gestu myŸniêcia palcem
            if (board.MovesHistory.Count > 0)
            {
                Team previous = GameVariables.Instance.GiveTeamFromPlayer(board.MovesHistory[board.MovesHistory.Count - 1].Player);
                if (previous.Controler == Controlling.GESTURES)
                {
                    Color markColor = Color.Gray;
                    markColor = previous.ShirtsColor;
                    if (moveAdded == true)
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

            if (GameVariables.Instance.CurrentPlayer.Coach == TeamCoach.CPU)
                spriteBatch.Draw(_gearwheel_brainwornikg,
                                 _thinking_indicator_RightSide,
                                 null,
                                 Color.Black,
                                 ROTATION_ANGLE,
                                 _offset_thinking_indicator,
                                 SpriteEffects.None,
                                 0.5f);
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

            base.Draw(gameTime);
        }

        
        private void DrawWhistle(SpriteBatch spriteBatch, double elapsedSeconds)
        {
            if (whistleTime > 0)
            {
                int xMove = rand.Next(1, 15);
                int yMove = rand.Next(1, 15);
                spriteBatch.Draw(_whistle, new Rectangle(300 + xMove, 120 + yMove, 200, 200), Color.White);
                whistleTime -= (float)elapsedSeconds;
            }
            else
            {
                board.Reset();
                whistleTime = WHISTLE_COOLDOWN;
                drawWhistle = false;
            }
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (drawWhistle)
                return;
            if (input.TouchState.Count > 0 && moveAdded == true)
                return;
            else
            {
                moveAdded = false;
            }
            if (input.Gestures.Count > 0)
            {
                if (animationStarted)
                {
                    ResetAnimationValue();
                }

                if (GameVariables.Instance.IsWiFiGame())
                {
                    if (TableGoal.GamePlay.IsHost)
                    {
                        firstPlayerCoach.HandleInput(input.Gestures[0].Position);
                    }
                    else
                    {
                        secondPlayerCoach.HandleInput(input.Gestures[0].Position);
                    }
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
                                moveAdded = true;
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
                OpponentPlayer.MoveMade(move, board.MovesHistory.Count);
                if (moveResult == 1)
                {
                    thinkingIndicator.IsWaiting = true;
                    if (TableGoal.GamePlay.IsHost)
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
                OpponentPlayer.CancelMove();
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
