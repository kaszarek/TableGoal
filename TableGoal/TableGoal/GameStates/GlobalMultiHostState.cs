using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.GamerServices;
using com.shephertz.app42.gaming.multiplayer.client;
using System.Diagnostics;

namespace TableGoal
{
    class GlobalMultiHostState : GameState
    {
        Menu menu;
        ColorSelector colSelector1st;
        UIShirt shirt1st;
        UIClock clock;
        UIBall ball;
        bool b_number_of;
        UIPicture number_of;
        bool b_units;
        bool b_ball;
        UIPicture units;
        UIButton btn1st;
        Digit goalsLimit;
        Random coin;
        SpriteFont _Font;
        string playerName = String.Empty;
        Rectangle playersNameBorder;
        Vector2 _size;
        bool connectionProblemOccurred = false;
        UIPicture _tellToFriend;

        /// <summary>
        /// Constructor
        /// </summary>
        public GlobalMultiHostState()
        {
            colSelector1st = new ColorSelector(new Rectangle(20, 330, 0, 0), ColorSelOrientation.HORIZONTAL);

            menu = new Menu("Backgrounds/Background", new Rectangle(200, 50, 400, 380));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);

            shirt1st = new UIShirt("Shirt", new Rectangle(125, 60, 150, 150), .75f);
            shirt1st.Color = colSelector1st.SelectedColor;
            shirt1st.Visible = false;
            menu.AddElement(shirt1st);
            GameVariables.Instance.FirstPlayer.ShirtsColor = shirt1st.Color;

            UIPicture host = new UIPicture("HostLbl", new Rectangle(125, 10, 150, 75));
            menu.AddElement(host);

            btn1st = new UIButton("StartReadyBtn", new Rectangle(80, 200, 240, 120));
            btn1st.Visible = false;
            menu.AddElement(btn1st);

            _tellToFriend = new UIPicture("megafon", new Rectangle(530, 380, 202, 90));
            _tellToFriend.Visible = false;
            menu.AddElement(_tellToFriend);                        
            
            UIPicture line = new UIPicture("empty4x4", new Rectangle(399, 0, 2, 480));
            line.Color = Color.Black;
            menu.AddElement(line);

            // zegarek do zmiany czasu trwania rozrywki
            clock = new UIClock("clock", new Rectangle(360, 20, 80, 80), .8f);
            if (!GameVariables.Instance.IsLimitedByGoals)
            {
                menu.AddElement(clock);
                units = new UIPicture("duration", new Rectangle(390, 110, 75, 26));
                units.SourceRectangle = new Rectangle(0, 0, 100, 35);
                b_units = units.Pressed;
                menu.AddElement(units);
                number_of = new UIPicture("duration", new Rectangle(350, 110, 38, 26));
                number_of.SourceRectangle = new Rectangle(100, 0, 50, 35);
                b_number_of = number_of.Pressed;
                menu.AddElement(number_of);
            }

            if (GameVariables.Instance.IsLimitedByGoals)
            {
                // pi³ka do zmiany limitu goli
                ball = new UIBall("ball", new Rectangle(365, 20, 80, 80), 1.4f);
                b_ball = ball.Pressed;
                menu.AddElement(ball);
                goalsLimit = new Digit(new Rectangle(355, 110, 38, 20), 1, true, 10);
                goalsLimit.IsZeroAllowed = false;
                menu.AddElement(goalsLimit);
                units = new UIPicture("goalgoals", new Rectangle(405, 110, 90, 30));
                units.SourceRectangle = new Rectangle(0, 0, 90, 35);
                b_units = units.Pressed;
                menu.AddElement(units);
            }
            coin = new Random(DateTime.Now.Millisecond);
            PlayerWriterReader.LoadFromIsolatedStorage();
            playerName = PlayerWriterReader.plInfo.Name;
            string playerId = PlayerWriterReader.plInfo.IdRandomPrefix;
            playersNameBorder = new Rectangle(450, 220, 200, 30);

