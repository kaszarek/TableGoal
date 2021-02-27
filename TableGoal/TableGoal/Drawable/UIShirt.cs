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
    class UIShirt : UIElement
    {
        private readonly float MAXSCALE = .9f;
        //private readonly float UP_SCALE_FACTOR = 1.009f;
        //private readonly float DOWN_SCALE_FACTOR = 0.978f;
        private readonly float UP_SCALE_FACTOR = .008f;
        private readonly float DOWN_SCALE_FACTOR = .016f;
        private bool bouncing = true;
        private bool pressed;
        private float setScale = 1.0f;
        private bool growing = true;
        private readonly float coolDownTime = 2.0f;
        private float actualCoolDown = 2.0f;
        private bool animationDone = false;
        private int animationIterator = 0;

        public bool Pressed
        {
            get { return pressed; }
            set
            {
                pressed = value;
                bouncing = !pressed;
                animationIterator = 0;
                actualCoolDown = coolDownTime;
            }
        }

        public UIShirt(string textureName, Rectangle destRect)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
        }

        public UIShirt(string textureName, Rectangle destRect, float scale)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
            setScale = scale;
            this.Scale = scale;
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
            this.Origin = new Vector2(this.ObjectTexture.Width / 2, this.ObjectTexture.Height / 2);
            float offset = (this.ObjectTexture.Height - this.ObjectTexture.Height * this.Scale) / 2;
            this.Position = new Vector2(DestinationRectangle.X - offset, DestinationRectangle.Y - offset) + this.Origin;
        }

        public override void Update(GameTime gametime)
        {
            if (bouncing) // jesli koszulka t�tni
            {
                if (animationDone) // jesli animacja zosta�a zako�czona
                {
                    actualCoolDown -= (float)gametime.ElapsedGameTime.TotalSeconds; // zmniejszamy cooldown
                    if (actualCoolDown < 0) // sprawdzamy czy nie jest mniejszy od zera
                    {
                        actualCoolDown = coolDownTime; // jak jest to resetujemy
                        animationDone = false;         // i zezwalamy na animacj�
                    }
                }
                else  // je�li jest pozwolenie na animacj� to jedziemy z koksem
                {
                    if (growing)            // jak ro�niemy
                    //{ Scale *= UP_SCALE_FACTOR; }    // to skalujemy sie do g�ry
                    { Scale += UP_SCALE_FACTOR; }    // to skalujemy sie do g�ry
                    else                    // a jak nie ro�niemy
                    //{ Scale *= DOWN_SCALE_FACTOR; }    // to skalujemy sie w d�
                    { Scale -= DOWN_SCALE_FACTOR; }    // to skalujemy sie w d�
                    if (Scale < setScale)   // jesli skala jest mniejsza od ustalonej wtedy
                    {
                        growing = true;     // ro�niemy
                        animationIterator++;// zwiekszamy ilo�� iteracji
                        if (animationIterator % 2 == 0) // sprawdzamy czy by�y juz 2 "wybicia serca"
                        {
                            animationDone = true;       // jak tak to animacja sko�czona i teraz czekamy 2 sekundy
                            animationIterator -= 2;     // �eby licznik by� blisko zera
                        }
                    }
                    if (Scale > MAXSCALE)       // maksymalna skala
                        growing = false;        // i jak osi�gnelismy maksymaln� to od teraz malejemy
                }
            }
            else
            {
                Scale = setScale;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.ObjectTexture,
                             this.Position,
                             null,
                             this.Color,
                             0f,
                             this.Origin,
                             this.Scale,
                             SpriteEffects.None,
                             0f);
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
            {
                if (!pressed)
                {
                    pressed = !pressed;
                    bouncing = !pressed; // if was it pressed then it mean it is not bouncing any more
                }
            }
        }

        public bool WasPressed(Vector2 tapPoint)
        {
            if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
            {
                return true;
            }
            return false;
        }
    }
}
