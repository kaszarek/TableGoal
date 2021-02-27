using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;

namespace TableGoal
{
    class UIPicture : UIElement
    {
        private bool pressed;
        bool usePosition = false;

        public bool UseVectorPositionToDraw
        {
            get { return usePosition; }
            set { usePosition = value; }
        }

        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }

        public UIPicture(string textureName, Vector2 place)
        {
            this.TextureName = textureName;
            this.Position = place;
            usePosition = true;
        }

        public UIPicture(string textureName, Rectangle destRect)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
        }

        public UIPicture(string textureName, Rectangle destRect, Color color)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
            this.Color = color;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                if (usePosition)
                    spriteBatch.Draw(this.ObjectTexture, this.Position, this.Color);
                else
                    spriteBatch.Draw(this.ObjectTexture, this.DestinationRectangle, this.SourceRectangle, this.Color);
            }
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (Visible)
                if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    pressed = !pressed;
                }
        }

        public bool WasPressed(Vector2 tapPoint)
        {
            if (Visible)
                if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    AudioManager.PlaySound("selected");
                    return true;
                }
            return false;
        }
    }
}
