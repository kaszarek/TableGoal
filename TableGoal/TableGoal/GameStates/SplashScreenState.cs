using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;

namespace TableGoal
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SplashScreenState : GameState
    {
        UIPicture background;
        float SCREEN_TIMEOUT = 3.0f;
        float actualTime = 0.0f;
        #region Extra text on splash screen
        Vector2 _screenSize;
        SpriteFont _Font;
        Vector2 _Size;
        string _Text = "";
        #endregion

        public SplashScreenState()
        {
            background = new UIPicture("Backgrounds/SplashScreenImage", new Rectangle(0, 0, 800, 480));
        }

        public override void Update(GameTime gameTime)
        {
            actualTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (actualTime >= SCREEN_TIMEOUT)
            {
                EndSplashScreen();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            background.Draw(spriteBatch);
            spriteBatch.DrawString(_Font,
                                   _Text,
                                   new Vector2(10, _screenSize.Y - _Size.Y * 1.2f),
                                   Color.Red);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        public override void LoadContent()
        {
            background.LoadTexture(GameManager.Game.Content);
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/TRIAL_font");
            _Size = _Font.MeasureString(_Text);
            _screenSize = new Vector2(GameManager.Game.GraphicsDevice.Viewport.Width,
                                      GameManager.Game.GraphicsDevice.Viewport.Height);
            base.LoadContent();
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (input.Gestures.Count > 0)
            {
                EndSplashScreen();
            }
        }

        private void EndSplashScreen()
        {
            GameManager.AddState(new MainMenuState());
            GameManager.RemoveState(this);
        }
    }
}
