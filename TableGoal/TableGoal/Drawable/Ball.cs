using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    class Ball : DrawableGameObject
    {
        public Ball(string textureName)
        {
            this.TextureName = textureName;
            this.Color = Color.White;
            this.Origin = Vector2.Zero;
            this.LayerDepth = 0.0f;
            SetProperScale();
        }

        public void SetProperScale()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    this.Scale = 0.5f;
                    break;
                case PlayField.large:
                    this.Scale = 0.3f;
                    break;
            } 
        }

        public void SetBigBall()
        {
            this.Scale = 0.5f;
        }

        public void SetSmallBall()
        {
            this.Scale = 0.3f;
        }

        public void SetCustomScale(float scale)
        {
            this.Scale = scale;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ObjectTexture,
                             Position,
                             SourceRectangle,
                             Color,
                             Roation,
                             Origin,
                             Scale,
                             SpriteEffects.None,
                             LayerDepth);
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            this.ObjectTexture = contentManager.Load<Texture2D>(TextureName);
            this.Origin = new Vector2(this.ObjectTexture.Width / 2, this.ObjectTexture.Height / 2);
        }
    }
}
