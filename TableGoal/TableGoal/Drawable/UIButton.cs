using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    class UIButton : UIElement
    {
        private string buttonText;
        private bool pressed;
        private Rectangle pressedRect;

        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }

        public Rectangle PressedRect
        {
            get { return pressedRect; }
            set { pressedRect = value; }
        }

        public string ButtonText
        {
            get { return buttonText; }
            set { buttonText = value; }
        }

        public UIButton(string textureName, Rectangle destRect)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
        }

        private void RecalculareRect()
        {
            this.SourceRectangle = new Rectangle(0, 0, ObjectTexture.Bounds.Width / 2, ObjectTexture.Bounds.Height);
            this.pressedRect = new Rectangle(ObjectTexture.Bounds.Width / 2, 0, ObjectTexture.Bounds.Width / 2, ObjectTexture.Bounds.Height);
        }

        public UIButton(string textureName, string context)
        {
            this.TextureName = textureName;
            this.buttonText = context;
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
            {
                pressed = !pressed;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!pressed)
            {
                spriteBatch.Draw(this.ObjectTexture, this.DestinationRectangle, this.SourceRectangle, this.Color);
            }
            else
            {
                spriteBatch.Draw(this.ObjectTexture, this.DestinationRectangle, pressedRect, this.Color);
            }
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
            RecalculareRect();
        }
    }
}
