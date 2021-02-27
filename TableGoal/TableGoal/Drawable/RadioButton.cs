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
    class RadioButton : UIElement
    {
        private bool selected;
        UIPicture picked;
        Color markingColor;

        public Color MarkingColor
        {
            set { markingColor = value; picked.Color = markingColor; }
        }

        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public void Select()
        {
            this.selected = true;
        }

        public RadioButton(string textureName)
        {
            this.TextureName = textureName;
            picked = new UIPicture("Picked", this.DestinationRectangle);
            selected = false;
        }

        public void DestinationRectChanged()
        {
            picked.DestinationRectangle = this.DestinationRectangle;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.ObjectTexture, this.DestinationRectangle, this.Color);
            if (selected)
                picked.Draw(spriteBatch);
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
            picked.LoadTexture(contentManager);
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
            {
                selected = true;
                AudioManager.PlaySound("selected");
            }
        }
    }
}
