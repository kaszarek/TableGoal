using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using System.IO.IsolatedStorage;
using System.IO;

namespace TableGoal
{
    class PresentStatisticsState : GameState
    {
        Menu menu;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        SpriteFont _Font;
        /// <summary>
        /// Counter for deviding by 3 to determine which screen should be shown.
        /// </summary>
        int tapCounter = 999;
        /// <summary>
        /// Modulo rest determines which screen to show.
        /// </summary>
        int screenModulo = 0;

        public PresentStatisticsState()
        {
            this.EnabledGestures = GestureType.Tap | GestureType.Flick;
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            //Statistics.Instance.CzasSpedzonyWmeczach = 7589341;
            //Statistics.Instance.CzasSpedzonyWmeczach = 589341;
            ////Statistics.Instance.CzasSpedzonyWmeczach = 89341;
            ////Statistics.Instance.CzasSpedzonyWmeczach = 9341;
            ////Statistics.Instance.CzasSpedzonyWmeczach = 341;
            //Statistics.Instance.IloscRozegranychMeczy = 1578;
            //Statistics.Instance.PoliczSredniCzasMeczu();
            //Statistics.Instance.Najwiecejodbic = 21;
            //Statistics.Instance.StraconeBramki = 314;
            //Statistics.Instance.ZdobyteBramki = 3493;
            //Statistics.Instance.WC_Attended = 182;
            //Statistics.Instance.WC_First = 112;
            //Statistics.Instance.WC_Second = 29;
            //Statistics.Instance.WC_Third = 9;
            //Statistics.Instance.EASYstats.RozegraneMecze = 300;
            //Statistics.Instance.EASYstats.WygraneMecze = 213;
            //Statistics.Instance.EASYstats.Remisy = 48;
            //Statistics.Instance.EASYstats.PrzegraneMecze = 35;
            //Statistics.Instance.EASYstats.Przerwane = 4;
            //Statistics.Instance.MEDIUMstats.RozegraneMecze = 783;
            //Statistics.Instance.MEDIUMstats.WygraneMecze = 517;
            //Statistics.Instance.MEDIUMstats.Remisy = 111;
            //Statistics.Instance.MEDIUMstats.PrzegraneMecze = 143;
            //Statistics.Instance.MEDIUMstats.Przerwane = 12;
            //Statistics.Instance.HARDstats.RozegraneMecze = 495;
            //Statistics.Instance.HARDstats.WygraneMecze = 189;
            //Statistics.Instance.HARDstats.Remisy = 219;
            //Statistics.Instance.HARDstats.PrzegraneMecze = 53;
            //Statistics.Instance.HARDstats.Przerwane = 34;
        }

