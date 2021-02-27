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
    class WifiLobbyState : GameState
    {
        Menu menu;
        Texture2D ball;
        Rectangle ballOnScreen;
        float rotationAngle = .0f;
        Vector2 offsetForBall;
        Point actualPosition;
        int moveIncrement = 10;
        float rotationChanges = .073f;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        string messageToPlayer;
        SpriteFont _Font;

        public WifiLobbyState(string playerName)
        {
            if (playerName != null)
                TableGoal.GamePlay.Join(playerName, true);

            menu = new Menu("Backgrounds/Background", new Rectangle(200, 350, 400, 108));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/BackBtn", ButtonType.Back);
            ballOnScreen = new Rectangle(25, 25, 50, 50);
            offsetForBall = new Vector2(25, 25);
            actualPosition = new Point(25, 25);
            rotationChanges = moveIncrement / offsetForBall.X;
            messageToPlayer = "waiting for the opponent . . .";
            TableGoal.GamePlay.StartGame += new StartGameHandler(GamePlay_StartGame);
            TableGoal.GamePlay.ChallangeBack += new ChallangeBackEventHandler(GamePlay_ChallangeBack);
            TableGoal.GamePlay.ConnectionProblem += new ConnectionProblemEventHandler(GamePlay_ConnectionProblem);
            menu.AddElement(new MultiplayerTips(new Vector2(400, 214)));
        }

        void GamePlay_ConnectionProblem(object sender, EventArgs e)
        {
            ShowMainMenu();
        }

        void GamePlay_ChallangeBack(object sender, EventArgs e)
        {
            ShowMainMenu();
        }

        void GamePlay_StartGame(object sender, EventArgs e)
        {
            StartGame();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            spriteBatch.Draw(ball,
                             ballOnScreen,
                             null,
                             Color.White,
                             rotationAngle,
                             offsetForBall,
                             SpriteEffects.None,
                             0.5f);

            spriteBatch.DrawString(_Font,
                                   messageToPlayer,
                                   new Vector2(100, 139),
                                   Color.Black);
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            ball = GameManager.Game.Content.Load<Texture2D>("ball");
            _Font = GameManager.Game.Content.Load<SpriteFont>("Fonts/SketchRockwell");
        }

        public override void Update(GameTime gameTime)
        {
            MoveBall();
            menu.Update(gameTime);
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                ShowMainMenu();
            }
            if (menu.PressedButton == ButtonType.Back)
            {
                ShowMainMenu();
            }
        }

        private void StartGame()
        {
            Statistics.Instance.ZaczynamKolejnyMecz();
            TableGoal.GamePlay.StartGame -= new StartGameHandler(GamePlay_StartGame);
            TableGoal.GamePlay.ChallangeBack -= new ChallangeBackEventHandler(GamePlay_ChallangeBack);
            TableGoal.GamePlay.ConnectionProblem -= new ConnectionProblemEventHandler(GamePlay_ConnectionProblem);
            GameState[] states = GameManager.GetStates();
            foreach (GameState state in states)
                GameManager.RemoveState(state);
            GameManager.AddState(new GameplayState(false));
        }

        private void ShowMainMenu()
        {
            if (TableGoal.GamePlay != null)
            {
                TableGoal.GamePlay.StartGame -= new StartGameHandler(GamePlay_StartGame);
                TableGoal.GamePlay.ChallangeBack -= new ChallangeBackEventHandler(GamePlay_ChallangeBack);
                TableGoal.GamePlay.ConnectionProblem -= new ConnectionProblemEventHandler(GamePlay_ConnectionProblem);
                TableGoal.GamePlay.Leave(false);
            }
            TableGoal.Players.Clear();
            GameState[] states = GameManager.GetStates();
            foreach (GameState state in states)
                if (!(state is MainMenuState) &&
                    !(state is NewGameMenu) &&
                    !(state is MultiplayerState))
                GameManager.RemoveState(state);
        }

        private void MoveBall()
        {

            rotationAngle -= (rotationChanges);
            if (rotationAngle > MathHelper.TwoPi)
                rotationAngle -= MathHelper.TwoPi;
            if (actualPosition.Y <= 25)
                actualPosition.X += moveIncrement;
            if (actualPosition.X >= 775)
                actualPosition.Y += moveIncrement;
            if (actualPosition.Y >= 455)
                actualPosition.X -= moveIncrement;
            if (actualPosition.X <= 25)
                actualPosition.Y -= moveIncrement;

            ballOnScreen.Location = new Point(actualPosition.X, actualPosition.Y);
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
