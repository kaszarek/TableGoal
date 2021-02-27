using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableGoal
{
    /*
     * 
     *  TO DO !!!
     *  For better optimization and to make code more cleaner.
     * 
     */
    public class DrawableOpponent : UIElement, IOpponent
    {
        WifiPlayer _wifiPlayer;
        AIPlayer _aiPlayer;

        public DrawableOpponent(ref Board board)
        {
            if (GameVariables.Instance.IsWiFiGame())
                _wifiPlayer = new WifiPlayer();
            else
                _aiPlayer = new AIPlayer(ref board);
        }

        public void MoveMade(string move)
        { }

        public void MakeMove()
        { }

        public bool isThinking()
        {
            return false;
        }

        public void CancelMove()
        { }

        public void CheckDifficultyLevel() 
        { }

        public void NotifyAboutMoves()
        { }

        public void UnregisterEvents()
        { }

        public override void LoadTexture(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gametime)
        {
            base.Update(gametime);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void HandleInput(Microsoft.Xna.Framework.Vector2 tapPoint)
        {
            base.HandleInput(tapPoint);
        }
    }
}
