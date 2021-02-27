using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    public enum EndedGame
    {
        PlayerWon,
        PlayerLost,
        HostWon,
        GuestWon,
        Draw,
        Undetermined
    }

    class GameFinishedState : GameState
    {
        Menu menu;
        UIPicture result;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        bool isResuming;
        Digit firstPlPoints;
        Digit secondPlPoints;
        MenuButton GroupTable;

        public GameFinishedState(bool resuming)
        {
            isResuming = resuming;
            this.EnabledGestures = GestureType.Tap;
            string resultPctPath = string.Empty;
            EndedGame gameEnd = EndedGame.Draw;
            if (!GameVariables.Instance.IsLimitedByGoals)
            {
                Statistics.Instance.CzasMeczu(GameVariables.Instance.TotalTime);
                if (GameVariables.Instance.SecondPlayer.Coach == TeamCoach.HUMAN)
                {
                    if (GameVariables.Instance.FirstPlayer.Goals >
                        GameVariables.Instance.SecondPlayer.Goals)
                        gameEnd = EndedGame.HostWon;
                    else if (GameVariables.Instance.FirstPlayer.Goals <
                             GameVariables.Instance.SecondPlayer.Goals)
                        gameEnd = EndedGame.GuestWon;
                }
                else
                {
                    if (GameVariables.Instance.FirstPlayer.Goals >
                        GameVariables.Instance.SecondPlayer.Goals)
                        gameEnd = EndedGame.PlayerWon;
                    else if (GameVariables.Instance.FirstPlayer.Goals <
                             GameVariables.Instance.SecondPlayer.Goals)
                        gameEnd = EndedGame.PlayerLost;
                }
            }
            else
            {
                Statistics.Instance.CzasMeczu(GameVariables.Instance.TimeLeft);
                if (GameVariables.Instance.SecondPlayer.Coach == TeamCoach.HUMAN)
                {
                    if (GameVariables.Instance.FirstPlayer.Goals == GameVariables.Instance.GoalsLimit)
                        gameEnd = EndedGame.HostWon;
                    if (GameVariables.Instance.SecondPlayer.Goals == GameVariables.Instance.GoalsLimit)
                        gameEnd = EndedGame.GuestWon;
                }
                else
                {
                    if (GameVariables.Instance.FirstPlayer.Goals == GameVariables.Instance.GoalsLimit)
                        gameEnd = EndedGame.PlayerWon;

                    if (GameVariables.Instance.SecondPlayer.Goals == GameVariables.Instance.GoalsLimit)
                        gameEnd = EndedGame.PlayerLost;
                }
            }
            

            switch (gameEnd)
            {
                case EndedGame.PlayerWon:
                    resultPctPath = "MenusElements/END_YouWon";
                    if (GameVariables.Instance.ActiveWorldCupMatch)
                        WorldCupProgress.Instance.ResultWithCurrentOpponent = EndedGame.PlayerWon;
                    Statistics.Instance.Wygrana();
                    break;
                case EndedGame.PlayerLost:
                    resultPctPath = "MenusElements/END_YouLost";
                    if (GameVariables.Instance.ActiveWorldCupMatch)
                        WorldCupProgress.Instance.ResultWithCurrentOpponent = EndedGame.PlayerLost;
                    Statistics.Instance.Przegrana();
                    break;
                case EndedGame.HostWon:
                    resultPctPath = "MenusElements/END_HostWon";
                    break;
                case EndedGame.GuestWon:
                    resultPctPath = "MenusElements/END_GuestWon";
                    break;
                case EndedGame.Draw:
                    resultPctPath = "MenusElements/END_Draw";
                    if (GameVariables.Instance.ActiveWorldCupMatch)
                        WorldCupProgress.Instance.ResultWithCurrentOpponent = EndedGame.Draw;
                    if (GameVariables.Instance.SecondPlayer.Coach == TeamCoach.CPU)
                        Statistics.Instance.Remis();
                    break;
                default:
                    break;
            }
            if (GameVariables.Instance.ActiveWorldCupMatch)
            {
                WorldCupProgress.Instance.Serialize();
            }
            menu = new Menu("Backgrounds/Dimmed", new Rectangle(200, 152, 400, 288));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            GroupTable = new MenuButton("MenusElements/GroupTableBtn", ButtonType.WCShowTable);
            GroupTable.Visible = true;
            menu.AddButton(GroupTable);
            if (!GameVariables.Instance.ActiveWorldCupMatch)
            {
                menu.RemoveButton(ButtonType.WCShowTable);
                menu.AddButton("MenusElements/RetryBtn", ButtonType.Retry);
                menu.AddButton("MenusElements/NewGameBtn", ButtonType.NewGame);
            }
            menu.AddButton("MenusElements/BackToMainMenuBtn", ButtonType.ShowMainMenu);
            result = new UIPicture(resultPctPath, new Rectangle(50,50,700,100));
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

        public override void Update(GameTime gameTime)
        {
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || menu.PressedButton == ButtonType.ShowMainMenu)
            {
                menu.PressedButton = ButtonType.None;
                ShowMainMenu();
            }
            if (menu.PressedButton == ButtonType.Retry)
            {
                menu.PressedButton = ButtonType.None;
                GameState[] states = GameManager.GetStates();
                GameplayState gameplayState = null;
                foreach (GameState state in states)
                {
                    if (state is GameplayState)
                        gameplayState = state as GameplayState;
                }
                GameVariables.Instance.RestartGame();
                gameplayState.Restart();
                GameManager.RemoveState(this);
            }
            if (menu.PressedButton == ButtonType.NewGame)
            {
                menu.PressedButton = ButtonType.None;
                ShowMainMenu();
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                {
                    if (state is MainMenuState)
                        state.ScreenState = global::TableGoal.ScreenState.Hidden;
                }
                GameManager.AddState(new NewGameMenu());
            }
            if (menu.PressedButton == ButtonType.WCShowTable)
            {
                menu.PressedButton = ButtonType.None;
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                {
                    GameManager.RemoveState(state);
                }
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                GameManager.AddState(new WcGroupTableState());
            }
        }

        private void ShowMainMenu()
        {
            GameManager.RemoveState(this);
            GameState[] states = GameManager.GetStates();
            foreach (GameState state in states)
            {
                GameManager.RemoveState(state);
            }
            // powrót do main menu oznacza rezygnacje z gry !!
            GameVariables.Instance.RestartGame();
            GameVariables.Instance.ResetVariables();
            GameManager.AddState(new MainMenuState());
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
            if (isResuming)
            {
                firstPlPoints.Number = GameVariables.Instance.FirstPlayer.Goals;
                secondPlPoints.Number = GameVariables.Instance.SecondPlayer.Goals;
                if (GameVariables.Instance.WorldCupStarted)
                {
                    GroupTable.Visible = true;
                    menu.RemoveButton(ButtonType.Retry);
                    menu.RemoveButton(ButtonType.NewGame);
                }
                isResuming = false;
            }
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
