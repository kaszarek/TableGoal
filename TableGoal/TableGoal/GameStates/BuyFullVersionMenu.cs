using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Phone.Tasks;
using System.Diagnostics;

namespace TableGoal
{
    class BuyFullVersionMenu : GameState
    {
        Menu menu;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;

        public BuyFullVersionMenu()
        {
            this.EnabledGestures = GestureType.Tap;
            menu = new Menu("Backgrounds/Dimmed", new Rectangle(200, 100, 400, 310));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/BuyBtn", ButtonType.BuyFullVersion);
            menu.AddButton("MenusElements/BackToMainMenuBtn", ButtonType.ShowMainMenu);
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (menu.PressedButton == ButtonType.BuyFullVersion)
            {
                MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
                marketplaceDetailTask.ContentIdentifier = "faebea51-caca-4421-912f-5bbabdcb16e7";
                try
                {
                    marketplaceDetailTask.Show();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine(" =========  BuyFullVer: EXCEPTION when calling DetailTask  =============");
                    Debug.WriteLine(ex.Message);
#endif
                }
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.ShowMainMenu || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                GameManager.RemoveState(this);
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
                
                GameManager.AddState(new MainMenuState());
                menu.PressedButton = ButtonType.None;
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
