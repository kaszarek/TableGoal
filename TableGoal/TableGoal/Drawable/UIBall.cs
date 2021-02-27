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
    class UIBall : UIElement
    {        
        private readonly int goalsInterval = 1;
        private int multiplitation = 0;
        private readonly float boundScale = 2.1f;
        private bool animationOngoing = false;
        private bool pressed = false;
        private bool clickable = true;

        public bool Clickable
        {
            get { return clickable; }
            set { clickable = value; }
        }

        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }             

        public bool AnimationOngoing
        {
            get { return animationOngoing; }
            set { animationOngoing = value; }
        }

        public int GoalsLimit()
        {
            return goalsInterval * (multiplitation + 1);
        }
        
        public UIBall(string textureName, Rectangle destRect)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
        }

        public UIBall(string textureName, Rectangle destRect, float scale)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
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
            if (animationOngoing)
            {
                if (this.Scale > 1.4f)
                {
                    this.Scale *= 0.98f;
                }
                else
                {
                    animationOngoing = false;
                    this.Scale = 1.4f;
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
                    AudioManager.PlaySound("kick");
                    multiplitation++;
                    multiplitation = multiplitation % 15;
                    animationOngoing = true;
                    this.Scale = boundScale;
                    pressed = !pressed;
                }
        }

        public void ProgramicClickOnBall()
        {
            AudioManager.PlaySound("kick");
            multiplitation++;
            multiplitation = multiplitation % 15;
            animationOngoing = true;
            this.Scale = boundScale;
        }
    }
}