        public override void LoadContent()
        {
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            menu.LoadTexture(GameManager.Game.Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            screenModulo = tapCounter % 3;
            switch (screenModulo)
            {
                case 0:
                    spriteBatch.DrawString(_Font, "1/3", new Vector2(5, 0), Color.Black);
                    spriteBatch.DrawString(_Font, "Matches' time", new Vector2(138, 15), Color.Black);
                    spriteBatch.DrawString(_Font, "Matches started", new Vector2(96, 60), Color.Black);
                    spriteBatch.DrawString(_Font, "Avg match duration", new Vector2(7, 105), Color.Black);
                    spriteBatch.DrawString(_Font, "Lost goals", new Vector2(219, 150), Color.Black);
                    spriteBatch.DrawString(_Font, "Scored goals", new Vector2(166, 195), Color.Black);
                    spriteBatch.DrawString(_Font, "The longest bounce", new Vector2(16, 240), Color.Black);
                    spriteBatch.DrawString(_Font, "World Cup started", new Vector2(42, 285), Color.Black);
                    spriteBatch.DrawString(_Font, "World Cup 1st", new Vector2(134, 330), Color.Black);
                    spriteBatch.DrawString(_Font, "World Cup 2nd", new Vector2(110, 375), Color.Black);
                    spriteBatch.DrawString(_Font, "World Cup 3rd", new Vector2(118, 420), Color.Black);

                    int spentSeconds = Statistics.Instance.CzasSpedzonyWmeczach;
                    int averageGame = Statistics.Instance.SredniCzasMeczu;
                    if (spentSeconds >= 86400)
                        spriteBatch.DrawString(_Font, String.Format("{0}d {1}h {2}m {0}s",
                                                      spentSeconds / 86400,
                                                      (spentSeconds % 86400) / 3600,
                                                      ((spentSeconds % 86400) % 3600) / 60,
                                                      (((spentSeconds % 86400) % 3600) % 60) % 60),
                                                      new Vector2(470, 15), Color.Black);
                    else if (spentSeconds >= 3600)
                        spriteBatch.DrawString(_Font, String.Format("{0}h {1}m {2}s",
                                                      spentSeconds / 3600, (spentSeconds % 3600) / 60, ((spentSeconds % 3600) % 60) % 60),
                                                      new Vector2(470, 15), Color.Black);
                    else if (spentSeconds >= 60)
                        spriteBatch.DrawString(_Font, String.Format("{0}m {1}s",
                                                      spentSeconds / 60, spentSeconds % 60),
                                                      new Vector2(470, 15), Color.Black);
                    else
                        spriteBatch.DrawString(_Font, String.Format("{0}s", spentSeconds), new Vector2(470, 15), Color.Black);

                    spriteBatch.DrawString(_Font, Statistics.Instance.IloscRozegranychMeczy.ToString(), new Vector2(470, 60), Color.Black);

                    if (averageGame >= 86400)
                        spriteBatch.DrawString(_Font, String.Format("{0}d {1}h {2}m {0}s",
                                                      averageGame / 86400,
                                                      (averageGame % 86400) / 3600,
                                                      ((averageGame % 86400) % 3600) / 60,
                                                      (((averageGame % 86400) % 3600) % 60) % 60),
                                                      new Vector2(470, 105), Color.Black);
                    else if (averageGame >= 3600)
                        spriteBatch.DrawString(_Font, String.Format("{0}h {1}m {2}s",
                                                      averageGame / 3600, (averageGame % 3600) / 60, ((averageGame % 3600) % 60) % 60),
                                                      new Vector2(470, 110), Color.Black);
                    else if (averageGame >= 60)
                        spriteBatch.DrawString(_Font, String.Format("{0}m {1}s",
                                                      averageGame / 60, averageGame % 60),
                                                      new Vector2(470, 105), Color.Black);
                    else
                        spriteBatch.DrawString(_Font, String.Format("{0}s", averageGame), new Vector2(470, 105), Color.Black);

                    spriteBatch.DrawString(_Font, Statistics.Instance.StraconeBramki.ToString(), new Vector2(470, 150), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.ZdobyteBramki.ToString(), new Vector2(470, 195), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.Najwiecejodbic.ToString(), new Vector2(470, 240), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.WC_Attended.ToString(), new Vector2(470, 285), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.WC_First.ToString(), new Vector2(470, 330), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.WC_Second.ToString(), new Vector2(470, 375), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.WC_Third.ToString(), new Vector2(470, 420), Color.Black);
                    break;
                case 1:
                    spriteBatch.DrawString(_Font, "2/3", new Vector2(5, 0), Color.Black);
                    spriteBatch.DrawString(_Font, "EASY", new Vector2(180, 100), Color.Black);
                    spriteBatch.DrawString(_Font, "MEDIUM", new Vector2(350, 100), Color.Black);
                    spriteBatch.DrawString(_Font, "HARD", new Vector2(600, 100), Color.Black);

                    spriteBatch.DrawString(_Font, "Played", new Vector2(10, 150), Color.Black);
                    spriteBatch.DrawString(_Font, "Won", new Vector2(10, 200), Color.Black);
                    spriteBatch.DrawString(_Font, "Draw", new Vector2(10, 250), Color.Black);
                    spriteBatch.DrawString(_Font, "Lost", new Vector2(10, 300), Color.Black);
                    spriteBatch.DrawString(_Font, "Unclear", new Vector2(10, 350), Color.Black);

                    spriteBatch.DrawString(_Font, Statistics.Instance.EASYstats.RozegraneMecze.ToString(), new Vector2(220, 150), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.EASYstats.WygraneMecze.ToString(), new Vector2(220, 200), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.EASYstats.Remisy.ToString(), new Vector2(220, 250), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.EASYstats.PrzegraneMecze.ToString(), new Vector2(220, 300), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.EASYstats.Przerwane.ToString(), new Vector2(220, 350), Color.Black);

                    spriteBatch.DrawString(_Font, Statistics.Instance.MEDIUMstats.RozegraneMecze.ToString(), new Vector2(450, 150), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.MEDIUMstats.WygraneMecze.ToString(), new Vector2(450, 200), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.MEDIUMstats.Remisy.ToString(), new Vector2(450, 250), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.MEDIUMstats.PrzegraneMecze.ToString(), new Vector2(450, 300), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.MEDIUMstats.Przerwane.ToString(), new Vector2(450, 350), Color.Black);

                    spriteBatch.DrawString(_Font, Statistics.Instance.HARDstats.RozegraneMecze.ToString(), new Vector2(650, 150), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.HARDstats.WygraneMecze.ToString(), new Vector2(650, 200), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.HARDstats.Remisy.ToString(), new Vector2(650, 250), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.HARDstats.PrzegraneMecze.ToString(), new Vector2(650, 300), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.HARDstats.Przerwane.ToString(), new Vector2(650, 350), Color.Black);
                    break;
                case 2:
                    spriteBatch.DrawString(_Font, "3/3", new Vector2(5, 0), Color.Black);
                    spriteBatch.DrawString(_Font, "Multiplayer games:", new Vector2(10, 100), Color.Black);
                    spriteBatch.DrawString(_Font, "Started", new Vector2(10, 150), Color.Black);
                    spriteBatch.DrawString(_Font, "Won", new Vector2(10, 200), Color.Black);
                    spriteBatch.DrawString(_Font, "Draw", new Vector2(10, 250), Color.Black);
                    spriteBatch.DrawString(_Font, "Lost", new Vector2(10, 300), Color.Black);
                    spriteBatch.DrawString(_Font, "Unclear", new Vector2(10, 350), Color.Black);

                    spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.RozegraneMecze.ToString(), new Vector2(310, 150), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.WygraneMecze.ToString(), new Vector2(310, 200), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.Remisy.ToString(), new Vector2(310, 250), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.PrzegraneMecze.ToString(), new Vector2(310, 300), Color.Black);
                    spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.Przerwane.ToString(), new Vector2(310, 350), Color.Black);
                    break;
                default:
                    break;
            }
            spriteBatch.End();
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
                    AudioManager.PlaySound("selected");
                    if (input.Gestures[0].Position.X >= 400)
                    {
                        tapCounter++;
                    }
                    else
                    {
                        tapCounter--;
                    }
                }
                if (input.Gestures[0].GestureType == GestureType.Flick)
                {
                    if (input.Gestures[0].Delta.X >= 1000)
                    {
                        tapCounter--;
                        AudioManager.PlaySound("selected");
                    }
                    else if (input.Gestures[0].Delta.X <= -1000)
                    {
                        tapCounter++;
                        AudioManager.PlaySound("selected");
                    }
                }
            }
        }
    }
}
