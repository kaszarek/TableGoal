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
using Microsoft.Xna.Framework.GamerServices;

namespace TableGoal
{
    class WifiHostSelectionState : GameState
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
        string playersName = String.Empty;
        Rectangle playersNameBorder;
        Vector2 _size;

        /// <summary>
        /// Constructor
        /// </summary>
        public WifiHostSelectionState()
        {
            colSelector1st = new ColorSelector(new Rectangle(20, 330, 0, 0), ColorSelOrientation.HORIZONTAL);

            menu = new Menu("Backgrounds/Background", new Rectangle(200, 50, 400, 380));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);

            shirt1st = new UIShirt("Shirt", new Rectangle(125, 60, 150, 150), .75f);
            shirt1st.Color = colSelector1st.SelectedColor;
            menu.AddElement(shirt1st);
            GameVariables.Instance.FirstPlayer.ShirtsColor = shirt1st.Color;

            UIPicture host = new UIPicture("HostLbl", new Rectangle(125, 10, 150, 75));
            menu.AddElement(host);

            btn1st = new UIButton("StartReadyBtn", new Rectangle(80, 200, 240, 120));
            menu.AddElement(btn1st);
                        
            
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
            playersName = PlayerWriterReader.plInfo.Name;
            playersNameBorder = new Rectangle(450, 220, 200, 30);
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
                                   playersName,
                                   new Vector2(600 - _size.X/2, 200),
                                   Color.Black);
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            colSelector1st.LoadTexture(GameManager.Game.Content);
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            _size = _Font.MeasureString(playersName);
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
                        Guide.BeginShowKeyboardInput(PlayerIndex.One, "Select your new name", "maximum length is 15 characters.", playersName, ShowKeybordCallback, playersName);
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
                    playersName = res.Substring(0, 15);
                else if (res.Length > 0)
                    playersName = res;

                if (playersName != PlayerWriterReader.plInfo.Name)
                {
                    PlayerWriterReader.plInfo.Name = playersName;
                    PlayerWriterReader.SaveToIsolatedStorage();
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                GameManager.RemoveState(this);
                AudioManager.PlaySound("selected");
            }
            if (btn1st.Pressed)
            {
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

                

                if (GameVariables.Instance.IsLimitedByGoals)
                {
                    GameVariables.Instance.GoalsLimit = goalsLimit.Number;
                    GameVariables.Instance.TimeLeft = 0;
                    GameVariables.Instance.TotalTime = 0;
                }
                else
                {
                    GameVariables.Instance.GoalsLimit = 0;
                    GameVariables.Instance.TimeLeft = clock.TimeOfPlayInSeconds();
                    GameVariables.Instance.TotalTime = clock.TimeOfPlayInSeconds();
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

                TableGoal.Players.Add(new PlayerInfo(playersName, colSelector1st.SelectedColor));
                GameVariables.Instance.SecondPlayer.Coach = TeamCoach.REMOTEOPPONENT;
                GameManager.AddState(new WifiLobbyState(playersName));
            }
            shirt1st.Color = colSelector1st.SelectedColor;

            _size = _Font.MeasureString(playersName);
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
