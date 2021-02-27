using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using com.shephertz.app42.gaming.multiplayer.client.events;

namespace TableGoal
{
    class GlobalMultiLobbyState : GameState
    {
        Menu menu;
        Texture2D ball;
        Rectangle ballOnScreen;
        float rotationAngle = .0f;
        Vector2 offsetForBall;
        Point actualPosition;
        int moveIncrement = 10;
        float rotationChanges = .073f;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        string messageToPlayer;
        SpriteFont _Font;
        bool challengeReceived;
        String challengerName;
        bool challengeRejected;
        bool gameStarts;
        bool roomDestroyed;
        Object padlock;
        volatile bool challengerLeftRoom;


        public GlobalMultiLobbyState()
        {
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 350, 400, 108));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/BackBtn", ButtonType.Back);
            ballOnScreen = new Rectangle(25, 25, 50, 50);
            offsetForBall = new Vector2(25, 25);
            actualPosition = new Point(25, 25);
            rotationChanges = moveIncrement / offsetForBall.X;
            messageToPlayer = "waiting for the opponent . . .";
            GlobalMultiplayerContext.roomReqListenerObj.JoinedRoom += new RoomReqListener.JoinedRoomEventHandler(roomReqListenerObj_JoinedRoom);
            GlobalMultiplayerContext.notificationListenerObj.LeftRoom += new NotificationListener.LeftRoomEventHandler(notificationListenerObj_LeftRoom);
            GlobalMultiplayerContext.notificationListenerObj.OnNewGameMessage += new NotificationListener.OnNewGameMessageEventHandler(notificationListenerObj_OnNewGameMessage);
            GlobalMultiplayerContext.notificationListenerObj.OnChallengeRejected += new NotificationListener.OnChallengeRejectedEventHandler(notificationListenerObj_OnChallengeRejected);
            GlobalMultiplayerContext.notificationListenerObj.OnChallengeAccepted += new NotificationListener.OnChallengeAcceptedEventHandler(notificationListenerObj_OnChallengeAccepted);
            GlobalMultiplayerContext.notificationListenerObj.RoomDestroyed += new NotificationListener.RoomDestroyedEventHandler(notificationListenerObj_RoomDestroyed);
            menu.AddElement(new MultiplayerTips(new Vector2(400, 214)));
            challengeReceived = false;
            challengeRejected = false;
            gameStarts = false;
            roomDestroyed = false;
            challengerLeftRoom = false;
            padlock = new object();
        }

        /// <summary>
        /// Removes event handlers functions from the hooks.
        /// </summary>
        public void UnregisterEvents(bool shouldLeaveAndDestroy)
        {
            if (shouldLeaveAndDestroy)
            {
                GlobalMultiplayerContext.warpClient.LeaveRoom(GlobalMultiplayerContext.GameRoomId);
                if (GlobalMultiplayerContext.PlayerIsFirst)
                {
                    GlobalMultiplayerContext.warpClient.DeleteRoom(GlobalMultiplayerContext.GameRoomId);
                }
            }
            GlobalMultiplayerContext.warpClient.UnsubscribeLobby();
            GlobalMultiplayerContext.roomReqListenerObj.JoinedRoom -= new RoomReqListener.JoinedRoomEventHandler(roomReqListenerObj_JoinedRoom);
            GlobalMultiplayerContext.notificationListenerObj.LeftRoom -= new NotificationListener.LeftRoomEventHandler(notificationListenerObj_LeftRoom);
            GlobalMultiplayerContext.notificationListenerObj.OnNewGameMessage -= new NotificationListener.OnNewGameMessageEventHandler(notificationListenerObj_OnNewGameMessage);
            GlobalMultiplayerContext.notificationListenerObj.OnChallengeRejected -= new NotificationListener.OnChallengeRejectedEventHandler(notificationListenerObj_OnChallengeRejected);
            GlobalMultiplayerContext.notificationListenerObj.OnChallengeAccepted -= new NotificationListener.OnChallengeAcceptedEventHandler(notificationListenerObj_OnChallengeAccepted);
            GlobalMultiplayerContext.notificationListenerObj.RoomDestroyed -= new NotificationListener.RoomDestroyedEventHandler(notificationListenerObj_RoomDestroyed);
        }

        void notificationListenerObj_OnChallengeAccepted()
        {
            gameStarts = true;
        }
        
        void notificationListenerObj_OnChallengeRejected(string challengedPlayer)
        {
            DiagnosticsHelper.SafeShow(String.Format("'{0}' rejected your match request", challengedPlayer.Split(':')[1]));
            challengeRejected = true;
        }

        void notificationListenerObj_OnNewGameMessage(String challenger)
        {
            challengerName = challenger.Split(':')[1];
            challengerLeftRoom = false;
            challengeReceived = true;
        }

        void notificationListenerObj_LeftRoom(string userName)
        {
            challengerLeftRoom = true;
        }

        void roomReqListenerObj_JoinedRoom()
        {
            // gracz który do³¹cza wysy³a wyzwanie graczowi który ju¿ jest w grze
            if (!GlobalMultiplayerContext.PlayerIsFirst)
            {
                GlobalMultiplayerContext.warpClient.SendUpdatePeers(MoveMessage.buildNewGameMessageBytes(GameVariables.Instance.SecondPlayer.ShirtsColor.ToString()));
            }
        }

        void notificationListenerObj_RoomDestroyed(string roomId)
        {
            if (GlobalMultiplayerContext.GameRoomId == roomId)
            {
                lock (padlock)
                {
                    roomDestroyed = true;
                }
                DiagnosticsHelper.SafeShow("Host removed the room.");
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            spriteBatch.Draw(ball,
                             ballOnScreen,
                             null,
                             Color.White,
                             rotationAngle,
                             offsetForBall,
                             SpriteEffects.None,
                             0.5f);

            spriteBatch.DrawString(_Font,
                                   messageToPlayer,
                                   new Vector2(100, 139),
                                   Color.Black);
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            ball = GameManager.Game.Content.Load<Texture2D>("ball");
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
        }

        public override void Update(GameTime gameTime)
        {
            lock (padlock)
            {
                if (roomDestroyed)
                {
                    ShowMainMenu();
                    return;
                }
            }
            if (challengeReceived)
            {
                challengeReceived = false;
                System.Windows.MessageBoxResult result = DiagnosticsHelper.SafeShow(String.Format("'{0}' wants to beat you." + Environment.NewLine + "Ok to accept, Cancel to reject", challengerName), "Match request", System.Windows.MessageBoxButton.OKCancel);
                if (result == System.Windows.MessageBoxResult.OK)
                {
                    if (challengerLeftRoom == true)
                    {
                        challengerLeftRoom = false;
                        DiagnosticsHelper.SafeShow(String.Format("'{0}' left the room in the meanwhile.", challengerName));
                        return;
                    }
                    GlobalMultiplayerContext.warpClient.SendUpdatePeers(MoveMessage.buildChallengeAcceptedMessageBytes());
                    notificationListenerObj_OnChallengeAccepted();
                }
                else if (result == System.Windows.MessageBoxResult.Cancel)
                {
                    GlobalMultiplayerContext.warpClient.SendUpdatePeers(MoveMessage.buildChallengeRejectedMessageBytes());
                }
                challengerName = String.Empty;
            }
            if (challengeRejected)
            {
                GlobalMultiplayerContext.warpClient.LeaveRoom(GlobalMultiplayerContext.GameRoomId);
                ShowMainMenu();
            }
            if (gameStarts)
            {
                StartGame();
                gameStarts = false;
            }
            MoveBall();
            menu.Update(gameTime);
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                ShowMainMenu();
            }
            if (menu.PressedButton == ButtonType.Back)
            {
                ShowMainMenu();
            }
        }

        private void StartGame()
        {
            Statistics.Instance.ZaczynamKolejnyMecz();
            UnregisterEvents(false);
            GameState[] states = GameManager.GetStates();
            foreach (GameState state in states)
                GameManager.RemoveState(state);
            GameManager.AddState(new GlobalMultiGameplayState());
        }

        private void ShowMainMenu()
        {
            UnregisterEvents(true);
            GameState[] states = GameManager.GetStates();
            foreach (GameState state in states)
                if (!(state is MainMenuState) &&
                    !(state is NewGameMenu) &&
                    !(state is MultiplayerState))
                GameManager.RemoveState(state);
        }

        private void MoveBall()
        {

            rotationAngle -= (rotationChanges);
            if (rotationAngle > MathHelper.TwoPi)
                rotationAngle -= MathHelper.TwoPi;
            if (actualPosition.Y <= 25)
                actualPosition.X += moveIncrement;
            if (actualPosition.X >= 775)
                actualPosition.Y += moveIncrement;
            if (actualPosition.Y >= 455)
                actualPosition.X -= moveIncrement;
            if (actualPosition.X <= 25)
                actualPosition.Y -= moveIncrement;

            ballOnScreen.Location = new Point(actualPosition.X, actualPosition.Y);
        }

        public void ButtonClicked(GameTime gameTime)
        {
            menuCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (menuCooldown <= 0.0f)
            {
                menuCooldown = MENUCOOLDOWN;
                clickAnimationOngoing = false;
            }
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (clickAnimationOngoing)
                return;
            if (input.Gestures.Count > 0)
            {
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    menu.WasPressed(input.Gestures[0].Position);
                    if (menu.PressedButton != ButtonType.None)
                    {
                        clickAnimationOngoing = true;
                        AudioManager.PlaySound("selected");
                    }
                }
            }
        }
    }
}
