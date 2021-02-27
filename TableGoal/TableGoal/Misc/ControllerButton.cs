using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    class ControllerButton : DrawableGameObject
    {
        public TypeOfMove Type {get; set;}

        public ControllerButton(string textureName, Rectangle destinationRect, Rectangle sourceRect, TypeOfMove type)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destinationRect;
            this.SourceRectangle = sourceRect;
            this.Type = type;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
        }
    }
}
