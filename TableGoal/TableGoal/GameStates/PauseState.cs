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
    public class PauseState : GameState
    {
        Menu menu;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        bool isResuming;
        Digit firstPlPoints;
        Digit secondPlPoints;

        public PauseState(bool resuming)
        {
            isResuming = resuming;
            this.EnabledGestures = GestureType.Tap;
            menu = new Menu("Backgrounds/Dimmed", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/ResumeBtn", ButtonType.ResumeGame);
            if (!GameVariables.Instance.ActiveWorldCupMatch)
            {
                menu.AddButton("MenusElements/RetryBtn", ButtonType.Retry);
            }
            else
            {
                AddFlags(false);
            }
            menu.AddButton("MenusElements/OptionsBtn", ButtonType.Options);
            menu.AddButton("MenusElements/HowToPlayBtn", ButtonType.HowToPlay);
            menu.AddButton("MenusElements/BackToMainMenuBtn", ButtonType.ShowMainMenu);
            firstPlPoints = new Digit(new Rectangle(30, 180, 120, 120), GameVariables.Instance.FirstPlayer.Goals);
            secondPlPoints = new Digit(new Rectangle(650, 180, 120, 120), GameVariables.Instance.SecondPlayer.Goals);
            menu.AddElement(firstPlPoints);
            menu.AddElement(secondPlPoints);
        }

        /// <summary>
        /// Tworzy flagi zespo³ów po bokach ekranu i malutki puchar na œrodku ekranu nad menu.
        /// </summary>
        /// <param name="loadFlagsAlso">Czy dla stworzonych flag maj¹ zostaæ wczytane tekstury.</param>
        private void AddFlags(bool loadFlagsAlso)
        {
            if (!GameVariables.Instance.ActiveWorldCupMatch)
                return;
            if (WorldCupProgress.Instance.SelectedCountry == Country.UNKNOWN)
                return;
            if (WorldCupProgress.Instance.CurrentOpponet == Country.UNKNOWN)
                return;
            UIPicture playerCountry = new UIPicture(Countries.pathToFlags[WorldCupProgress.Instance.SelectedCountry], new Rectangle(10, 10, 100, 70));
            UIPicture opponentCountry = new UIPicture(Countries.pathToFlags[WorldCupProgress.Instance.CurrentOpponet], new Rectangle(690, 10, 100, 70));
            UIPicture trophyG = new UIPicture("trophy", new Rectangle(390, 0, 27, 50));
            trophyG.Color = Color.Gold;
            if (loadFlagsAlso)
            {
                playerCountry.LoadTexture(GameManager.Game.Content);
                opponentCountry.LoadTexture(GameManager.Game.Content);
                trophyG.LoadTexture(GameManager.Game.Content);
            }
            menu.AddElement(trophyG);
            menu.AddElement(playerCountry);
            menu.AddElement(opponentCountry);
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
            if (menu.PressedButton == ButtonType.ResumeGame)
            {
                GameManager.RemoveState(this);
            }
            if (menu.PressedButton == ButtonType.Retry)
            {
                GameState[] states = GameManager.GetStates();
                GameplayState gameplayState = null;
                foreach (GameState state in states)
                {
                    if (state is GameplayState)
                        gameplayState = state as GameplayState;
                }

                if (GameVariables.Instance.SecondPlayer.Coach == TeamCoach.CPU)
                    Statistics.Instance.Przerwany();

                if (GameVariables.Instance.IsLimitedByGoals)
                {
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TimeLeft);
                }
                else
                {
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TotalTime - GameVariables.Instance.TimeLeft);
                }
                GameVariables.Instance.RestartGame();
                gameplayState.Restart();
                GameManager.RemoveState(this);
            }
            if (menu.PressedButton == ButtonType.Options)
            {
                GameManager.AddState( new OptionMenuState());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.HowToPlay)
            {
                GameManager.AddState(new HowToPlayState());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.ShowMainMenu)
            {
                /*
                 * Jeœli chcemy wyjœæ z meczu i to jest mecz w trybie mistrzostw
                 * to zapisujemy stan mistrzostw.
                 * W przeciwnym razie podbijamy licznik przerwanych meczy.
                 */
                if (GameVariables.Instance.ActiveWorldCupMatch)
                {
                    WorldCupProgress.Instance.SaveCurrentMatchState();
                }
                else
                {
                    if (GameVariables.Instance.SecondPlayer.Coach == TeamCoach.CPU)
                        Statistics.Instance.Przerwany();
                }

                GameManager.RemoveState(this);
                GameState[] states = GameManager.GetStates();
                GameplayState gameplayState = null;
                foreach (GameState state in states)
                {
                    if (state is GameplayState)
                        gameplayState = state as GameplayState;
                }
                gameplayState.StopAI();
                GameManager.RemoveState(gameplayState);
                if (GameVariables.Instance.IsLimitedByGoals)
                {
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TimeLeft);
                }
                else
                {
                    Statistics.Instance.CzasMeczu(GameVariables.Instance.TotalTime - GameVariables.Instance.TimeLeft);
                }
                // powrót do main menu oznacza rezygnacje z gry !!
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
            if (isResuming)
            {
                firstPlPoints.Number = GameVariables.Instance.FirstPlayer.Goals;
                secondPlPoints.Number = GameVariables.Instance.SecondPlayer.Goals;
                if (GameVariables.Instance.WorldCupStarted)
                {
                    menu.RemoveButton(ButtonType.Retry);
                    AddFlags(true);
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
