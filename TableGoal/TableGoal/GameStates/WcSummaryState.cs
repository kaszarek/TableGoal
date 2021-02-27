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
    class WcSummaryState : GameState
    {
        Menu menu;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        int _playersPlace = 0;

        public WcSummaryState(List<Country> finalFour)
        {
            WorldCupProgress.Instance.ClearCurrentMatchState();
            menu = new Menu("Backgrounds/Dimmed", new Rectangle(200, 320, 400, 130));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            if (finalFour.Count == 0)
            {
                UIPicture flag = new UIPicture(Countries.pathToFlags[WorldCupProgress.Instance.SelectedCountry],
                                               new Rectangle(335, 40, 130, 91));
                menu.AddElement(flag);
                menu.AddElement(new UIPicture("WC/dropOut", new Rectangle(125, 180, 550, 140), Color.Red));
                WorldCupProgress.Instance.Clear();
                GameVariables.Instance.ActiveWorldCupMatch = false;
                GameVariables.Instance.WorldCupStarted = false;
            }
            else
            {
                for (int i = 0; i < finalFour.Count; i++)
                {
                    if (finalFour[i].ToString().Contains(WorldCupProgress.Instance.SelectedCountry.ToString()))
                    {
                        _playersPlace = i + 1;
                        break;
                    }
                }
                UIPicture first = new UIPicture(Countries.pathToFlags[finalFour[0]], new Rectangle(335, 40, 130, 91));
                menu.AddElement(first);
                UIPicture second = new UIPicture(Countries.pathToFlags[finalFour[1]], new Rectangle(100, 130, 110, 77));
                menu.AddElement(second);
                UIPicture third = new UIPicture(Countries.pathToFlags[finalFour[2]], new Rectangle(600, 137, 100, 70));
                menu.AddElement(third);
                UIPicture fourth = new UIPicture(Countries.pathToFlags[finalFour[3]], new Rectangle(630, 287, 70, 49));
                menu.AddElement(fourth);
                UIPicture trophyGold = new UIPicture("WC/trophy", new Rectangle(279, 40, 49, 91));
                trophyGold.Color = Color.Gold;
                menu.AddElement(trophyGold);
                UIPicture trophySilver = new UIPicture("WC/trophy", new Rectangle(51, 130, 42, 77));
                trophySilver.Color = Color.Silver;
                menu.AddElement(trophySilver);
                UIPicture trophyBronze = new UIPicture("WC/trophy", new Rectangle(555, 137, 38, 70));
                trophyBronze.Color = Color.SaddleBrown;
                menu.AddElement(trophyBronze);
            }
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                menu.PressedButton == ButtonType.Back)
            {
                GameVariables.Instance.WorldCupStarted = false;
                AudioManager.PlaySound("selected");
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
                if (_playersPlace == 1)
                    Statistics.Instance.WorldCup_PierwszeMiejsce();
                else if (_playersPlace == 2)
                    Statistics.Instance.WorldCup_DrugieMiejsce();
                else if (_playersPlace == 3)
                    Statistics.Instance.WorldCup_TrzecieMiejsce();
                WorldCupProgress.Instance.Clear();
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
