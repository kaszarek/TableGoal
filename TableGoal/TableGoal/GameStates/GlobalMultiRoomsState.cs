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
using Microsoft.Phone.Tasks;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;

namespace TableGoal
{
    class RoomDetails
    {
        public RoomDetails(string id, string name, string owner, PlayField fieldType, bool isGoalLimited, int gameLimit, Color hostColor)
        {
            Id = id;
            Name = name;
            Owner = owner;
            FieldType = fieldType;
            IsGoalLimited = isGoalLimited;
            GameLimit = gameLimit;
            HostColor = hostColor;
        }
        public String Id;
        public String Name;
        public String Owner;
        public PlayField FieldType;
        public bool IsGoalLimited;
        public int GameLimit;
        public Color HostColor;
    }

    class GlobalMultiRoomsState : GameState
    {
        Menu menu;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        int _tableMarigin = 30;
        UIJumpingUIPicture startMatch;
        bool clickAnimationOngoing = false;
        Object padlock;
        bool connectionProblemOccurred = false;
        UIPicture _tellToFriend;
        Dictionary<string, GlobalMultiRoom> _roomsList;
        string _selecterRoomId = string.Empty;

        int ySpeed;
        int yDisplacement;
        bool shouldBounceBack;
        SpriteFont _mainElement;
        SpriteFont _detailsElements;
        UIPicture _horizontalLine;
        UIPicture _verticalLine;
        UIPicture _empty;
        Rectangle screen;
        int upBorder = 50;
        int downBorder = 420;
        int countingDots;
        int _totalDisplacement = 0;

        public GlobalMultiRoomsState()
        {
            this.EnabledGestures = GestureType.FreeDrag | GestureType.Tap | GestureType.Flick;
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 350, 400, 108));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            
            _roomsList = new Dictionary<string, GlobalMultiRoom>();
            _horizontalLine = new UIPicture("WC/hor_line", new Rectangle(0, 0, 0, 0));
            _horizontalLine.Color = Color.Black;
            _verticalLine = new UIPicture("WC/ver_line", new Rectangle(0, 0, 0, 0));
            _verticalLine.Color = Color.Black;
            _empty = new UIPicture("halftrans4x4", new Rectangle(0, 0, 0, 0));
            _empty.Color = Color.White;
            ySpeed = 0;
            shouldBounceBack = false;
            screen = new Rectangle(0, 0, 800, 480);
            countingDots = 0;
            
            startMatch = new UIJumpingUIPicture("whistle", new Rectangle(710, 410, 90, 90), 0.3f, 0.4f, 0.5f, .8f);
            startMatch.Visible = false;
            GlobalMultiProvider.InitializeWarp42();
            GlobalMultiProvider.AssignListeners();
            PlayerWriterReader.LoadFromIsolatedStorage();
            _tellToFriend = new UIPicture("megafon", new Rectangle(300, 380, 202, 90));
            _tellToFriend.Visible = false;
            menu.AddElement(_tellToFriend);
            string playerName = PlayerWriterReader.plInfo.Name;
            string playerId = PlayerWriterReader.plInfo.IdRandomPrefix;
            string userName = String.Format("{0}:{1}", playerId, playerName);
            Debug.WriteLine("Username = " + userName);

