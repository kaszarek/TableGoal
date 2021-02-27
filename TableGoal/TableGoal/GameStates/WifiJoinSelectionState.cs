using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;

namespace TableGoal
{
    class WifiJoinSelectionState : GameState
    {
        Menu menu;
        ColorSelector colSelector2nd;
        UIPicture field;
        UIShirt shirt1st;
        UIShirt shirt2nd;
        UIButton btn2nd;
        string opponentName = String.Empty;
        SpriteFont _Font;

        /// <summary>
        /// Constructor
        /// </summary>
        public WifiJoinSelectionState(string opponent)
        {
            if (opponent != String.Empty)
                opponentName = opponent;
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 50, 400, 380));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);

            shirt1st = new UIShirt("Shirt", new Rectangle(125, 60, 150, 150), .75f);
            shirt1st.Pressed = true;
            shirt1st.Color = GameVariables.Instance.FirstPlayer.ShirtsColor;
            menu.AddElement(shirt1st);

            UIPicture host = new UIPicture("HostLbl", new Rectangle(125, 10, 150, 75));
            menu.AddElement(host);


            btn2nd = new UIButton("StartReadyBtn", new Rectangle(480, 200, 240, 120));
            btn2nd.Pressed = false;
            menu.AddElement(btn2nd);

            // x coordinate based on the 800pixels width of the screen minus (from the ColorSelection constructor)
            // 2 * (bok + horizontalMarigin)
            colSelector2nd = new ColorSelector(new Rectangle(420, 330, 0, 0), ColorSelOrientation.HORIZONTAL);
            colSelector2nd.ForbiddenColor = shirt1st.Color;

            shirt2nd = new UIShirt("Shirt", new Rectangle(525, 60, 150, 150), .75f);
            shirt2nd.Color = colSelector2nd.SelectedColor;
            menu.AddElement(shirt2nd);
            GameVariables.Instance.SecondPlayer.ShirtsColor = shirt2nd.Color;

            UIPicture guest = new UIPicture("GuestLbl", new Rectangle(525, 10, 150, 75));
            menu.AddElement(guest);

            UIPicture line = new UIPicture("empty4x4", new Rectangle(399, 0, 2, 480));
            line.Color = Color.Black;
            menu.AddElement(line);


            if (GameVariables.Instance.IsLimitedByGoals)
            {
                UIBall ball = new UIBall("ball", new Rectangle(90, 230, 80, 80), 1.1f);
                ball.Clickable = false;
                menu.AddElement(ball);
            }
            else
            {
                UIClock clock = new UIClock("clock", new Rectangle(90, 230, 80, 80), .6f);
                clock.Clickable = false;
                menu.AddElement(clock);
            }

            if (GameVariables.Instance.TypeOfField == PlayField.classic)
            {
                field = new UIPicture("MenusElements/FieldChosing", new Rectangle(100, 330, 200, 105));
                field.SourceRectangle = new Rectangle(0, 0, 200, 105);
            }
            else
            {
                field = new UIPicture("MenusElements/FieldChosing", new Rectangle(100, 330, 200, 105));
                field.SourceRectangle = new Rectangle(200, 0, 200, 105);
            }
            menu.AddElement(field);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            colSelector2nd.Draw(spriteBatch);
            if (GameVariables.Instance.IsLimitedByGoals)
            {
                spriteBatch.DrawString(_Font, String.Format("{0} goal(s)", GameVariables.Instance.Limitation()), new Vector2(160, 245), Color.Black);
            }
            else
            {
                spriteBatch.DrawString(_Font, String.Format("{0} min", GameVariables.Instance.Limitation() / 60), new Vector2(160, 245), Color.Black);
            }
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            colSelector2nd.LoadTexture(GameManager.Game.Content);
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/TRIAL_font");
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (input.Gestures.Count > 0)
            {
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    menu.WasPressed(input.Gestures[0].Position);

                    // gdy przyscisk start NIE jest klikniêty to wtedy zbieramy input z colorSelectora
                    if (!btn2nd.Pressed)
                    {
                        colSelector2nd.HandleInput(input.Gestures[0].Position);
                        GameVariables.Instance.SecondPlayer.ShirtsColor = colSelector2nd.SelectedColor;
                    }
                    else  // jeœli u¿ytwkonik kliknie w start, a nie w koszulkê, to musi to zadzia³aæ tak jakby klikn¹³ w koszulkê
                    {
                        shirt2nd.Pressed = true;
                    }
                    // i tu podobnie. Jak kliknie w koszulkê, to wtedy start te¿ jest wciœniêty
                    if (shirt2nd.Pressed)
                        btn2nd.Pressed = true;
                }
            }
        }

        private void SetDifficultyLevel(DifficultyLevel lvl)
        {
            GameVariables.Instance.DiffLevel = lvl;
        }

        private void ToPreviousMenu()
        {
            TableGoal.Players.Clear();
            GameState[] states = GameManager.GetStates();
            foreach (GameState state in states)
                if (!(state is MainMenuState) &&
                    !(state is NewGameMenu) &&
                    !(state is MultiplayerState))
                    GameManager.RemoveState(state);
            AudioManager.PlaySound("selected");
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                TableGoal.GamePlay.Leave(false);
                ToPreviousMenu();
            }
            if (btn2nd.Pressed)
            {
                GameVariables.Instance.FirstPlayer.Controler = Controlling.NOTSET;
                GameVariables.Instance.SecondPlayer.Controler = Controlling.BUTTONS;
                /*
                 * 
                 * TODO -> Kto zaczyna
                 * 
                 */
                GameVariables.Instance.NextMoveForFirstPlayer();


                //GameState[] states = GameManager.GetStates();
                //foreach (GameState state in states)
                //    GameManager.RemoveState(state);
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                
                /*
                 * 
                 * Jako Join po zaakceptowaniu gry wchodzimy do lobby. Po wysy³aniu komunikatu do Hosta i otrzymaniu 
                 * zgody na mecz zaczynamy grê. Jak mecz zostanie odwo³any przez Hosta to wyœwietlamy komunikat
                 * i wracamy do Main menu.
                 * 
                 */

                /*
                 * Wysy³amy dwa razy -> to przez to ¿e to jest UDP i czasami mo¿e nie dojœæ.
                 * A drugi raz nie zaszkodzi na pewno.
                 */
                TableGoal.GamePlay.SendColorDetails(opponentName);
                TableGoal.GamePlay.SendColorDetails(opponentName);
                GameManager.AddState(new WifiLobbyState(null));
                TableGoal.GamePlay.Challenge(opponentName);
            }
            shirt2nd.Color = colSelector2nd.SelectedColor;
            menu.Update(gameTime);
        }
    }
}
