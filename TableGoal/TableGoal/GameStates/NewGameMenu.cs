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
    class NewGameMenu : GameState
    {
        Menu menu;
        CheckBox fieldPicker;
        CheckBox gameTypePicker;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;

        public NewGameMenu()
        {
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/WorldCupBtn", ButtonType.WorldCup);
            menu.AddButton("MenusElements/OnePlayerBtn", ButtonType.OnePlayerGame);
            menu.AddButton("MenusElements/TwoPlayersBtn", ButtonType.TwoPlayerGame);
            menu.AddButton("MenusElements/BackBtn", ButtonType.Back);
            fieldPicker = new CheckBox("MenusElements/FieldChosing");
            fieldPicker.DestinationRectangle = new Rectangle(590, 270, 160, 160);
            gameTypePicker = new CheckBox("MenusElements/GameTypeChosing");
            gameTypePicker.DestinationRectangle = new Rectangle(590, 50, 170, 155);
            gameTypePicker.Checked = true;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            fieldPicker.Draw(spriteBatch);
            gameTypePicker.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            fieldPicker.LoadTexture(GameManager.Game.Content);
            gameTypePicker.LoadTexture(GameManager.Game.Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
            if (menu.PressedButton == ButtonType.WorldCup)
            {
                GameVariables.Instance.FirstPlayer.Coach = TeamCoach.HUMAN;
                GameVariables.Instance.FirstPlayer.Controler = Controlling.BUTTONS;
                GameVariables.Instance.SecondPlayer.Coach = TeamCoach.CPU;
                GameVariables.Instance.SecondPlayer.Controler = Controlling.NOTSET;
                GameManager.AddState(new WorldCupState());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.OnePlayerGame)
            {
                SetFieldAndTypeOfGame();
                GameVariables.Instance.FirstPlayer.Coach = TeamCoach.HUMAN;
                GameVariables.Instance.FirstPlayer.Controler = Controlling.BUTTONS;
                GameVariables.Instance.SecondPlayer.Coach = TeamCoach.CPU;
                GameVariables.Instance.SecondPlayer.Controler = Controlling.NOTSET;
                GameManager.AddState(new SelectionState(false));
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.TwoPlayerGame)
            {
                SetFieldAndTypeOfGame();
                GameVariables.Instance.SecondPlayer.Coach = TeamCoach.HUMAN;
                GameManager.AddState(new MultiplayerState());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.Back)
            {
                GameManager.RemoveState(this);
            }
        }

        private void SetFieldAndTypeOfGame()
        {
            if (fieldPicker.Checked)
            {
                GameVariables.Instance.TypeOfField = PlayField.large;
            }
            else
            {
                GameVariables.Instance.TypeOfField = PlayField.classic;
            }

            if (gameTypePicker.Checked)
            {
                GameVariables.Instance.IsLimitedByGoals = true;
            }
            else
            {
                GameVariables.Instance.IsLimitedByGoals = false;
            }
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
                    fieldPicker.HandleInput(input.Gestures[0].Position);
                    gameTypePicker.HandleInput(input.Gestures[0].Position);
                    if (menu.PressedButton != ButtonType.None)
                    {
                        clickAnimationOngoing = true;
                        AudioManager.PlaySound("selected");
                    }
                }
            }
        }
    }
}
