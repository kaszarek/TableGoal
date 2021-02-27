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
    class ControllsChangeState : GameState
    {
        Menu menu;
        CombineRatioButtons radiosLeft;
        CombineRatioButtons radiosRight;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;

        public ControllsChangeState()
        {
            this.EnabledGestures = GestureType.Tap;
            menu = new Menu("Backgrounds/Background", new Rectangle(310, 360, 400, 140));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);

            UIPicture host = new UIPicture("HostLbl", new Rectangle(120, 0, 150, 75));
            menu.AddElement(host);

            radiosLeft = new CombineRatioButtons("CtrlsButtons", "CtrlsGestures", new Rectangle(90, 70, 220, 330), CombineRatioButtons.Mode.Vertical);
            if (GameVariables.Instance.FirstPlayer.ShirtsColor == Color.Gold)
            {
                radiosLeft.SetSelectionColor(Color.Red);
            }
            else
            {
                radiosLeft.SetSelectionColor(GameVariables.Instance.FirstPlayer.ShirtsColor);
            }
            radiosLeft.Controls = GameVariables.Instance.FirstPlayer.Controler;
            menu.AddElement(radiosLeft);

            //if (GameVariables.Instance.SecondPlayer.Coach == TeamCoach.HUMAN)
            //{
                UIPicture guest = new UIPicture("GuestLbl", new Rectangle(520, 0, 150, 75));
                menu.AddElement(guest);
                radiosRight = new CombineRatioButtons("CtrlsButtons", "CtrlsGestures", new Rectangle(490, 70, 220, 330), CombineRatioButtons.Mode.Vertical);
                if (GameVariables.Instance.SecondPlayer.ShirtsColor == Color.Gold)
                {
                    radiosRight.SetSelectionColor(Color.Blue);
                }
                else
                {
                    radiosRight.SetSelectionColor(GameVariables.Instance.SecondPlayer.ShirtsColor);
                }
                radiosRight.Controls = GameVariables.Instance.SecondPlayer.Controler;
                menu.AddElement(radiosRight);
            //}

            UIPicture line = new UIPicture("empty4x4", new Rectangle(399, 0, 2, 480));
            line.Color = Color.Black;
            menu.AddElement(line);
            menu.AddButton("MenusElements/BackBtn", ButtonType.Back);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || menu.PressedButton == ButtonType.Back)
            {
                GameVariables.Instance.FirstPlayer.Controler = radiosLeft.Controls;
                if (GameVariables.Instance.SecondPlayer.Coach == TeamCoach.HUMAN)
                    GameVariables.Instance.SecondPlayer.Controler = radiosRight.Controls;
                else
                    GameVariables.Instance.SecondPlayer.Controler = Controlling.NOTSET;
                GameVariables.Instance.UpdateCurrentPlayer();
                AudioManager.PlaySound("selected");
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
