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
        /// Tworzy podskakuj�cy obiekt z domy�lnymi warto�ciami dla animacji.
        /// </summary>
        /// <param name="textureName">Nazwa tekstury</param>
        /// <param name="destinationRect">Prostok�t, w kt�rym zostanie wy�wietlona tekstura.</param>
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
        ///// Tworzy podskakuj�cy obiekt ze zdefiniowamyni parametrami.
        ///// </summary>
        ///// <param name="textureName">Nazwa tekstury</param>
        ///// <param name="destinationRect">Prostok�t, w kt�rym zostanie wy�wietlona tekstura.</param>
        ///// <param name="growingTime">Czas przez, kt�ry obiekt b�dzie si� powi�ksza�.</param>
        ///// <param name="shrinkingTime">Czas przez, kt�ry obiekt b�dzie si� pomniejsza�.</param>
        ///// <param name="intervalTime">Czas przez, kt�ry obiekt pozostanie niezmieniony po powi�kszeniu i pomniejszeniu.</param>
        ///// <param name="maximumScale">Maksymalna skala do jakiej zostanie powi�kszony obrazek. Jesli jest ona za ma�a to zostanie zmieniona na 10% wi�ksz� od skali pocz�tkowej obrazka.</param>
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
        /// Tworzy podskakuj�cy obiekt ze zdefiniowanymi parametrami.
        /// </summary>
        /// <param name="textureName">Nazwa tekstury.</param>
        /// <param name="destinationRect">Prostok�c, w kt�rym b�dzie wy�wietlona tekstura.</param>
        /// <param name="growingTime">Czas przez, kt�ry obiekt b�dzie si� powi�ksza�.</param>
        /// <param name="shrinkingTime">Czas przez, kt�ry obiekt b�dzie si� pomniejsza�.</param>
        /// <param name="intervalTime">Czas przez, kt�ry obiekt pozostanie niezmieniony po powi�kszeniu i pomniejszeniu.</param>
        /// <param name="growingPercentage">Procentowy wzrost obiektu z jego stanu podstawowego. Je�li mniejsze od zera wtedy zostanie zmienione na liczb� dodatni�. Jesli zero wtedy = 0.1 .</param>
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
            this.Origin = new Vector2(this.ObjectTexture.Width / 2, this.ObjectTexture.Height / 2);    // ustalamy �rodek realnej tekstury
            this.Scale = (float)DestinationRectangle.Width / this.ObjectTexture.Width;                 // ustala skal� pocz�tkow�
            this.initialScale = Scale;                                                                 // przypisuje skal� pocz�twkow�
            maximumScale = initialScale * (1.0f + growingPercentage);                                  // liczy maksymaln� skal�
            offset = (this.ObjectTexture.Height - this.ObjectTexture.Height * this.Scale) / 2;   // oblicza offset, czyli ile tekstura wpisana w prostok�t jest przesuni�ta
            this.Position = new Vector2(DestinationRectangle.X - offset, DestinationRectangle.Y - offset) + this.Origin; // oblicza pozycj� dla tekstury wpisanej w prostok�t (czyli o konkternym po�o�eniu i wymiarach)
            this.scaleInterval_GROWING = Math.Abs(Scale - maximumScale) / this.growingTime / 30.0f;     // oblicza tempo wzrostu      30 to ile klatek na sekund� jest wy�wietlanych.
            this.scaleInterval_SHRINKING = Math.Abs(Scale - maximumScale) / this.shrinkingTime / 30.0f; // oblicza tempo zmniejszania
        }

        public override void Update(GameTime gametime)
        {
            if (Visible)
                if (bouncing) // jesli animacja jest w�aczona
                {
                    if (Scale > maximumScale)                   // jesli skala wi�ksza niz maksymalna
                        growing = false;                        // to nie ro�niemy
                    if (Scale <= initialScale)                   // je�li skala mniejsza niz minimalna
                    {
                        if (interval_counter <= 0)              // je�li licznik bezruchu (gry nie ma animacji) jest <= zero
                        {
                            growing = true;                     // zaczynamy rosn��
                            interval_counter = intervalTime;    // i zerujemy licznik bezruchu
                        }
                        else                                    // je�li licznik jest > od zera
                        {
                            this.Scale = initialScale;          // to ustawiamy skal� na pocz�tkow�
                            interval_counter -= (float)gametime.ElapsedGameTime.TotalSeconds; // zmniejszami licznik o czas, kt�ry min��
                            return;                             // i ko�czymy animacj�
                        }
                    }
                    if (growing)                                // je�li rosniemy
                    {
                        this.Scale += scaleInterval_GROWING;    // to zwi�kszamy skal� o przyrost
                    }
                    else                                        // je�li malejemy
                    {
                        this.Scale -= scaleInterval_SHRINKING;  // to zmniejszamy skal�
                    }
                }
                else                                            // je�li animacja jest wy��czona
                {
                    this.Scale = initialScale;                  // to ustawiamy skal� na pocz�tkow�
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
        /// Sprawdza czy obiekt zosta� klikni�ty.
        /// </summary>
        /// <param name="tapPoint">Punkt, w kt�ry nast�pi�o tapni�cie.</param>
        /// <returns>Prawda je�li obiekt zosta� tapni�ty, lub fa�sz w przeciwnym wypadku.</returns>
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
