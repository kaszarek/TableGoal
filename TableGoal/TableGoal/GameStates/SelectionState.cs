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
    class SelectionState : GameState
    {
        Menu menu;
        ColorSelector colSelector1st;
        ColorSelector colSelector2nd;
        UIShirt shirt1st;
        UIShirt shirt2nd;
        UIClock clock;
        UIBall ball;
        UIPicture selectedDiffLevel;
        UIPicture easy;
        UIPicture medium;
        UIPicture hard;
        bool b_number_of;
        UIPicture number_of;
        bool b_units;
        bool b_ball;
        UIPicture units;
        UIButton btn1st;
        UIButton btn2nd;
        Digit goalsLimit;
        bool twoPlayersGame;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="twoPlayers">Specifies is it two player game.</param>
        public SelectionState(bool twoPlayers)
        {
            this.twoPlayersGame = twoPlayers;
            colSelector1st = new ColorSelector(new Rectangle(20,330,0,0), ColorSelOrientation.HORIZONTAL);
            
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 50, 400, 380));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            
            shirt1st = new UIShirt("Shirt", new Rectangle(125, 60, 150, 150), .75f);
            shirt1st.Color = colSelector1st.SelectedColor;
            menu.AddElement(shirt1st);
            GameVariables.Instance.FirstPlayer.ShirtsColor = shirt1st.Color;
            
            UIPicture host = new UIPicture("HostLbl", new Rectangle(125, 10, 150, 75));
            menu.AddElement(host);

            //btn1st = new UIButton("StartReadyBtn", new Rectangle(200, 250, 150, 75));
            btn1st = new UIButton("StartReadyBtn", new Rectangle(80, 200, 240, 120));
            menu.AddElement(btn1st);

            // for the second player if needed
            // creates these two object beforehand
            btn2nd = new UIButton("StartReadyBtn", new Rectangle(480, 200, 240, 120));
            // set buttont to true in case this in one plaer game
            btn2nd.Pressed = true;

            if (!twoPlayers)
            {
                easy = new UIPicture("MenusElements/DiffEasy", new Rectangle(500, 25, 200, 120));
                menu.AddElement(easy);
                medium = new UIPicture("MenusElements/DiffMedium", new Rectangle(500, 175, 200, 120));
                medium.Visible = false;
                hard = new UIPicture("MenusElements/DiffHard", new Rectangle(500, 325, 200, 120));
                hard.Visible = false;
                medium.Visible = true;
                menu.AddElement(medium);
                hard.Visible = true;
                menu.AddElement(hard);
                selectedDiffLevel = new UIPicture("MenusElements/DiffSelected", easy.DestinationRectangle);
                selectedDiffLevel.Color = shirt1st.Color;
                menu.AddElement(selectedDiffLevel);
            }
            else
            {
                // x coordinate based on the 800pixels width of the screen minus (from the ColorSelection constructor)
                // 2 * (bok + horizontalMarigin)
                colSelector2nd = new ColorSelector(new Rectangle(420, 330, 0, 0), ColorSelOrientation.HORIZONTAL);

                shirt2nd = new UIShirt("Shirt", new Rectangle(525, 60, 150, 150), .75f);
                shirt2nd.Color = colSelector2nd.SelectedColor;
                menu.AddElement(shirt2nd);
                GameVariables.Instance.SecondPlayer.ShirtsColor = shirt2nd.Color;

                UIPicture guest = new UIPicture("GuestLbl", new Rectangle(525, 10, 150, 75));
                menu.AddElement(guest);

                btn2nd.Pressed = false;
                menu.AddElement(btn2nd);
            }

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
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            colSelector1st.Draw(spriteBatch);
            if (twoPlayersGame)
                colSelector2nd.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            colSelector1st.LoadTexture(GameManager.Game.Content);
            if (twoPlayersGame)
                colSelector2nd.LoadTexture(GameManager.Game.Content);
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
                    // jeœli dla dwóch graczy to wtedy input jest przekierowywany na kontrolki drugiego gracza
                    if (twoPlayersGame)
                    {
                        if (!btn2nd.Pressed)
                        {
                            colSelector2nd.HandleInput(input.Gestures[0].Position);
                            GameVariables.Instance.SecondPlayer.ShirtsColor = colSelector2nd.SelectedColor;
                        }
                        else
                        {
                            shirt2nd.Pressed = true;
                        }
                        if (shirt2nd.Pressed)
                            btn2nd.Pressed = true;
                    }
                    else // je¿eli gra dla jednego gracza, to wtedy input maj¹ poziomy trudnoœci
                    {
                        selectedDiffLevel.Color = colSelector1st.SelectedColor;
                        if (easy.WasPressed(input.Gestures[0].Position))
                        {
                            selectedDiffLevel.DestinationRectangle = easy.DestinationRectangle;
                            SetDifficultyLevel(DifficultyLevel.EASY);
                            return;
                        }
                        if (medium.WasPressed(input.Gestures[0].Position))
                        {
                            selectedDiffLevel.DestinationRectangle = medium.DestinationRectangle;
                            SetDifficultyLevel(DifficultyLevel.MEDIUM);
                            return;
                        }
                        if (hard.WasPressed(input.Gestures[0].Position))
                        {
                            selectedDiffLevel.DestinationRectangle = hard.DestinationRectangle;
                            SetDifficultyLevel(DifficultyLevel.HARD);
                            return;
                        }
                    }
                    
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

                }
            }
        }

        private void SetDifficultyLevel(DifficultyLevel lvl)
        {
            GameVariables.Instance.DiffLevel = lvl;
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                // jeœli dwa dwóch graczy i potwierdzi³ któryœ z graczy to cofiemy operacje potwierdzania
                if (twoPlayersGame && (btn1st.Pressed || btn2nd.Pressed))
                {
                    btn1st.Pressed = false;
                    shirt1st.Pressed = false;
                    btn2nd.Pressed = false;
                    shirt2nd.Pressed = false;
                }
                else // w przeciwnym razie wracamy
                    GameManager.RemoveState(this);
                AudioManager.PlaySound("selected");
            }
            if (btn1st.Pressed && btn2nd.Pressed)
            {
                GameVariables.Instance.FirstPlayer.Coach = TeamCoach.HUMAN;
                GameVariables.Instance.FirstPlayer.Controler = Controlling.BUTTONS;
                GameVariables.Instance.SecondPlayer.Coach = TeamCoach.HUMAN;
                GameVariables.Instance.SecondPlayer.Controler = Controlling.BUTTONS;
                if (!twoPlayersGame)
                {
                    GameVariables.Instance.SecondPlayer.Coach = TeamCoach.CPU;
                    GameVariables.Instance.SecondPlayer.Controler = Controlling.NOTSET;
                    GameVariables.Instance.SecondPlayer.ShirtsColor = colSelector1st.GiveNextFreeColor();
                    // ustal kto zaczyna w grze dla pojedynczego gracza !!
                    switch (GameVariables.Instance.DiffLevel)
                    {
                        case DifficultyLevel.EASY:
                        case DifficultyLevel.MEDIUM:
                            GameVariables.Instance.NextMoveForFirstPlayer();
                            break;
                        case DifficultyLevel.HARD:
                            GameVariables.Instance.NextMoveForSecondPlayer();
                            break;
                    }
                }
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
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
                Statistics.Instance.ZaczynamKolejnyMecz();
                GameManager.AddState(new GameplayState(false));
            }
            shirt1st.Color = colSelector1st.SelectedColor;
            // these are only restrictions for two player game
            if (twoPlayersGame)
            {
                colSelector1st.ForbiddenColor = colSelector2nd.SelectedColor;
                colSelector2nd.ForbiddenColor = colSelector1st.SelectedColor;
                shirt2nd.Color = colSelector2nd.SelectedColor;
            }
            menu.Update(gameTime);
        }
    }
}
