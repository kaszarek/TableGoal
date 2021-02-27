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
    class WcSelectedFlagState : GameState
    {
        Menu menu;
        UIPicture flag;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;

        public WcSelectedFlagState()
        {
            menu = new Menu("Backgrounds/Dimmed", new Rectangle(200, 250, 400, 208));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/ContinueBtn", ButtonType.WCContinue);
            menu.AddButton("MenusElements/BackBtn", ButtonType.Back);
            flag = new UIPicture(Countries.pathToFlags[WorldCupProgress.Instance.SelectedCountry], new Rectangle(300, 70, 200, 120));
            menu.AddElement(flag);
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
            if (menu.PressedButton == ButtonType.WCContinue)
            {
                GameVariables.Instance.WorldCupStarted = true;
                GameVariables.Instance.ActiveWorldCupMatch = true;
                WorldCupProgress.Instance.PhaseOfTheWorldCup = StateOfPlay.GROUP_PHASE;
                Statistics.Instance.WejscieDoWorldCup();
                menu.PressedButton = ButtonType.None;
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
                GameManager.AddState(new WcGroupTableState()); 
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
