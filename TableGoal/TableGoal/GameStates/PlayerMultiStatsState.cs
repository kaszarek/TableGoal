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
    class PlayerMultiStatsState : GameState
    {
        Menu menu;
        SpriteFont _Font;
        SpriteFont _SmallFont;
        int tapCounter = 0;
        Rectangle _tapEraseRect;
        string _playersName = String.Empty;
        string _welcome;
        Vector2 _size;
        Rectangle _changeName;

        public PlayerMultiStatsState()
        {
            this.EnabledGestures = GestureType.Tap;
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            _tapEraseRect = new Rectangle(0, 455, 23, 23);
            //Statistics.Instance.MULTIPLAYERstats.RozegraneMecze = 27;
            //Statistics.Instance.MULTIPLAYERstats.WygraneMecze = 14;
            //Statistics.Instance.MULTIPLAYERstats.Remisy = 4;
            //Statistics.Instance.MULTIPLAYERstats.PrzegraneMecze = 3;
            //Statistics.Instance.MULTIPLAYERstats.Przerwane = 6;

            _welcome = "Welcome {0}";
            PlayerWriterReader.LoadFromIsolatedStorage();
            _playersName = PlayerWriterReader.plInfo.Name;
            _changeName = new Rectangle();
        }

        public override void LoadContent()
        {
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            _SmallFont = GameManager.Game.Content.Load<SpriteFont>("Fonts/Sketch15");
            _size = _Font.MeasureString(String.Format(_welcome, _playersName));
            _changeName = new Rectangle(400 - (int)(_size.X / 2), 50, (int)_size.X, (int)_size.Y);
            menu.LoadTexture(GameManager.Game.Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
            if (tapCounter >= 5)
            {
                tapCounter = 0;
                if (!Guide.IsVisible)
                    Guide.BeginShowMessageBox("How did you find that O_o ?!",
                                              "You are about to clear your multiplayer statistics. Do you really want to do this?",
                                              new string[] { "Yes", "No" },
                                              1,
                                              MessageBoxIcon.Warning,
                                              new AsyncCallback(OnMessageBoxClosed),
                                              null);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);

            spriteBatch.DrawString(_SmallFont, "This is the hidden menu. Do not tell anybody how to get here.", new Vector2(75, 5), Color.Red);
            spriteBatch.DrawString(_Font, String.Format(_welcome, _playersName), new Vector2(400 - _size.X /2, 50), Color.Black);
            
            spriteBatch.DrawString(_Font, "Multiplayer games:", new Vector2(20, 150), Color.Black);
            spriteBatch.DrawString(_Font, "Started", new Vector2(50, 210), Color.Black);
            spriteBatch.DrawString(_Font, "Won", new Vector2(50, 260), Color.Black);
            spriteBatch.DrawString(_Font, "Draw", new Vector2(50, 310), Color.Black);
            spriteBatch.DrawString(_Font, "Lost", new Vector2(50, 360), Color.Black);
            spriteBatch.DrawString(_Font, "Unclear", new Vector2(50, 410), Color.Black);

            spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.RozegraneMecze.ToString(), new Vector2(360, 210), Color.Black);
            spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.WygraneMecze.ToString(), new Vector2(360, 260), Color.Black);
            spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.Remisy.ToString(), new Vector2(360, 310), Color.Black);
            spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.PrzegraneMecze.ToString(), new Vector2(360, 360), Color.Black);
            spriteBatch.DrawString(_Font, Statistics.Instance.MULTIPLAYERstats.Przerwane.ToString(), new Vector2(360, 410), Color.Black);

            spriteBatch.End();
        }
        
        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (input.Gestures.Count > 0)
            {
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    menu.WasPressed(input.Gestures[0].Position);
                    if (_tapEraseRect.Contains(new Point((int)input.Gestures[0].Position.X, (int)input.Gestures[0].Position.Y)))
                        tapCounter++;
                    if (_changeName.Contains(new Point((int)input.Gestures[0].Position.X, (int)input.Gestures[0].Position.Y)))
                        if (!Guide.IsVisible)
                            Guide.BeginShowKeyboardInput(PlayerIndex.One, "Select your new name", "maximum length is 15 characters.", _playersName, ShowKeybordCallback, _playersName);
                    AudioManager.PlaySound("selected");
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
                bool isAlphaNumeric = System.Text.RegularExpressions.Regex.IsMatch(res.Replace(' ', 'a').Replace('.', 'a'), "^[a-zA-Z0-9_]*$");

                if (!isAlphaNumeric)
                    return;

                if (res.Length > 15)
                    _playersName = res.Substring(0, 15);
                else if (res.Length > 0)
                    _playersName = res;

                if (_playersName != PlayerWriterReader.plInfo.Name)
                {
                    PlayerWriterReader.plInfo.Name = _playersName;
                    PlayerWriterReader.SaveToIsolatedStorage();

                    _size = _Font.MeasureString(String.Format(_welcome, _playersName));
                    _changeName = new Rectangle(400 - (int)(_size.X / 2), 50, (int)_size.X, (int)_size.Y);
                }
            }
        }
                
        /// <summary>
        /// Callback function for MessageBox
        /// </summary>
        /// <param name="ar">Encapsulated result.</param>
        private void OnMessageBoxClosed(IAsyncResult ar)
        {
            tapCounter = 0;
            int? buttonIndex = Guide.EndShowMessageBox(ar);
            switch (buttonIndex)
            {
                    /*
                     * YES -> czyœæ multi statsy
                     */
                case 0:
                    Statistics.Instance.MultiClear();
                    break;
                    /*
                     * NIE czyœæ nic :)
                     */
                case 1:
                    break;
                default:
                    break;
            }
        }

    }
}
