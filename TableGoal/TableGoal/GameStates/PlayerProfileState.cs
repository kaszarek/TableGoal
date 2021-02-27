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
using System.Diagnostics;

namespace TableGoal
{
    class PlyerProfileState : GameState
    {
        Menu menu;
        bool clickAnimationOngoing = false;
        SpriteFont _Font;
        string _playersName = String.Empty;
        string _welcome;
        Vector2 _size;
        Rectangle _changeName;
        Texture2D _coachSprite;
        CoachPipTalkCodes[] pickedPipTalk;
        Coach realCoach;
        PipTalkBaloon[] pickedPipTalkInBaloons;
        UIPicture stats;
        int _counter = 0;

        public PlyerProfileState()
        {
            this.EnabledGestures = GestureType.Tap | GestureType.Flick | GestureType.FreeDrag;
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            stats = new UIPicture("MenusElements/stats", new Rectangle(663, 383, 120, 100));
            stats.Color = Color.Black;
            menu.AddElement(stats);
            _welcome = "Welcome {0}";
            PlayerWriterReader.LoadFromIsolatedStorage();
            PipTalkWriterReader.LoadFromIsolatedStorage();
            _playersName = PlayerWriterReader.plInfo.Name;
            pickedPipTalk = new CoachPipTalkCodes[4];
            pickedPipTalk[0] = PipTalkWriterReader.ptInfo.First;
            pickedPipTalk[1] = PipTalkWriterReader.ptInfo.Second;
            pickedPipTalk[2] = PipTalkWriterReader.ptInfo.Third;
            pickedPipTalk[3] = PipTalkWriterReader.ptInfo.Fourth;

            realCoach = new Coach(new Rectangle(142, 247, 50, 120), new Rectangle(0, 0, 50, 120), Color.Gold, true);
            realCoach.PipTalkPositionVect = new Vector2(realCoach.DestinationRectangle.X + 14, realCoach.DestinationRectangle.Y + 10);
            realCoach.PipTalkBaloonPositionVect = new Vector2(realCoach.DestinationRectangle.X + 6, realCoach.DestinationRectangle.Y + 10);
            realCoach.Active = true;
            _changeName = new Rectangle();
        }

        public override void LoadContent()
        {
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            _size = _Font.MeasureString(String.Format(_welcome, _playersName));
            _changeName = new Rectangle(400 - (int)(_size.X / 2), 50, (int)_size.X, (int)_size.Y);
            _coachSprite = GameManager.Game.Content.Load<Texture2D>("coach");
            menu.LoadTexture(GameManager.Game.Content);
            realCoach.LoadTexture(GameManager.Game.Content);
            realCoach.ShowCoachMessage(CoachPipTalkCodes.NoBaloon);
            PrecalculatePipTalks();
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
            if (stats.Pressed)
            {
                AudioManager.PlaySound("selected");
                stats.Pressed = false;
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                GameManager.AddState(new PresentStatisticsState());
            }
            realCoach.Update(gameTime);
        }

        private void PrecalculatePipTalks()
        {
            pickedPipTalkInBaloons = new PipTalkBaloon[4];
            for (int i = 0; i < 4; i++)
            {
                pickedPipTalkInBaloons[i] = new PipTalkBaloon(new Vector2(400 - realCoach.pipTalkLenghts[pickedPipTalk[i]].X / 2, 150 + i * 70), Color.Black, realCoach.pipTalkStrings[pickedPipTalk[i]], pickedPipTalk[i]);//, 55);
                pickedPipTalkInBaloons[i].LoadTexture(GameManager.Game.Content);
            }
        }

        private void AdjustChangedPipTalk(int index, CoachPipTalkCodes newCode)
        {
            pickedPipTalkInBaloons[index] = new PipTalkBaloon(new Vector2(400 - realCoach.pipTalkLenghts[newCode].X / 2, 150 + index * 70), Color.Black, realCoach.pipTalkStrings[pickedPipTalk[index]], pickedPipTalk[index]);
            pickedPipTalkInBaloons[index].LoadTexture(GameManager.Game.Content);
        }

        private void DrawPipTalkBaloons(SpriteBatch sb)
        {
            for (int i = 0; i < 4; i++)
            {
                pickedPipTalkInBaloons[i].Draw(sb);
            }
        }

        private void UpdatePipTalk(int index, CoachPipTalkCodes newCode)
        {
            pickedPipTalk[index] = newCode;
            switch (index)
            {
                case 0:
                    PipTalkWriterReader.ptInfo.First = newCode;
                    break;
                case 1:
                    PipTalkWriterReader.ptInfo.Second = newCode;
                    break;
                case 2:
                    PipTalkWriterReader.ptInfo.Third = newCode;
                    break;
                case 3:
                    PipTalkWriterReader.ptInfo.Fourth = newCode;
                    break;
                default:
                    break;
            }
            AdjustChangedPipTalk(index, newCode);
            PipTalkWriterReader.SaveToIsolatedStorage();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            spriteBatch.DrawString(_Font, String.Format(_welcome, _playersName), new Vector2(400 - _size.X / 2, 45), Color.Black);
            realCoach.Draw(spriteBatch);
            DrawPipTalkBaloons(spriteBatch);
            spriteBatch.End();
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (clickAnimationOngoing)
                return;
            if (input.Gestures.Count > 0)
            {
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    if (realCoach.WasPressed(input.Gestures[0].Position))
                    {
                        if (_counter % 2 == 0)
                        {
                            if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "pl")
                            {
                                realCoach.ShowStringMessage("Pogadamy?");
                            }
                            else
                            {
                                realCoach.ShowStringMessage("Wanna talk?");
                            }
                        }
                        else
                        {
                            realCoach.CleanMessages();
                        }
                        _counter++;
                    }
                    menu.WasPressed(input.Gestures[0].Position);
                    for (int i = 0; i < pickedPipTalkInBaloons.Length; i++)
                    {
                        if (pickedPipTalkInBaloons[i].WasPressed(input.Gestures[0].Position))
                        {
#if DEBUG
                            Debug.WriteLine(String.Format("Tapped {0}", pickedPipTalkInBaloons[i].PipTalkCode));
#endif
                            GameManager.AddState(new PipTalkSelectionState(pickedPipTalk, realCoach, i, UpdatePipTalk));
                            AudioManager.PlaySound("selected");
                            this.ScreenState = global::TableGoal.ScreenState.Hidden;
                            return;
                        }
                    }
                    if (_changeName.Contains(new Point((int)input.Gestures[0].Position.X, (int)input.Gestures[0].Position.Y)))
                    {
                        if (!Guide.IsVisible)
                        {
                            Guide.BeginShowKeyboardInput(PlayerIndex.One, "Select your new name", "maximum length is 15 characters.", _playersName, ShowKeybordCallback, _playersName);
                        }
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
    }
}