            padlock = new object();
            if (GlobalMultiProvider.IsConnected)
            {
                RegisterEvents();
                WarpClient.GetInstance().SubscribeLobby();
                WarpClient.GetInstance().GetRoomsInRange(1, 1);
            }
            else
            {
                UnregisterEvents();
                GlobalMultiplayerContext.warpClient.RemoveConnectionRequestListener(GlobalMultiplayerContext.connectionListenObj);
                GlobalMultiplayerContext.connectionListenObj = new ConnectionListener();
                GlobalMultiplayerContext.warpClient.AddConnectionRequestListener(GlobalMultiplayerContext.connectionListenObj);
                RegisterEvents();
                GlobalMultiProvider.Connect(userName); 
            }
        }

        private void RegisterEvents()
        {
            GlobalMultiplayerContext.notificationListenerObj.JoinRoom += new NotificationListener.JoinRoomEventHandler(notificationListenerObj_JoinRoom);
            GlobalMultiplayerContext.roomReqListenerObj.GotLiveRoomInfo += new RoomReqListener.GotLiveRoomInfoEventHandler(roomReqListenerObj_GotLiveRoomInfo);
            GlobalMultiplayerContext.connectionListenObj.OnConnectionDone += new ConnectionListener.OnConnectionDoneEventHandler(connectionListenObj_OnConnectionDone);
            GlobalMultiplayerContext.zoneListenerObj.GotAllRooms += new ZoneReqListener.GotAllRoomsEventHandler(zoneListenerObj_GotAllRooms);
            GlobalMultiplayerContext.notificationListenerObj.RoomDestroyed += new NotificationListener.RoomDestroyedEventHandler(notificationListenerObj_RoomDestroyed);
            GlobalMultiplayerContext.connectionListenObj.SeriousConnectionProblem += new ConnectionListener.SeriousConnectionProblemEventHandler(connectionListenObj_SeriousConnectionProblem);
        }

        public void UnregisterEvents()
        {
            GlobalMultiplayerContext.notificationListenerObj.JoinRoom -= new NotificationListener.JoinRoomEventHandler(notificationListenerObj_JoinRoom);
            GlobalMultiplayerContext.roomReqListenerObj.GotLiveRoomInfo -= new RoomReqListener.GotLiveRoomInfoEventHandler(roomReqListenerObj_GotLiveRoomInfo);
            GlobalMultiplayerContext.connectionListenObj.OnConnectionDone -= new ConnectionListener.OnConnectionDoneEventHandler(connectionListenObj_OnConnectionDone);
            GlobalMultiplayerContext.zoneListenerObj.GotAllRooms -= new ZoneReqListener.GotAllRoomsEventHandler(zoneListenerObj_GotAllRooms);
            GlobalMultiplayerContext.notificationListenerObj.RoomDestroyed -= new NotificationListener.RoomDestroyedEventHandler(notificationListenerObj_RoomDestroyed);
            GlobalMultiplayerContext.connectionListenObj.SeriousConnectionProblem -= new ConnectionListener.SeriousConnectionProblemEventHandler(connectionListenObj_SeriousConnectionProblem);
        }

        void connectionListenObj_SeriousConnectionProblem()
        {
            GlobalMultiplayerContext.warpClient.Disconnect();
            DiagnosticsHelper.SafeShow("Could not connect to the server. Please try again in a minute.");
            connectionProblemOccurred = true;
        }

        void zoneListenerObj_GotAllRooms()
        {
            Debug.WriteLine(String.Format("There are {0} rooms", GlobalMultiplayerContext.roomsIDs.Count));
            foreach (String room in GlobalMultiplayerContext.roomsIDs)
            {
                if (room != String.Empty)
                {
                    GlobalMultiplayerContext.warpClient.GetLiveRoomInfo(room);
                }
            }
        }

        void connectionListenObj_OnConnectionDone()
        {
            Debug.WriteLine(String.Format("Connected... -> "));
            WarpClient.GetInstance().SubscribeLobby();
            GlobalMultiplayerContext.warpClient.GetRoomsInRange(1, 1);
        }

        void notificationListenerObj_JoinRoom(string roomId)
        {
            if (roomId == _selecterRoomId)
            {
                _selecterRoomId = String.Empty;
            }
            lock (padlock)
            {
                if (_roomsList.Keys.Contains(roomId))
                {
                    _roomsList.Remove(roomId);
                }
            }
        }

        void notificationListenerObj_RoomDestroyed(string roomId)
        {
            if (roomId == _selecterRoomId)
            {
                _selecterRoomId = String.Empty;
            }
            lock (padlock)
            {
                if (_roomsList.Keys.Contains(roomId))
                {
                    _roomsList.Remove(roomId);
                }
            }
        }

        void roomReqListenerObj_GotLiveRoomInfo(LiveRoomInfoEvent roomData)
        {
            if (roomData.getProperties() == null)
            {
                return;
            }
            PlayField field;
            try
            {
                field = (PlayField)Enum.Parse(typeof(PlayField), roomData.getProperties()[KnownRoomProperties.FieldType].ToString(), true);
            }
            catch (Exception)
            {
                // Do not add this room since field did not convert nicely
                return;
            }

            bool isGoalLimited;
            try
            {
                isGoalLimited = bool.Parse(roomData.getProperties()[KnownRoomProperties.GameType].ToString());
            }
            catch (Exception)
            {
                // DO not add this room since limitation of a game did not convert nicely
                return;
            }
            
            int limit;
            try
            {
                limit = int.Parse(roomData.getProperties()[KnownRoomProperties.GameLimit].ToString());
            }
            catch (Exception)
            {
                // Do not add this room since game limit did not convert nicely
                return;
            }
            Color col = ExportColor(roomData.getProperties()[KnownRoomProperties.OpponentColor].ToString());
            lock (padlock)
            {
                //for (int i = 0; i < 10; i++)
                _roomsList.Add(roomData.getData().getId(),// + i.ToString(),
                               new GlobalMultiRoom(new Rectangle(_tableMarigin, _tableMarigin, 800 - 2 * _tableMarigin, 80),
                                                   new RoomDetails(roomData.getData().getId(), roomData.getData().getName(), roomData.getData().getRoomOwner(), field, isGoalLimited, limit, col),
                                                   ref _mainElement,
                                                   ref _detailsElements,
                                                   ref _horizontalLine,
                                                   ref _verticalLine,
                                                   ref _empty,
                                                   _roomsList.Keys.Count,
                                                   _totalDisplacement));
            }
        }

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

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch); 
            if (GlobalMultiProvider.IsConnected && _roomsList.Keys.Count < 4)
            {
                _tellToFriend.Visible = true;
            }
            else
            {
                _tellToFriend.Visible = false;
            }
            if (!GlobalMultiProvider.IsConnected)
            {
                switch (countingDots)
                {
                    case 0:
                        spriteBatch.DrawString(_mainElement,
                                               "Connecting.",
                                               new Vector2(45,
                                                           35),
                                               Color.Black);
                        break;
                    case 1:
                        spriteBatch.DrawString(_mainElement,
                                               "Connecting..",
                                               new Vector2(45,
                                                           35),
                                               Color.Black);
                        break;
                    case 2:
                        spriteBatch.DrawString(_mainElement,
                                               "Connecting...",
                                               new Vector2(45,
                                                           35),
                                               Color.Black);
                        break;
                    default:
                        spriteBatch.DrawString(_mainElement,
                                               "Connecting",
                                               new Vector2(45,
                                                           35),
                                               Color.Black);
                        break;
                }
                spriteBatch.DrawString(_detailsElements,
                                       "Please stay tuned, soon you will see rooms' list.",
                                       new Vector2(60,
                                                   85),
                                       Color.Black);
                spriteBatch.End();
                return;
            }
            if (_roomsList.Keys.Count == 0)
            {
                spriteBatch.DrawString(_mainElement,
                                       "No games available ...",
                                       new Vector2(45,
                                                   35),
                                       Color.Black);
                spriteBatch.DrawString(_detailsElements,
                                       "Host a new game or ask your friend to do it.",
                                       new Vector2(60,
                                                   85),
                                       Color.Black);
            }
            lock (padlock)
            {
                foreach (string k in _roomsList.Keys)
                {
                    _roomsList[k].Draw(spriteBatch);
                }
            }
            startMatch.Draw(spriteBatch);

            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            startMatch.LoadTexture(GameManager.Game.Content);
            _tellToFriend.LoadTexture(GameManager.Game.Content);
            _mainElement = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            _detailsElements = GameManager.Game.Content.Load<SpriteFont>("Fonts/Sketch15");
            _horizontalLine.LoadTexture(GameManager.Game.Content);
            _verticalLine.LoadTexture(GameManager.Game.Content);
            _empty.LoadTexture(GameManager.Game.Content);
        }

        public override void Update(GameTime gameTime)
        {
            menu.Update(gameTime);
            if (_selecterRoomId == string.Empty)
            {
                startMatch.Visible = false;
            }
            if (startMatch.Visible)
            {
                startMatch.DestinationRectangle = new Rectangle(_roomsList[_selecterRoomId].DestinationRectangle.Right - 105, _roomsList[_selecterRoomId].DestinationRectangle.Top, 90, 90);
                startMatch.Position = new Vector2(startMatch.DestinationRectangle.X - startMatch.Offset,
                                                  startMatch.DestinationRectangle.Y - startMatch.Offset) + startMatch.Origin;
            }
            startMatch.Update(gameTime);
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (!GlobalMultiProvider.IsConnected)
            {
                countingDots = gameTime.TotalGameTime.Seconds % 3;
            }
            if (_tellToFriend.Pressed)
            {
                _tellToFriend.Pressed = false;
                AudioManager.PlaySound("selected");
                EmailComposeTask email = new EmailComposeTask();
                if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "pl")
                {
                    email.Subject = "Partyjka w pi³karzyki?";
                    email.Body = "Hej,\n\nMo¿e zagramy meczyk w pi³karzyki (Paper Soccer Online)? Jak coœ to mój login to: " + PlayerWriterReader.plInfo.Name + "\nGrê mo¿esz pobraæ st¹d:\n\n http://www.windowsphone.com/s?appid=2a52b226-048c-453d-bdff-43ce4c18e6b9 \n\nDo zobaczenia w trakcie meczu!";
                }
                else
                {
                    email.Subject = "Wanna play Paper Soccer Online?";
                    email.Body = "Hey,\n\nDo you have 5 minutes to play online match in Paper Soccer Online? My online nick is: " + PlayerWriterReader.plInfo.Name + "\nYou can get the game from here:\n\n http://www.windowsphone.com/s?appid=2a52b226-048c-453d-bdff-43ce4c18e6b9 \n\nSee you online!";
                }
                try
                {
                    email.Show();
                }
                catch (Exception ex)
                {
                }
            }
            if (connectionProblemOccurred)
            {
                AudioManager.PlaySound("selected");
                UnregisterEvents();
                GameManager.RemoveState(this); 
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                UnregisterEvents();
                GameManager.RemoveState(this);
            }
            if (startMatch.Pressed)
            {
                SetGameDetails();
                UnregisterEvents();
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                GameManager.AddState(new GlobalMultiJoinState(_roomsList[_selecterRoomId].FullRoomDetails));
                startMatch.Pressed = false;
            }
            lock (padlock)
            {
                int i = 0;
                _totalDisplacement += (yDisplacement + ySpeed);
                foreach (string k in _roomsList.Keys)
                {
                    _roomsList[k].Index = i;
                    i++;
                    _roomsList[k].AdjustPosition(_totalDisplacement);
                    if (_roomsList[k].DestinationRectangle.Intersects(screen))
                    {
                        _roomsList[k].Visible = true;
                    }
                    else
                    {
                        _roomsList[k].Visible = false;
                    }
                }
            }
            yDisplacement = 0;

            if (_roomsList.Keys.Count < 5)
            {
                _totalDisplacement = 0;
                return;
            }

            /*
             * Jeœli po³o¿enie pierwszego balonika jest wiêksze od 50 pixeli.
             */
            if (_roomsList.Values.First().DestinationRectangle.Top > upBorder)
            {
                shouldBounceBack = true;
                ySpeed = (int)(ySpeed * -0.2);
                    if (ySpeed >= 0)
                        ySpeed = -28;
            }
            /*
             * Jeœli po³o¿enie ostatniego balonika - dowlna krawêdŸ - jest mniejsze od 400).
             */
            else if (_roomsList.Values.Last().DestinationRectangle.Bottom < downBorder)
            {
                    shouldBounceBack = true;
                    ySpeed = (int)(ySpeed * -0.3);
                    if (ySpeed <= 0)
                        ySpeed = 28;
            }
            /*
             * Jeœli ¿adne z poni¿szych to dzia³amy parametrem "tarcia" na prêdkoœæ.
             */
            else
            {
                if (shouldBounceBack)
                {
                    if (Math.Abs(ySpeed) > 23)
                    {
                        ySpeed = (int)(ySpeed * 0.4);
                    }
                    else if (Math.Abs(ySpeed) > 15)
                    {
                        ySpeed = (int)(ySpeed * 0.4);
                    }
                    else if (Math.Abs(ySpeed) > 6)
                    {
                        ySpeed = (int)(ySpeed * 0.4);
                    }
                }
                ySpeed = (int)(ySpeed * 0.96);
                if (Math.Abs(ySpeed) < 2)
                {
                    ySpeed = 0;
                    shouldBounceBack = false;
                }
            }
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (clickAnimationOngoing)
                return;
            if (input.Gestures.Count > 0)
            {
                shouldBounceBack = false;
                ySpeed = 0;
                if (input.Gestures[0].GestureType == GestureType.FreeDrag && _roomsList.Keys.Count > 4)
                {
                    yDisplacement = (int)input.Gestures[0].Delta.Y;
                    yDisplacement = (int)(2.5 * yDisplacement);
                }
                if (input.Gestures[0].GestureType == GestureType.Flick && _roomsList.Keys.Count > 4)
                {
                    /*
                     * Bierzemy delte x z Flickniecia i dzielimy przez 20 - to daje nam predkosc
                     */
                    ySpeed = (int)input.Gestures[0].Delta.Y / 24;
                    if (_roomsList.Values.First().DestinationRectangle.Top > upBorder)
                    {
                        if (ySpeed > 0)
                            ySpeed = -1;
                    }
                    if (_roomsList.Values.Last().DestinationRectangle.Bottom < downBorder)
                    {
                        if (ySpeed < 0)
                            ySpeed = 1;
                    }
                }
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    _selecterRoomId = String.Empty;
                    lock (padlock)
                    {
                        foreach (string k in _roomsList.Keys)
                        {
                            _roomsList[k].Pressed = false;
                            _roomsList[k].HandleInput(input.Gestures[0].Position);
                            if (_roomsList[k].Pressed)
                            {
                                _selecterRoomId = k;
                            }
                        }
                    }
                    menu.WasPressed(input.Gestures[0].Position);
                    if (menu.PressedButton != ButtonType.None)
                    {
                        clickAnimationOngoing = true;
                        AudioManager.PlaySound("selected");
                    }
                    if (startMatch.WasPressed(input.Gestures[0].Position))
                    {
                        startMatch.Pressed = true;
                    }
                    if (_selecterRoomId != String.Empty)
                    {
                        startMatch.DestinationRectangle = new Rectangle(_roomsList[_selecterRoomId].DestinationRectangle.Right - 105, _roomsList[_selecterRoomId].DestinationRectangle.Top, 90, 90);
                        startMatch.Position = new Vector2(startMatch.DestinationRectangle.X - startMatch.Offset,
                                                          startMatch.DestinationRectangle.Y - startMatch.Offset) + startMatch.Origin;               
                        startMatch.Visible = true;
                    }
                    else
                    {
                        startMatch.Visible = false;
                    }
                }
            }
        }

        private void SetGameDetails()
        {
            RoomDetails rd = _roomsList[_selecterRoomId].FullRoomDetails; // TODO -> do poprawy
            if (rd != null)
            {
                GameVariables.Instance.TypeOfField = rd.FieldType;
                GameVariables.Instance.IsLimitedByGoals = rd.IsGoalLimited;
                if (rd.IsGoalLimited)
                {
                    GameVariables.Instance.GoalsLimit = rd.GameLimit;
                }
                else
                {
                    GameVariables.Instance.TimeLeft = rd.GameLimit;
                    GameVariables.Instance.TotalTime = rd.GameLimit;
                }
                GameVariables.Instance.FirstPlayer.ShirtsColor = rd.HostColor;
                GameVariables.Instance.FirstPlayer.Coach = TeamCoach.REMOTEOPPONENT;
            }
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
    }
}
