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
    /// <summary>
    /// Stan gry gdzie wybiera siê flagi.
    /// </summary>
    public class WcFlagSelectionState : GameState
    {
        Menu menu;
        UIPicture[] flags;
        int xSpeed;
        int xDisplacement;
        Country selectedCountry;
        bool shouldBounceBack;

        public WcFlagSelectionState()
        {
            selectedCountry = Country.UNKNOWN;
            this.EnabledGestures = GestureType.FreeDrag | GestureType.Tap | GestureType.Flick;
            menu = new Menu("Backgrounds/Dimmed", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            flags = new UIPicture[Countries.pathToFlags.Count];
            int x = 0;
            int y = 0;
            int dx = 120;
            int dy = 75;
            int xMarigin = 30;
            int yMarigin = 35;
            for (int i = 0; i < flags.Length; i++)
            {
                x = xMarigin + i / 4 * (xMarigin + dx);
                y = yMarigin + (i % 4) * (yMarigin + dy);
                flags[i] = new UIPicture(Countries.pathToFlags.Values.ElementAt(i), new Rectangle(x, y, dx, dy));
            }
            xSpeed = 0;
            xDisplacement = 0;
            shouldBounceBack = false;
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            foreach (UIPicture flag in flags)
                flag.LoadTexture(GameManager.Game.Content);
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
            if (selectedCountry != Country.UNKNOWN)
            {
                GameVariables.Instance.SelectedCountry = selectedCountry;
                WorldCupProgress.Instance.SelectedCountry = selectedCountry;
                GameManager.AddState(new WcSelectedFlagState());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
                selectedCountry = Country.UNKNOWN;
                return;
            }
            Rectangle screen = new Rectangle(0,0,800,480);

            for (int flagID = 0; flagID < flags.Length; flagID++)
            {
                flags[flagID].DestinationRectangle = new Rectangle(flags[flagID].DestinationRectangle.X + xDisplacement + xSpeed,
                                                         flags[flagID].DestinationRectangle.Y,
                                                         flags[flagID].DestinationRectangle.Width,
                                                         flags[flagID].DestinationRectangle.Height);
                if (flags[flagID].DestinationRectangle.Intersects(screen))
                    flags[flagID].Visible = true;
                else
                    flags[flagID].Visible = false;
            }
            
            /*
             * Jeœli po³o¿enie pierwszej flagi (lewy, górny róg wyœwietlanych flag) jest wiêksze od 30 pixeli.
             */
            if (flags[0].DestinationRectangle.Left > 30)
            {
                shouldBounceBack = true;
                xSpeed = (int)(xSpeed * -0.2);
                if (xSpeed >= 0)
                    xSpeed = -28;
            }
                /*
                 * Jeœli po³o¿enie ostatniej flagi (prawy, dolny róg wyœwietlanych flag) jest mniejsze od 770).
                 */
            else if (flags[flags.Length - 1].DestinationRectangle.Right < 770)
            {
                shouldBounceBack = true;
                xSpeed = (int)(xSpeed * -0.3);
                if (xSpeed <= 0)
                    xSpeed = 28;
            }
                /*
                 * Jeœli ¿adne z poni¿szych to dzia³amy parametrem "tarcia" na prêdkoœæ.
                 */
            else
            {
                if (shouldBounceBack)
                {
                    if (Math.Abs(xSpeed) > 23)
                    {
                        xSpeed = (int)(xSpeed * 0.4);
                    }
                    else if (Math.Abs(xSpeed) > 15)
                    {
                        xSpeed = (int)(xSpeed * 0.4);
                    }
                    else if (Math.Abs(xSpeed) > 6)
                    {
                        xSpeed = (int)(xSpeed * 0.4);
                    }
                }
                xSpeed = (int)(xSpeed * 0.96);
                if (Math.Abs(xSpeed) < 2)
                {
                    xSpeed = 0;
                    shouldBounceBack = false;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            for (int i = 0; i < flags.Length; i++)
                flags[i].Draw(spriteBatch);
            base.Draw(gameTime);
            spriteBatch.End();
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            xDisplacement = 0;            
            if (input.Gestures.Count > 0)
            {
                shouldBounceBack = false;
                xSpeed = 0;
                if (input.Gestures[0].GestureType == GestureType.FreeDrag)
                {
                    xDisplacement = (int)input.Gestures[0].Delta.X;
                    /*
                     * ustalamy przesuniecie na 2 i pol raza delta x
                     */
                    xDisplacement = (int)(2.5 * xDisplacement);
                }
                if (input.Gestures[0].GestureType == GestureType.Flick)
                {
                    /*
                     * Bierzemy delte x z Flickniecia i dzielimy przez 20 - to daje nam predkosc
                     */
                    xSpeed = (int)input.Gestures[0].Delta.X / 20;
                    if (flags[0].DestinationRectangle.Left >= 30)
                    {
                        if (xSpeed > 0)
                            xSpeed = -1;
                    }
                    if (flags[flags.Length - 1].DestinationRectangle.Right <= 770)
                    {
                        if (xSpeed < 0)
                            xSpeed = 1;
                    }
                }
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    /*
                     * Jeœli tapniemy i jest jakas prêdkoœæ to zatrzymujemy.
                     */
                    if (xSpeed != 0)
                    {
                        xSpeed = 0;
                        return;
                    }
                    for (int i = 0; i < flags.Length; i++)
                    {
                        if (flags[i].WasPressed(input.Gestures[0].Position))
                        {
                            selectedCountry = Countries.GetCountryFromTextureName(flags[i].TextureName);
                        }
                    }
                }
            }
            base.HandleInput(gameTime, input);
        }
    }
}