            GlobalMultiProvider.InitializeWarp42();
            GlobalMultiProvider.AssignListeners();
            string userName = String.Format("{0}:{1}", playerId, playerName);
            Debug.WriteLine("Username = " + userName);

            if (!GlobalMultiProvider.IsConnected)
            {
                GlobalMultiplayerContext.warpClient.RemoveConnectionRequestListener(GlobalMultiplayerContext.connectionListenObj);
                GlobalMultiplayerContext.connectionListenObj = new ConnectionListener();
                GlobalMultiplayerContext.warpClient.AddConnectionRequestListener(GlobalMultiplayerContext.connectionListenObj);
                GlobalMultiProvider.Connect(userName);
            }
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            GlobalMultiplayerContext.connectionListenObj.SeriousConnectionProblem += new ConnectionListener.SeriousConnectionProblemEventHandler(connectionListenObj_SeriousConnectionProblem);
        }

        public void UnregisterEvents()
        {
            GlobalMultiplayerContext.connectionListenObj.SeriousConnectionProblem -= new ConnectionListener.SeriousConnectionProblemEventHandler(connectionListenObj_SeriousConnectionProblem);
        }

        void connectionListenObj_SeriousConnectionProblem()
        {
            DiagnosticsHelper.SafeShow("Could not connect to the server. Please try again in a minute.");
            connectionProblemOccurred = true;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            colSelector1st.Draw(spriteBatch);
            spriteBatch.DrawString(_Font,
                                   "Your name:",
                                   new Vector2(475, 150),
                                   Color.Black);
            spriteBatch.DrawString(_Font,
                                   playerName,
                                   new Vector2(600 - _size.X/2, 200),
                                   Color.Black);
            if (!GlobalMultiProvider.IsConnected)
            {
                int mod = gameTime.TotalGameTime.Seconds % 3;
                switch (mod)
                {
                    case 0:
                        spriteBatch.DrawString(_Font,
                                               "Connecting.",
                                               new Vector2(80, 180),
                                               Color.Black);
                        break;
                    case 1:
                        spriteBatch.DrawString(_Font,
                                               "Connecting..",
                                               new Vector2(80, 180),
                                               Color.Black);
                        break;
                    case 2:
                        spriteBatch.DrawString(_Font,
                                               "Connecting...",
                                               new Vector2(80, 180),
                                               Color.Black);
                        break;
                    default:
                        spriteBatch.DrawString(_Font,
                                               "Connecting",
                                               new Vector2(80, 180),
                                               Color.Black);
                        break;
                }
            }
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            colSelector1st.LoadTexture(GameManager.Game.Content);
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            _size = _Font.MeasureString(playerName);
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (input.Gestures.Count > 0)
            {
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    menu.WasPressed(input.Gestures[0].Position);
                    // gdy przyscisk start NIE jest klikniêty to wtedy zbieramy input z colorSelectora
                    if (!btn1st.Pressed)
                    {
                        colSelector1st.HandleInput(input.Gestures[0].Position);
                        GameVariables.Instance.FirstPlayer.ShirtsColor = colSelector1st.SelectedColor;
                    }
                    else // jeœli u¿ytwkonik kliknie w start, a nie w koszulkê, to musi to zadzia³aæ tak jakby klikn¹³ w koszulkê
                    {
                        shirt1st.Pressed = true;
                    }
                    // i tu podobnie. Jak kliknie w koszulkê, to wtedy start te¿ jest wciœniêty
                    if (shirt1st.Pressed)
                        btn1st.Pressed = true;
                    
                    // gdy number_of zosta³ klikniêty, to wartoœæ Pressed jest inna ni¿ cache w b_number_of
                    // wtedy programatycznie klikamy w clock i synchronizujemy Pressed ze zmienn¹ bool
                    if (!GameVariables.Instance.IsLimitedByGoals)
                    {
                        if (number_of.Pressed != b_number_of)
                        {
                            number_of.Pressed = b_number_of;
                            clock.ProgramicClickOnClock();
                        }
                        if (units.Pressed != b_units)
                        {
                            units.Pressed = b_units;
                            clock.ProgramicClickOnClock();
                        }

                        int which_picture = clock.TimeOfPlayInMinutes() / 5;
                        number_of.SourceRectangle = new Rectangle(50 + which_picture * 50, 0, 50, 35);
                    }
                    else
                    {
                        // jeœli zegar by³ klikniêty i goal nie by³ klikniêty
                        if (ball.Pressed != b_ball)
                        {
                            ball.Pressed = b_ball;
                            goalsLimit.ProgramicClickOnDigit(false);
                        }
                        if (goalsLimit.Pressed != b_number_of)
                        {
                            goalsLimit.Pressed = b_number_of;
                            ball.ProgramicClickOnBall();
                        }
                        if (units.Pressed != b_units)
                        {
                            units.Pressed = b_units;
                            goalsLimit.ProgramicClickOnDigit(false);
                            ball.ProgramicClickOnBall();
                        }
                        if (goalsLimit.Number == 1)
                            units.SourceRectangle = new Rectangle(0, 0, 90, 35);
                        else
                            units.SourceRectangle = new Rectangle(90, 0, 90, 35);
                    }

                    if (playersNameBorder.Contains(new Point((int)input.Gestures[0].Position.X, (int)input.Gestures[0].Position.Y)))
                    {
                        if (!Guide.IsVisible)
                        Guide.BeginShowKeyboardInput(PlayerIndex.One, "Choose your new name", "maximum length is 15 characters.", playerName, ShowKeybordCallback, playerName);
                    }
                }
            }
        }

        private void ShowKeybordCallback(IAsyncResult result)
        {
            string res = Guide.EndShowKeyboardInput(result);
            if (res != null)
            {
                res = res.Trim();
                if (res.Length == 0)
                    return;
                /*
                 * To powinno zapobiec wpisywaniu nazw ze znakami z akcentem.
                 * Przede wszystkim problem z nimi jest taki, ¿e czcionka ich nie obs³uguje.
                 */
                bool isAlphaNumeric = System.Text.RegularExpressions.Regex.IsMatch(
                    res.Replace(' ', 'a').Replace('.', 'a'), "^[a-zA-Z0-9_]*$");

                if (!isAlphaNumeric)
                    return;

                if (res.Length > 15)
                    playerName = res.Substring(0, 15);
                else if (res.Length > 0)
                    playerName = res;

                if (playerName != PlayerWriterReader.plInfo.Name)
                {
                    PlayerWriterReader.plInfo.Name = playerName;
                    PlayerWriterReader.SaveToIsolatedStorage();
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!GlobalMultiProvider.IsConnected)
            {
                btn1st.Visible = false;
                shirt1st.Visible = false;
                _tellToFriend.Visible = false;
            }
            else
            {
                btn1st.Visible = true;
                shirt1st.Visible = true;
                _tellToFriend.Visible = true;
            }
            if (connectionProblemOccurred)
            {
                UnregisterEvents();
                GameManager.RemoveState(this);
                AudioManager.PlaySound("selected"); 
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                UnregisterEvents();
                GameManager.RemoveState(this);
                AudioManager.PlaySound("selected");
            }
            if (_tellToFriend.Pressed)
            {
                _tellToFriend.Pressed = false;
                AudioManager.PlaySound("selected");
                EmailComposeTask email = new EmailComposeTask();
                if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "pl")
                {
                    email.Subject = "Partyjka w pi³karzyki?";
                    email.Body = "Hej,\n\nMo¿e zagramy meczyk w pi³karzyki (Paper Soccer Online)? W³aœnie tworzê grê online, a mój login to: " + PlayerWriterReader.plInfo.Name + "\nGrê mo¿esz pobraæ st¹d:\n\n http://www.windowsphone.com/s?appid=2a52b226-048c-453d-bdff-43ce4c18e6b9 \n\nDo zobaczenia w trakcie meczu!";
                }
                else
                {
                    email.Subject = "Wanna play Paper Soccer Online?";
                    email.Body = "Hey,\n\nDo you have 5 minutes to play online match in Paper Soccer Online? I am about to create online game with nick being: " + PlayerWriterReader.plInfo.Name + "\nYou can get the game from here:\n\n http://www.windowsphone.com/s?appid=2a52b226-048c-453d-bdff-43ce4c18e6b9 \n\nSee you online!";
                }
                try
                {
                    email.Show();
                }
                catch (Exception ex)
                {
                }

            }
            if (btn1st.Pressed)
            {
                UnregisterEvents();
                GameVariables.Instance.FirstPlayer.Controler = Controlling.BUTTONS;
                GameVariables.Instance.FirstPlayer.Coach = TeamCoach.HUMAN;
                int startingPlayerIndex = WhosFirst();

                /*
                 * KOMUNIKACJA KTO ZACZYNA !!
                 * TODO
                 * Na razie zaczyna zawsze host !!
                 * 
                 */
                startingPlayerIndex = 3;

                switch (startingPlayerIndex)
                {
                    case 1:
                        GameVariables.Instance.NextMoveForFirstPlayer();
                        break;
                    case 2:
                        GameVariables.Instance.NextMoveForSecondPlayer();
                        break;
                    default:
                        GameVariables.Instance.NextMoveForFirstPlayer();
                        break;
                }
                //
                // TODO -> kolor dla drugiego gracza -> To bêdzie w lobby
                //
                GameVariables.Instance.SecondPlayer.Controler = Controlling.NOTSET;
                GameVariables.Instance.SecondPlayer.ShirtsColor = colSelector1st.GiveNextFreeColor();

                int gameLimit = 0;

                if (GameVariables.Instance.IsLimitedByGoals)
                {
                    gameLimit = goalsLimit.Number;
                    GameVariables.Instance.GoalsLimit = goalsLimit.Number;
                    GameVariables.Instance.TimeLeft = 0;
                    GameVariables.Instance.TotalTime = 0;
                }
                else
                {
                    GameVariables.Instance.GoalsLimit = 0;
                    GameVariables.Instance.TimeLeft = clock.TimeOfPlayInSeconds();
                    GameVariables.Instance.TotalTime = clock.TimeOfPlayInSeconds();
                    gameLimit = clock.TimeOfPlayInSeconds();
                }

                /*
                 * 
                 * Wchodzimy na ekran poczekali - lobby. W nim czekamy na drugiego gracza i po otrzymaniu
                 * komunikatu o po³¹czeniu to akceptujemy albo odrzucamy. I dopiero wtredy nastepuje w³aœciwa gra !
                 * 
                 */
                //GameState[] states = GameManager.GetStates();
                //foreach (GameState state in states)
                //    GameManager.RemoveState(state);
                this.ScreenState = global::TableGoal.ScreenState.Hidden;

                GameVariables.Instance.SecondPlayer.Coach = TeamCoach.REMOTEOPPONENT;

                Dictionary<String, Object> tableProperties = new Dictionary<String, Object>();                
                tableProperties.Add(KnownRoomProperties.FieldType.ToString(), GameVariables.Instance.TypeOfField);
                tableProperties.Add(KnownRoomProperties.GameType.ToString(), GameVariables.Instance.IsLimitedByGoals);
                tableProperties.Add(KnownRoomProperties.GameLimit.ToString(), gameLimit);
                tableProperties.Add(KnownRoomProperties.OpponentColor.ToString(), colSelector1st.SelectedColor.ToString());

                GameManager.AddState(new GlobalMultiLobbyState());
                GlobalMultiplayerContext.PlayerIsFirst = true;
                GlobalMultiplayerContext.warpClient.CreateRoom(playerName, playerName, 2, tableProperties);
            }
            shirt1st.Color = colSelector1st.SelectedColor;

            _size = _Font.MeasureString(playerName);
            playersNameBorder = new Rectangle(600 - (int)(_size.X / 2) - 25, 150 - (int)(_size.Y / 2), (int)_size.X + 25, (int)(_size.Y * 2.7f));

            menu.Update(gameTime);
        }

        private int WhosFirst()
        {
            int range = 10000;
            int result = coin.Next(0, range);
            if (result >= range / 2)
                result = 2;
            else
                result = 1;
            return result;
        }
    }
}
