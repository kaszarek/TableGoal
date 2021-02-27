using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Microsoft.Advertising.Mobile.Xna;
using System.Diagnostics;

namespace TableGoal
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GameStatesManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        List<GameState> states = new List<GameState>();
        List<GameState> toUpdate = new List<GameState>();
        List<GameState> toDraw = new List<GameState>();

        Input input = new Input();

        SpriteBatch spriteBatch;

        bool isInitialized = false;

        DrawableAd smallBannerAd;
        DrawableAd mediumBannerAd;

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        public GameStatesManager(Game game)
            : base(game)
        {
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
            GameVariables.Instance.NextMoveForFirstPlayer();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
            if (ApplicationLicence.IsTrialMode)
            {
                //10805088
                //Image300_50
                smallBannerAd = AdGameComponent.Current.CreateAd("10805088", new Rectangle(0, 430, 300, 50));
                smallBannerAd.BorderEnabled = true;
                smallBannerAd.ErrorOccurred += new EventHandler<Microsoft.Advertising.AdErrorEventArgs>(smallBannerAd_ErrorOccurred);

                //10805087
                //Image480_80
                mediumBannerAd = AdGameComponent.Current.CreateAd("10805087", new Rectangle(160, -65, 480, 80));
                mediumBannerAd.BorderEnabled = false;
                mediumBannerAd.ErrorOccurred += new EventHandler<Microsoft.Advertising.AdErrorEventArgs>(mediumBannerAd_ErrorOccurred);
            }
        }

        void mediumBannerAd_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Debug.WriteLine(String.Format("MEDIUM AdControl error: {0}", e.Error.Message));
        }

        void smallBannerAd_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Debug.WriteLine(String.Format("SMALL AdControl error: {0}", e.Error.Message));
        }

        protected override void LoadContent()
        {
            ContentManager content = Game.Content;

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Game.Services.AddService(typeof(SpriteBatch), spriteBatch);

            foreach (GameState state in states)
            {
                state.LoadContent();
            }
        }

        protected override void UnloadContent()
        {
            foreach (GameState state in states)
            {
                state.UnloadContent();
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            input.Update();
            toUpdate.Clear();
            foreach (GameState state in states)
            {
                if (state.ScreenState == ScreenState.Active)
                    toUpdate.Add(state);
            }

            while (toUpdate.Count > 0)
            {
                GameState state = toUpdate[toUpdate.Count - 1];
                toUpdate.RemoveAt(toUpdate.Count - 1);
                state.Update(gameTime);
                state.HandleInput(gameTime, input);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            toDraw.Clear();
            foreach (GameState state in states)
            {
                if (state.ScreenState == ScreenState.Hidden)
                    continue;
                toDraw.Add(state);
            }
            while (toDraw.Count > 0)
            {
                GameState state = toDraw[0];
                toDraw.RemoveAt(0);
                state.Draw(gameTime);
            }
        }

        public void AddState(GameState state)
        {
            foreach (GameState gs in states)
            {
                if (gs.GetType() == state.GetType())
                {
                    return;
                }
            }

            state.GameManager = this;
            if (isInitialized)
            {
                state.LoadContent();
            }
            states.Add(state);
            TouchPanel.EnabledGestures = state.EnabledGestures;
            CheckCurrentState(state);
        }

        private void CheckCurrentState(GameState state)
        {
            if (smallBannerAd != null)
            {
                // podwójne zabezpieczenie przez wyœwietlaniem reklam w przypadku gdy aplikacja nie zap³acona
                if (!ApplicationLicence.IsTrialMode)
                {
                    smallBannerAd.Visible = false;
                    mediumBannerAd.Visible = false;
                    return;
                }
                smallBannerAd.Visible = true;
                smallBannerAd.DisplayRectangle = new Rectangle(0, 430, 300, 50);
                mediumBannerAd.Visible = false;
                if (state is SelectionState)
                {
                    smallBannerAd.Visible = false;
                }
                if (state is MultiplayerState)
                {
                    smallBannerAd.DisplayRectangle = new Rectangle(500, 430, 300, 50);
                }
                if (state is MainMenuState)
                {
                    smallBannerAd.DisplayRectangle = new Rectangle(250, 440, 300, 50);
                    smallBannerAd.Visible = true;
                }
                if (state is WifiHostSelectionState)
                {
                    smallBannerAd.DisplayRectangle = new Rectangle(500, 430, 300, 50);
                }
                if (state is PresentStatisticsState)
                {
                    smallBannerAd.DisplayRectangle = new Rectangle(500, 430, 300, 50);
                }
                if (state is PlayerMultiStatsState)
                {
                    smallBannerAd.DisplayRectangle = new Rectangle(500, 430, 300, 50);
                }
                if (state is WcGroupTableState)
                {
                    smallBannerAd.DisplayRectangle = new Rectangle(-70, 430, 300, 50);
                }
                if (state is WifiEndGameState)
                {
                    smallBannerAd.Visible = true;
                    smallBannerAd.DisplayRectangle = new Rectangle(250, 0, 300, 50);

                    mediumBannerAd.Visible = true;
                    mediumBannerAd.DisplayRectangle = new Rectangle(160, 400, 480, 80);
                }
                if (state is GameplayState || state is GlobalMultiGameplayState)
                {
                    smallBannerAd.Visible = false;
                    mediumBannerAd.DisplayRectangle = new Rectangle(160, -65, 480, 80);
                    mediumBannerAd.Visible = true;
                }
                if (state is PauseState)
                {
                    mediumBannerAd.DisplayRectangle = new Rectangle(320, 430, 480, 80);
                    mediumBannerAd.Visible = true;
                }
                if (state is GameFinishedState || state is GlobalMultiEndGameState)
                {
                    smallBannerAd.Visible = true;
                    smallBannerAd.DisplayRectangle = new Rectangle(250, 0, 300, 50);
                    mediumBannerAd.Visible = true;
                    mediumBannerAd.DisplayRectangle = new Rectangle(160, 415, 480, 80);
                }
                if (state is PlyerProfileState)
                {
                    mediumBannerAd.Visible = true;
                    mediumBannerAd.DisplayRectangle = new Rectangle(160, 415, 480, 80);
                    smallBannerAd.Visible = true;
                    smallBannerAd.DisplayRectangle = new Rectangle(250, 430, 300, 50); 
                }
                if (state is GlobalMultiHostState)
                {
                    smallBannerAd.Visible = true;
                    smallBannerAd.DisplayRectangle = new Rectangle(500, 0, 300, 50);
                }
                if (state is GlobalMultiRoomsState)
                {
                    mediumBannerAd.DisplayRectangle = new Rectangle(500, 430, 480, 80);
                    mediumBannerAd.Visible = true;
                }
                if (state is PipTalkSelectionState)
                {
                    smallBannerAd.DisplayRectangle = new Rectangle(0, 0, 300, 50);
                    smallBannerAd.Visible = true;
                }
                if (state is GlobalMultiLobbyState || state is WifiLobbyState)
                {
                    mediumBannerAd.DisplayRectangle = new Rectangle(160, 50, 480, 80);
                    mediumBannerAd.Visible = true;
                }                
            }
        }

        public void RemoveState(GameState state)
        {
            if (isInitialized)
            {
                state.UnloadContent();
            }
            states.Remove(state);
            if (states.Count > 0)
            {
                TouchPanel.EnabledGestures = states[states.Count - 1].EnabledGestures;
                states[states.Count - 1].ScreenState = ScreenState.Active;
                CheckCurrentState(states.Last());
            }
        }

        public GameState[] GetStates()
        {
            return states.ToArray();
        }
    }
}
