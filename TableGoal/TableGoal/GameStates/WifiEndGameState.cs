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
    public class WifiEndGameState : GameState
    {
        Menu menu;
        UIPicture result;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        Digit firstPlPoints;
        Digit secondPlPoints;

        public WifiEndGameState()
        {
            this.EnabledGestures = GestureType.Tap;
            string resultPctPath = string.Empty;
            EndedGame gameEnd = EndedGame.Draw;
            if (!GameVariables.Instance.IsLimitedByGoals)
            {
                Statistics.Instance.CzasMeczu(GameVariables.Instance.TotalTime);
                if (GameVariables.Instance.FirstPlayer.Goals >
                    GameVariables.Instance.SecondPlayer.Goals)
                    if (TableGoal.GamePlay.IsHost)
                        gameEnd = EndedGame.PlayerWon;
                    else
                        gameEnd = EndedGame.PlayerLost;
                else if (GameVariables.Instance.FirstPlayer.Goals <
                         GameVariables.Instance.SecondPlayer.Goals)
                    if (TableGoal.GamePlay.IsHost)
                        gameEnd = EndedGame.PlayerLost;
                    else
                        gameEnd = EndedGame.PlayerWon;
            }
            else
            {
                Statistics.Instance.CzasMeczu(GameVariables.Instance.TimeLeft);
                if (GameVariables.Instance.FirstPlayer.Goals == GameVariables.Instance.GoalsLimit)
                    if (TableGoal.GamePlay.IsHost)
                        gameEnd = EndedGame.PlayerWon;
                    else
                        gameEnd = EndedGame.PlayerLost;

                if (GameVariables.Instance.SecondPlayer.Goals == GameVariables.Instance.GoalsLimit)
                    if (TableGoal.GamePlay.IsHost)
                        gameEnd = EndedGame.PlayerLost;
                    else
                        gameEnd = EndedGame.PlayerWon;
            }


            switch (gameEnd)
            {
                case EndedGame.PlayerWon:
                    resultPctPath = "MenusElements/END_YouWon";
                    Statistics.Instance.Wygrana();
                    break;
                case EndedGame.PlayerLost:
                    resultPctPath = "MenusElements/END_YouLost";
                    Statistics.Instance.Przegrana();
                    break;
                case EndedGame.Draw:
                    resultPctPath = "MenusElements/END_Draw";
                    Statistics.Instance.Remis();
                    break;
                default:
                    break;
            }



            menu = new Menu("Backgrounds/Dimmed", new Rectangle(200, 200, 400, 200));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/BackToMainMenuBtn", ButtonType.ShowMainMenu);
            result = new UIPicture(resultPctPath, new Rectangle(50, 50, 700, 100));
            menu.AddElement(result);
            firstPlPoints = new Digit(new Rectangle(30, 180, 120, 120), GameVariables.Instance.FirstPlayer.Goals);
            secondPlPoints = new Digit(new Rectangle(650, 180, 120, 120), GameVariables.Instance.SecondPlayer.Goals);
            menu.AddElement(firstPlPoints);
            menu.AddElement(secondPlPoints);
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            result.DestinationRectangle = new Rectangle(400 - (int)(result.ObjectTexture.Width / 2),
                                                        50,
                                                        result.ObjectTexture.Width,
                                                        result.ObjectTexture.Height);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                menu.PressedButton == ButtonType.ShowMainMenu)
            {
                if (TableGoal.GamePlay != null)
                {
                    TableGoal.GamePlay.Leave(false);
                }
                GameManager.RemoveState(this);
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                {
                    GameManager.RemoveState(state);
                }

                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                GameManager.AddState(new MainMenuState());
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

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
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
                    menu.WasPressed(input.Gestures[0].Position);
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
