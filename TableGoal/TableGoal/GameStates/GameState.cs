using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;


namespace TableGoal
{

    public enum ScreenState
    {
        Active,
        Visible,
        Hidden
    }
    
    public abstract class GameState
    {
        ScreenState screenState = ScreenState.Active;

        public ScreenState ScreenState
        {
            get { return screenState; }
            set { screenState = value; }
        }

        GameStatesManager gameManager;

        public GameStatesManager GameManager
        {
            get { return gameManager; }
            internal set { gameManager = value; }
        }

        GestureType enabledGestures = GestureType.Tap;

        public GestureType EnabledGestures
        {
            get { return enabledGestures; }
            set { enabledGestures = value; }
        }

        
        /// <summary>
        /// Load graphics content for the screen.
        /// </summary>
        public virtual void LoadContent() { }


        /// <summary>
        /// Unload content for the screen.
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Allows the screen to handle user input. Unlike Update, this method
        /// is only called when the screen is active, and not when some other
        /// screen has taken the focus.
        /// </summary>
        public virtual void HandleInput(GameTime gameTime, Input input) { }

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public virtual void Draw(GameTime gameTime) { }
    }
}
