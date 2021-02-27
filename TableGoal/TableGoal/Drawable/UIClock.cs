using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Content;

namespace TableGoal
{
    class UIClock : UIElement
    {
        private readonly int timeInterval = 5;
        private int multiplitation = 0;
        private readonly float boundScale = 1.2f;
        private bool clicked = false;
        private bool clickable = true;

        public bool Clickable
        {
            get { return clickable; }
            set { clickable = value; }
        }

        public int TimeOfPlayInMinutes()
        {
            return timeInterval * (multiplitation + 1);
        }

        public int TimeOfPlayInSeconds()
        {
            return timeInterval * (multiplitation + 1) * 60;
        }

        public UIClock(string textureName, Rectangle destRect)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
        }

        public UIClock(string textureName, Rectangle destRect, float scale)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
            this.SourceRectangle = new Rectangle(multiplitation * 100, 0, 100, 100);
            this.Scale = scale;
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
            this.Origin = new Vector2(this.ObjectTexture.Height / 2, this.ObjectTexture.Height / 2);
            float offset = (this.ObjectTexture.Height - this.ObjectTexture.Height * this.Scale) / 2;
            this.Position = new Vector2(DestinationRectangle.X - offset, DestinationRectangle.Y - offset) + this.Origin;
        }

        public override void Update(GameTime gametime)
        {
            if (clicked)
            {
                if (this.Scale > 0.8f)
                {
                    this.Scale *= 0.98f;
                }
                else
                {
                    clicked = false;
                    this.Scale = .8f;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.ObjectTexture,
                             this.Position,
                             this.SourceRectangle,
                             this.Color,
                             0f,
                             this.Origin,
                             this.Scale,
                             SpriteEffects.None,
                             0f);
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (clickable)
                if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    AudioManager.PlaySound("clock");
                    multiplitation++;
                    multiplitation = multiplitation % 9;
                    this.SourceRectangle = new Rectangle(multiplitation * this.ObjectTexture.Height,
                                                         0,
                                                         this.ObjectTexture.Height,
                                                         this.ObjectTexture.Height);
                    clicked = true;
                    this.Scale = boundScale;
                }
        }

        public void ProgramicClickOnClock()
        {
            AudioManager.PlaySound("clock");
            multiplitation++;
            multiplitation = multiplitation % 9;
            this.SourceRectangle = new Rectangle(multiplitation * this.ObjectTexture.Height,
                                                 0,
                                                 this.ObjectTexture.Height,
                                                 this.ObjectTexture.Height);
            clicked = true;
            this.Scale = boundScale;
        }
    }
}
