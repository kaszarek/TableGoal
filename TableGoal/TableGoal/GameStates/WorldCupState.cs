using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;

namespace TableGoal
{
    public enum StateOfPlay
    {
        GROUP_PHASE,            // 8 grup po 4 dru퓓ny
        ONE_EIGHT_PHASE,        // 8 meczy -> 16 dru퓓n
        QUATER_FINAL_PHASE,     // 4 mecze -> 8 dru퓓n
        SEMI_FINAL_PHASE,       // 2 mecze -> 4 dru퓓ny
        FINAL_PHASE,            // mecz fina쿽wy -> 2 dru퓓ny
        SMALL_FINAL_PHASE,      // mecz o 3cie miejsce -> 2 dru퓓ny
        UNKNOWN
    }

    public class WorldCupState : GameState
    {
        Menu menu;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        UIPicture flag;
        bool overrideExistingSaveGame = false;

        public WorldCupState()
        {
            WorldCupProgress.Instance.Deserialize();
            if (!TableGoal.WcProgressOK)
                WorldCupProgress.Instance.ClearIsolatedStorageRelatedData();
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 100, 400, 320));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/NewGameBtn", ButtonType.WCNewGame);
            if (WorldCupProgress.Instance.Populated)
            {
                menu.AddButton("MenusElements/ContinueBtn", ButtonType.WCContinue);
            }
            menu.AddButton("MenusElements/BackBtn", ButtonType.Back);
            UIPicture name = new UIPicture("MenusElements/WorldCupBtn", new Rectangle(0, 0, 200, 59));
            name.Color = Color.Black;
            menu.AddElement(name);
            if (WorldCupProgress.Instance.Populated)
            {
                flag = new UIPicture(Countries.pathToFlags[WorldCupProgress.Instance.SelectedCountry], new Rectangle(550, 225, 100, 60));
                menu.AddElement(flag);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
        }

        /// <summary>
        /// Callback function for MessageBox
        /// </summary>
        /// <param name="ar">Encapsulated result.</param>
        private void OnMessageBoxClosed(IAsyncResult ar)
        {
            int? buttonIndex = Guide.EndShowMessageBox(ar);
            switch (buttonIndex)
            {
                case 0:
                    //Deployment.Current.Dispatcher.BeginInvoke(() => overrideExistingSaveGame = true);
                    overrideExistingSaveGame = true;
                    WorldCupProgress.Instance.Clear();
                    flag.Visible = false;
                    menu.RemoveButton(ButtonType.WCContinue);
                    break;
                case 1:
                    //Deployment.Current.Dispatcher.BeginInvoke(() => overrideExistingSaveGame = false);
                    overrideExistingSaveGame = false;
                    menu.PressedButton = ButtonType.None;
                    break;
                default:
                    break;
            }
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
                GameVariables.Instance.WorldCupStarted = false;
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
            if (menu.PressedButton == ButtonType.WCNewGame)
            {
                if (overrideExistingSaveGame)
                {
                    WorldCupProgress.Instance.Clear();
                    GameVariables.Instance.SecondPlayer.Coach = TeamCoach.CPU;
                    GameManager.AddState(new WcFlagSelectionState());
                    this.ScreenState = global::TableGoal.ScreenState.Hidden;
                    menu.PressedButton = ButtonType.None;
                    overrideExistingSaveGame = false;
                }
                if (WorldCupProgress.Instance.Populated)
                {
                    if (!Guide.IsVisible)
                        Guide.BeginShowMessageBox("New World Cup",
                                                  "Save game exists. Do you want to start a new World Cup?",
                                                  new string[] { "Yes", "No" },
                                                  0,
                                                  MessageBoxIcon.Warning,
                                                  new AsyncCallback(OnMessageBoxClosed),
                                                  null);
                }
                else
                    overrideExistingSaveGame = true;
            }
            if (menu.PressedButton == ButtonType.WCContinue)
            {
                if (WorldCupProgress.Instance.Populated)
                {
                    GameVariables.Instance.SecondPlayer.Coach = TeamCoach.CPU;
                    GameVariables.Instance.WorldCupStarted = true;
                    GameVariables.Instance.ActiveWorldCupMatch = true;
                    GameState[] states = GameManager.GetStates();
                    foreach (GameState state in states)
                        GameManager.RemoveState(state);
                    GameManager.AddState(new WcGroupTableState());
                }
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.Back)
            {
                GameManager.RemoveState(this);
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
