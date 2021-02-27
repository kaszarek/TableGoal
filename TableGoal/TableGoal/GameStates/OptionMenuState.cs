using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO.IsolatedStorage;
using System.IO;

namespace TableGoal
{
    class OptionMenuState : GameState
    {
        Menu menu;
        CheckBox sound;
        CheckBox music;
        OptionsWriterReader.Options opts;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;

        #region VERSION
        Vector2 _screenSize;
        SpriteFont _Font;
        Vector2 _Size;
        string _Text = "1.0.0.0";
        #endregion

        public OptionMenuState()
        {
            opts = new OptionsWriterReader.Options();
            opts.Music = true;
            opts.Sound = true;
            opts.DefaultStyle = true;
            this.LoadSetttingsFromIsolatedStorage();
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            _screenSize = new Vector2(800, 480);
            sound = new CheckBox("MenusElements/SoundCbx");
            sound.Checked = opts.Sound;
            menu.AddElement(sound);
            music = new CheckBox("MenusElements/MusicCbx");
            music.Checked = opts.Music;
            menu.AddElement(music);
            menu.AddButton("MenusElements/ControllsBtn", ButtonType.Controlls);
            menu.AddButton("MenusElements/BackBtn", ButtonType.Back);
            _Text = System.Reflection.Assembly.GetCallingAssembly().FullName.Split('=')[1].Split(',')[0];
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/TRIAL_font");
            _Size = _Font.MeasureString(_Text);
            _screenSize = new Vector2(GameManager.Game.GraphicsDevice.Viewport.Width,
                                      GameManager.Game.GraphicsDevice.Viewport.Height);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (menu.PressedButton == ButtonType.Back ||    
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                opts.Music = music.Checked;
                opts.Sound = sound.Checked;
                this.SaveSettingsToIsolatedStorage();
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
            if (menu.PressedButton == ButtonType.Controlls)
            {
                GameManager.AddState(new ControllsChangeState());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            spriteBatch.DrawString(_Font,
                                   _Text,
                                   new Vector2(_screenSize.X - _Size.X * 1.2f, _screenSize.Y - _Size.Y * 1.2f),
                                   Color.Black);
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
                    GameVariables.Instance.MusicOn = music.Checked;
                    GameVariables.Instance.SoundsOn = sound.Checked;
                    if (menu.PressedButton != ButtonType.None)
                    {
                        clickAnimationOngoing = true;
                        AudioManager.PlaySound("selected");
                    }
                }
            }
        }

        private void SaveSettingsToIsolatedStorage()
        {
            OptionsWriterReader.opts = opts;
            OptionsWriterReader.SaveSettingsToIsolatedStorage();
        }

        private void LoadSetttingsFromIsolatedStorage()
        {
            OptionsWriterReader.LoadSetttingsFromIsolatedStorage();
            opts = OptionsWriterReader.opts;
        }
    }
}
