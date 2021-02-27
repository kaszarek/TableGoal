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
    class UIJumpingUIPicture : UIElement
    {
        private float growingTime;
        private float shrinkingTime;
        private float maximumScale;
        private float intervalTime;
        private float scaleInterval_GROWING;
        private float scaleInterval_SHRINKING;
        private bool bouncing = true;
        private float initialScale;
        private bool growing = true;
        private bool pressed;
        private float growingPercentage;
        private float interval_counter;
        private float offset;

        public float Offset
        {
            get { return offset; }
            internal set { offset = value; }
        }

        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }
        
        /// <summary>
        /// Tworzy podskakuj¹cy obiekt z domyœlnymi wartoœciami dla animacji.
        /// </summary>
        /// <param name="textureName">Nazwa tekstury</param>
        /// <param name="destinationRect">Prostok¹t, w którym zostanie wyœwietlona tekstura.</param>
        public UIJumpingUIPicture(string textureName, Rectangle destinationRect)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destinationRect;
            growingTime = 1.0f;
            shrinkingTime = 1.0f;
            intervalTime = .3f;
            maximumScale = .0f;
            this.scaleInterval_GROWING = maximumScale / growingTime;
            this.scaleInterval_SHRINKING = maximumScale / shrinkingTime;

        }
        ///// <summary>
        ///// Tworzy podskakuj¹cy obiekt ze zdefiniowamyni parametrami.
        ///// </summary>
        ///// <param name="textureName">Nazwa tekstury</param>
        ///// <param name="destinationRect">Prostok¹t, w którym zostanie wyœwietlona tekstura.</param>
        ///// <param name="growingTime">Czas przez, który obiekt bêdzie siê powiêksza³.</param>
        ///// <param name="shrinkingTime">Czas przez, który obiekt bêdzie siê pomniejsza³.</param>
        ///// <param name="intervalTime">Czas przez, który obiekt pozostanie niezmieniony po powiêkszeniu i pomniejszeniu.</param>
        ///// <param name="maximumScale">Maksymalna skala do jakiej zostanie powiêkszony obrazek. Jesli jest ona za ma³a to zostanie zmieniona na 10% wiêksz¹ od skali pocz¹tkowej obrazka.</param>
        //public UIJumpingUIPicture(string textureName, Rectangle destinationRect, float growingTime, float shrinkingTime,
        //                           float intervalTime, float maximumScale)
        //{
        //    this.TextureName = textureName;
        //    this.DestinationRectangle = destinationRect;
        //    this.growingTime = growingTime;
        //    this.shrinkingTime = shrinkingTime;
        //    this.intervalTime = intervalTime;
        //    this.maximumScale = maximumScale;
        //    this.scaleInterval_GROWING = maximumScale / growingTime;
        //    this.scaleInterval_SHRINKING = maximumScale / shrinkingTime;
        //    this.growingPercentage = .0f;
        //}

        /// <summary>
        /// Tworzy podskakuj¹cy obiekt ze zdefiniowanymi parametrami.
        /// </summary>
        /// <param name="textureName">Nazwa tekstury.</param>
        /// <param name="destinationRect">Prostok¹c, w którym bêdzie wyœwietlona tekstura.</param>
        /// <param name="growingTime">Czas przez, który obiekt bêdzie siê powiêksza³.</param>
        /// <param name="shrinkingTime">Czas przez, który obiekt bêdzie siê pomniejsza³.</param>
        /// <param name="intervalTime">Czas przez, który obiekt pozostanie niezmieniony po powiêkszeniu i pomniejszeniu.</param>
        /// <param name="growingPercentage">Procentowy wzrost obiektu z jego stanu podstawowego. Jeœli mniejsze od zera wtedy zostanie zmienione na liczbê dodatniê. Jesli zero wtedy = 0.1 .</param>
        public UIJumpingUIPicture(string textureName, Rectangle destinationRect, float growingTime, float shrinkingTime,
                                   float intervalTime, float growingPercentage)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destinationRect;
            this.growingTime = growingTime;
            this.shrinkingTime = shrinkingTime;
            this.intervalTime = intervalTime;
            this.interval_counter = intervalTime;
            this.maximumScale = .0f;
            if (growingPercentage < 0)
                growingPercentage *= -1;
            if (growingPercentage == 0)
                growingPercentage = 0.1f;
            this.growingPercentage = growingPercentage;
            this.scaleInterval_GROWING = maximumScale / growingTime;
            this.scaleInterval_SHRINKING = maximumScale / shrinkingTime;
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
            this.Origin = new Vector2(this.ObjectTexture.Width / 2, this.ObjectTexture.Height / 2);    // ustalamy œrodek realnej tekstury
            this.Scale = (float)DestinationRectangle.Width / this.ObjectTexture.Width;                 // ustala skalê pocz¹tkow¹
            this.initialScale = Scale;                                                                 // przypisuje skalê pocz¹twkow¹
            maximumScale = initialScale * (1.0f + growingPercentage);                                  // liczy maksymaln¹ skalê
            offset = (this.ObjectTexture.Height - this.ObjectTexture.Height * this.Scale) / 2;   // oblicza offset, czyli ile tekstura wpisana w prostok¹t jest przesuniêta
            this.Position = new Vector2(DestinationRectangle.X - offset, DestinationRectangle.Y - offset) + this.Origin; // oblicza pozycjê dla tekstury wpisanej w prostok¹t (czyli o konkternym po³o¿eniu i wymiarach)
            this.scaleInterval_GROWING = Math.Abs(Scale - maximumScale) / this.growingTime / 30.0f;     // oblicza tempo wzrostu      30 to ile klatek na sekundê jest wyœwietlanych.
            this.scaleInterval_SHRINKING = Math.Abs(Scale - maximumScale) / this.shrinkingTime / 30.0f; // oblicza tempo zmniejszania
        }

        public override void Update(GameTime gametime)
        {
            if (Visible)
                if (bouncing) // jesli animacja jest w³aczona
                {
                    if (Scale > maximumScale)                   // jesli skala wiêksza niz maksymalna
                        growing = false;                        // to nie roœniemy
                    if (Scale <= initialScale)                   // jeœli skala mniejsza niz minimalna
                    {
                        if (interval_counter <= 0)              // jeœli licznik bezruchu (gry nie ma animacji) jest <= zero
                        {
                            growing = true;                     // zaczynamy rosn¹æ
                            interval_counter = intervalTime;    // i zerujemy licznik bezruchu
                        }
                        else                                    // jeœli licznik jest > od zera
                        {
                            this.Scale = initialScale;          // to ustawiamy skalê na pocz¹tkow¹
                            interval_counter -= (float)gametime.ElapsedGameTime.TotalSeconds; // zmniejszami licznik o czas, który min¹³
                            return;                             // i koñczymy animacjê
                        }
                    }
                    if (growing)                                // jeœli rosniemy
                    {
                        this.Scale += scaleInterval_GROWING;    // to zwiêkszamy skalê o przyrost
                    }
                    else                                        // jeœli malejemy
                    {
                        this.Scale -= scaleInterval_SHRINKING;  // to zmniejszamy skalê
                    }
                }
                else                                            // jeœli animacja jest wy³¹czona
                {
                    this.Scale = initialScale;                  // to ustawiamy skalê na pocz¹tkow¹
                }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
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
            if (Visible)
                if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    if (!this.pressed)
                    {
                        pressed = !pressed;
                        //bouncing = !Pressed; // if was it pressed then it mean it is not bouncing any more
                    }
                }
        }
        /// <summary>
        /// Sprawdza czy obiekt zosta³ klikniêty.
        /// </summary>
        /// <param name="tapPoint">Punkt, w który nast¹pi³o tapniêcie.</param>
        /// <returns>Prawda jeœli obiekt zosta³ tapniêty, lub fa³sz w przeciwnym wypadku.</returns>
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
