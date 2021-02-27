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
    class Digit : UIElement
    {
		private static readonly string textureName = "Digits73x102";
		private Rectangle digitSize;
        private int number;
		private int unityFromNumber;
		private int tensFromNumber;
		private Rectangle firstDigitDestinationRectangle;
		private Rectangle firstDigitSourceRectangle;
		private Rectangle secondDigitDestinationRectangle;
		private Rectangle secondDigitSourceRectangle;
        private bool isIncrementable;
        private int maximumGoalsLimit;
        private bool pressed = false;
        private bool isZeroAllowed = true;

        public bool IsZeroAllowed
        {
            get { return isZeroAllowed; }
            set { isZeroAllowed = value; }
        }

        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }
        
        public int Number
        {
            get { return number; }
            set { number = value; SplitNumber(); }
        }

        public bool IsIncrementable
        {
            get { return isIncrementable; }
            set { isIncrementable = value; }
        }

        public Digit(Rectangle destRect, int number)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
            this.Color = Color.Black;
            this.isIncrementable = false;
            this.number = number;
            this.maximumGoalsLimit = 0;
            SplitNumber();
        }

        public Digit(Rectangle destRect, int number, int maximumGoals)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
            this.Color = Color.Black;
            this.isIncrementable = false;
            this.number = number;
            this.maximumGoalsLimit = maximumGoals;
            SplitNumber();
        }
        
        public Digit(Rectangle destRect, int number, bool canChange)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
            this.Color = Color.Black;
            this.number = number;
            this.isIncrementable = canChange;
            this.maximumGoalsLimit = 0;
            SplitNumber();
        }

        public Digit(Rectangle destRect, int number, bool canChange, int maximumGoals)
        {
            this.TextureName = textureName;
            this.DestinationRectangle = destRect;
            this.Color = Color.Black;
            this.number = number;
            this.isIncrementable = canChange;
            this.maximumGoalsLimit = maximumGoals;
            SplitNumber();
        }

        private void SplitNumber()
        {
            if (number == 0 && !isZeroAllowed)
                number++;
			if (number < 100)
			{
                unityFromNumber = number % 10;
				tensFromNumber = number / 10;
			}
			else
			{
				unityFromNumber = 9;
				tensFromNumber = 9;
			}
			SetRectangles();
		}
		
		private void SetRectangles()
        {
            int halfWidth = this.DestinationRectangle.Width / 2;
			if (tensFromNumber > 0)
			{
				firstDigitDestinationRectangle = new Rectangle(this.DestinationRectangle.X,
															   this.DestinationRectangle.Y,
															   halfWidth,
															   this.DestinationRectangle.Height);
				firstDigitSourceRectangle = new Rectangle(tensFromNumber * digitSize.Width,
                                                          0,
                                                          digitSize.Width,
                                                          digitSize.Height);
                secondDigitDestinationRectangle = new Rectangle(this.DestinationRectangle.X + halfWidth,
															    this.DestinationRectangle.Y,
															    halfWidth,
															    this.DestinationRectangle.Height);
				secondDigitSourceRectangle = new Rectangle(unityFromNumber * digitSize.Width,
                                                           0,
                                                           digitSize.Width,
                                                           digitSize.Height);
			}
			else
			{
                firstDigitDestinationRectangle = new Rectangle(this.DestinationRectangle.X + halfWidth,
                                                               this.DestinationRectangle.Y,
                                                               halfWidth,
                                                               this.DestinationRectangle.Height);
				firstDigitSourceRectangle = new Rectangle(unityFromNumber * digitSize.Width, 0, digitSize.Width, digitSize.Height);
			}
		}

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
            digitSize = new Rectangle(0, 0, this.ObjectTexture.Width / 10, this.ObjectTexture.Height);
            SetRectangles();
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
			if (number < 10)
				spriteBatch.Draw(this.ObjectTexture,
								 this.firstDigitDestinationRectangle,
								 this.firstDigitSourceRectangle,
								 this.Color);
            else
            {
				spriteBatch.Draw(this.ObjectTexture,
								 this.firstDigitDestinationRectangle,
								 this.firstDigitSourceRectangle,
								 this.Color);
				spriteBatch.Draw(this.ObjectTexture,
								 this.secondDigitDestinationRectangle,
								 this.secondDigitSourceRectangle,
								 this.Color);
			}
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
            {
				if (isIncrementable)
				{
					this.number++;
                    if (maximumGoalsLimit > 0)
                        this.number %= maximumGoalsLimit + 1;
					SplitNumber();
				}
                pressed = true;
            }
        }

        public void ProgramicClickOnDigit(bool withPressed)
        {
            if (isIncrementable)
            {
                this.number++;
                if (maximumGoalsLimit > 0)
                    this.number %= maximumGoalsLimit + 1;

                SplitNumber();
            }
            if (withPressed)
                pressed = true;
        }
    }
}
