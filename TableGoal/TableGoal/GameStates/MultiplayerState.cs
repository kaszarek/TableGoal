using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;

namespace TableGoal
{
    public class MultiplayerState : GameState
    {
        Menu menu;
        CheckBox fieldPicker;
        CheckBox gameTypePicker;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;

        public MultiplayerState()
        {
            menu = new Menu("Backgrounds/Background", new Rectangle(200, -5, 400, 495));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            //menu.AddButton("MenusElements/Local", ButtonType.MultiLocal);
            menu.AddButton("MenusElements/GlobalHost", ButtonType.MultiGlobalHost);
            menu.AddButton("MenusElements/GlobalJoin", ButtonType.MultiGlobalJoin);
            menu.AddButton("MenusElements/WifiHost", ButtonType.MultiWifiHost);
            menu.AddButton("MenusElements/WifiJoin", ButtonType.MultiWifiJoin);
            menu.AddButton("MenusElements/BackBtn", ButtonType.Back);
            fieldPicker = new CheckBox("MenusElements/FieldChosing");
            fieldPicker.DestinationRectangle = new Rectangle(590, 270, 160, 160);
            if (GameVariables.Instance.TypeOfField == PlayField.large)
                fieldPicker.Checked = true;
            gameTypePicker = new CheckBox("MenusElements/GameTypeChosing");
            gameTypePicker.DestinationRectangle = new Rectangle(590, 50, 170, 155);
            if (GameVariables.Instance.IsLimitedByGoals)
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
            if (menu.PressedButton == ButtonType.MultiLocal)
            {
                SetFieldAndTypeOfGame();
                GameVariables.Instance.SecondPlayer.Coach = TeamCoach.HUMAN;
                GameManager.AddState(new SelectionState(true));
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.MultiGlobalHost)
            {
                if (CheckInternet())
                {
                    SetFieldAndTypeOfGame();
                    GameManager.AddState(new GlobalMultiHostState());
                    this.ScreenState = global::TableGoal.ScreenState.Hidden;
                }
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.MultiGlobalJoin)
            {
                if (CheckInternet())
                {
                    GameManager.AddState(new GlobalMultiRoomsState());
                    this.ScreenState = global::TableGoal.ScreenState.Hidden;
                }
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.MultiWifiHost)
            {
                if (CheckWiFi())
                {
                    SetFieldAndTypeOfGame();
                    GameManager.AddState(new WifiHostSelectionState());
                    this.ScreenState = global::TableGoal.ScreenState.Hidden;
                }
                
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.MultiWifiJoin)
            {
                if (CheckWiFi())
                {
                    GameManager.AddState(new WifiRoomsListState());
                    this.ScreenState = global::TableGoal.ScreenState.Hidden;
                }
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

        private bool CheckWiFi()
        {
            if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator) // tylko jak jesteœmy w emulatorze
            {
                return true;
            }

            bool wifiAvailable = NetworkInterfaceHelper.IsConnectedToWiFi();
            if (!wifiAvailable)
            {
                Guide.BeginShowMessageBox("Multiplayer over WiFi?",
                                          "Both players have to be connected to the same WiFi network.\nPlease check your connection.",
                                          new string[] { "Okay" },
                                          0,
                                          MessageBoxIcon.Alert,
                                          null,
                                          null);
            }
            return wifiAvailable;
        }

        private bool CheckInternet()
        {
            if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator) // tylko jak jesteœmy w emulatorze
            {
                return true;
            }

            bool internetAvailable = NetworkInterfaceHelper.IsInternetAvailable();
            if (!internetAvailable)
            {
                Guide.BeginShowMessageBox("Want to play multiplayer?",
                                          "You need to have access to the interner.\nPlease check your connection.",
                                          new string[] { "Okay" },
                                          0,
                                          MessageBoxIcon.Alert,
                                          null,
                                          null);
            }
            return internetAvailable;
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
