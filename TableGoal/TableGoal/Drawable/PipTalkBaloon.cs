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
    class PipTalkBaloon : UIElement
    {
        private bool pressed;

        Vector2 v_baloonPosition;
        Vector2 v_stringMeasure;
        /// <summary>
        /// Fragment baloon z lewej strony
        /// </summary>
        Rectangle r_baloonSourceLeftSide;
        /// <summary>
        /// Fragment baloon z prawej strony
        /// </summary>
        Rectangle r_baloonSourceRightSide;
        /// <summary>
        /// Tekst do baloona
        /// </summary>
        string s_pipTalkText;
        /// <summary>
        /// Pozycja tekstu
        /// </summary>
        Vector2 v_pipTalkTextPosition;
        public SpriteFont pipTalkFont;
        Rectangle r_hitbox;

        public Rectangle R_hitbox
        {
            get { return r_hitbox; }
            set { r_hitbox = value; }
        }
        CoachPipTalkCodes _pipTalkCode;

        public CoachPipTalkCodes PipTalkCode
        {
            get { return _pipTalkCode; }
            set { _pipTalkCode = value; }
        }


        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }

        float f_scale = 1;

        public PipTalkBaloon(Vector2 destVect, Color color, string text, CoachPipTalkCodes code)
        {
            _pipTalkCode = code;
            this.TextureName = "pipTalk";
            v_baloonPosition = destVect;
            v_pipTalkTextPosition = new Vector2(destVect.X + 8, destVect.Y);
            r_baloonSourceLeftSide = new Rectangle(0, 0, 34, 46);
            r_baloonSourceRightSide = new Rectangle(584, 0, 16, 46);
            r_hitbox = new Rectangle((int)destVect.X, (int)destVect.Y, r_baloonSourceLeftSide.Width + r_baloonSourceRightSide.Width, r_baloonSourceRightSide.Height);
            s_pipTalkText = text;
            this.Color = color;
            Active = true;
        }

        public PipTalkBaloon(Vector2 destVect, Color color, string text, CoachPipTalkCodes code, float baloonHeight)
        {
            f_scale = baloonHeight / 46;
            _pipTalkCode = code;
            this.TextureName = "pipTalk";
            v_baloonPosition = destVect;
            v_pipTalkTextPosition = new Vector2(destVect.X + 8, destVect.Y);
            r_baloonSourceLeftSide = new Rectangle(0, 0, 34, 46);
            r_baloonSourceRightSide = new Rectangle(584, 0, 16, 46);
            r_hitbox = new Rectangle((int)destVect.X, (int)destVect.Y, r_baloonSourceLeftSide.Width + r_baloonSourceRightSide.Width, (int)baloonHeight);
            s_pipTalkText = text;
            this.Color = color;
            Active = true;
        }

        public void AdjustPosition(int xDisplacement, int yDisplacement)
        {
            v_baloonPosition = new Vector2(v_baloonPosition.X + xDisplacement, v_baloonPosition.Y + yDisplacement);
            v_pipTalkTextPosition = new Vector2(v_baloonPosition.X + 8, v_baloonPosition.Y);
            int scaledWidth = (int)(f_scale * r_baloonSourceLeftSide.Width + r_baloonSourceRightSide.Width);
            int scaledHeight = (int)(f_scale * 46);
            r_hitbox = new Rectangle((int)v_baloonPosition.X, (int)v_baloonPosition.Y, scaledWidth, scaledHeight);            
        }

        public override void LoadTexture(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            pipTalkFont = contentManager.Load<SpriteFont>("Fonts/TRIAL_font");
            v_stringMeasure = pipTalkFont.MeasureString(s_pipTalkText);
            r_baloonSourceLeftSide = new Rectangle(0, 0, (int)v_stringMeasure.X, 46);
            AdjustPosition(0, 0);
            base.LoadTexture(contentManager);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                Color = Color.Black;
            }
            else
            {
                Color = Color.Gray;
            }
            if (Visible)
            {
                if (f_scale == 1)
                {
                    spriteBatch.Draw(ObjectTexture, v_baloonPosition, r_baloonSourceLeftSide, Color);
                    spriteBatch.Draw(ObjectTexture, new Vector2(v_baloonPosition.X + r_baloonSourceLeftSide.Width, v_baloonPosition.Y), r_baloonSourceRightSide, Color);
                    spriteBatch.DrawString(pipTalkFont, s_pipTalkText, v_pipTalkTextPosition, Color);
                }
                else
                {
                    spriteBatch.Draw(ObjectTexture, v_baloonPosition, r_baloonSourceLeftSide, Color, .0f, new Vector2(0, 0), f_scale, SpriteEffects.None, 1);
                    spriteBatch.Draw(ObjectTexture, new Vector2(v_baloonPosition.X + r_baloonSourceLeftSide.Width * f_scale, v_baloonPosition.Y), r_baloonSourceRightSide, Color, .0f, new Vector2(0, 0), f_scale, SpriteEffects.None, 1);
                    spriteBatch.DrawString(pipTalkFont, s_pipTalkText, v_pipTalkTextPosition, Color, .0f, new Vector2(0, 0), f_scale, SpriteEffects.None, 1);
                }
            }
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (Visible && Active)
            {
                if (r_hitbox.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    pressed = !pressed;
                }
            }
        }

        public bool WasPressed(Vector2 tapPoint)
        {
            if (Visible && Active)
            {
                if (r_hitbox.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    AudioManager.PlaySound("selected");
                    return true;
                }
            }
            return false;
        }
    }
}
