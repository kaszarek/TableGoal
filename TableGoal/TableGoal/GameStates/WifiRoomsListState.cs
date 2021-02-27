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
    class WifiRoomsListState : GameState
    {
        Menu menu;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        SpriteFont _Font;
        string playersName = String.Empty;
        UIScrollableList list;
        int _tableMarigin = 30;
        UIJumpingUIPicture startMatch;
        MultiplayerTips _multiTips;


        public WifiRoomsListState()
        {
            //this.EnabledGestures = GestureType.Tap | GestureType.VerticalDrag;
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 350, 400, 108));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            PlayerWriterReader.LoadFromIsolatedStorage();
            playersName = PlayerWriterReader.plInfo.Name;
            list = new UIScrollableList(new Rectangle(_tableMarigin, _tableMarigin, 800 - 2 * _tableMarigin, 480 - 2 * _tableMarigin), playersName);
            menu.AddElement(list);
            /*
             * Tutaj wspó³rzêdne s¹ nie istotne w Prostok¹cie poniewa¿ gwizdek pojawi siê dopiero po klikniêciu
             * otwartej gry i jego wspó³rzêdne bêd¹ skorelowane z po³o¿eniem tej gry.
             * Istotne natomiast s¹ wymiary obrazka.
             */
            startMatch = new UIJumpingUIPicture("whistle", new Rectangle(710, 410, 90, 90), 0.3f, 0.4f, 0.5f, .8f);
            startMatch.Visible = false;
            _multiTips = new MultiplayerTips(new Vector2(400, 341));
            menu.AddElement(_multiTips);
            TableGoal.Players.Add(new PlayerInfo(playersName));
            TableGoal.GamePlay.Join(playersName, false);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            startMatch.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
            startMatch.LoadTexture(GameManager.Game.Content);
        }

        public override void Update(GameTime gameTime)
        {
            menu.Update(gameTime);
            if (list.SelectedIndex == -1)
            {
                startMatch.Visible = false;
            }
            startMatch.Update(gameTime);
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                ShowMainMenu();
            }
            if (startMatch.Pressed)
            {
                SetGameDetails();
                GameManager.AddState(new WifiJoinSelectionState(list.SelectedPlayer().PlayerName));
                startMatch.Pressed = false;
            }
        }

        private void SetGameDetails()
        {
            PlayerInfo pi = list.SelectedPlayer();
            if (pi != null)
            {
                GameVariables.Instance.TypeOfField = pi.Field;
                GameVariables.Instance.IsLimitedByGoals = pi.IsGoalLimited;
                if (pi.IsGoalLimited)
                {
                    GameVariables.Instance.GoalsLimit = pi.GameLimit;
                }
                else
                {
                    GameVariables.Instance.TimeLeft = pi.GameLimit;
                    GameVariables.Instance.TotalTime = pi.GameLimit;
                }
                GameVariables.Instance.FirstPlayer.ShirtsColor = pi.PlayerColor;
                GameVariables.Instance.FirstPlayer.Coach = TeamCoach.REMOTEOPPONENT;
            }
        }

        private void ShowMainMenu()
        {
            if (TableGoal.GamePlay != null)
            {
                TableGoal.GamePlay.Leave(false);
            }
            TableGoal.Players.Clear();
            GameState[] states = GameManager.GetStates();
            foreach (GameState state in states)
                if (!(state is MainMenuState) &&
                    !(state is NewGameMenu) &&
                    !(state is MultiplayerState))
                    GameManager.RemoveState(state);
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
                    if (menu.PressedButton != ButtonType.None)
                    {
                        clickAnimationOngoing = true;
                        AudioManager.PlaySound("selected");
                    }
                    if (startMatch.WasPressed(input.Gestures[0].Position))
                    {
                        startMatch.Pressed = true;
                    }
                    if (list.SelectedIndex >= 0)
                    {
                        Rectangle r = list.SelectedArea();
                        if (r != Rectangle.Empty)
                        {
                            startMatch.DestinationRectangle = new Rectangle(r.Right - 105, r.Top - 0, 90, 90);
                            startMatch.Position = new Vector2(startMatch.DestinationRectangle.X - startMatch.Offset,
                                                              startMatch.DestinationRectangle.Y - startMatch.Offset) + startMatch.Origin;
                            startMatch.Visible = true;
                        }
                    }
                    else
                    {
                        startMatch.Visible = false;
                    }
                }
            }
        }
    }
}
