using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    class CheckBox : UIElement
    {
        private bool _checked;
        private Rectangle checkedRect;
        private Rectangle uncheckedRect;

        public bool Checked
        {
            get { return _checked; }
            set { _checked = value; }
        }

        public CheckBox(string textureName)
        {
            this._checked = false;
            this.TextureName = textureName;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                if (_checked)
                {
                    spriteBatch.Draw(this.ObjectTexture, this.DestinationRectangle, this.checkedRect, Color.White);
                }
                else
                {
                    spriteBatch.Draw(this.ObjectTexture, this.DestinationRectangle, this.uncheckedRect, Color.White);
                }
            }
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
            uncheckedRect = new Rectangle(0, 0, this.ObjectTexture.Bounds.Width / 2, this.ObjectTexture.Bounds.Height);
            checkedRect = new Rectangle(this.ObjectTexture.Bounds.Width / 2, 0, this.ObjectTexture.Bounds.Width / 2, this.ObjectTexture.Bounds.Height);
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (Visible)
            {
                if (this.DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    _checked = !_checked;
                    AudioManager.PlaySound("selected");
                }
            }
        }
    }
}
