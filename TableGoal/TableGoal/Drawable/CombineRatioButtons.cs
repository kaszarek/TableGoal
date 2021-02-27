using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;

namespace TableGoal
{
    class CombineRatioButtons : UIElement
    {
        public enum Mode
        {
            Horizontal,
            Vertical
        }
        private RadioButton selectionOne;
        private RadioButton selectionTwo;
        private Controlling controls;
        private Mode mode;

        public Controlling Controls
        {
            get { return controls; }
            set
            {
                controls = value;
                ControllingSet();
            }
        }

        public CombineRatioButtons(string selectionOneTextureName, string selectionTwoTextureName, Rectangle destRect, Mode orientation)
        {
            selectionOne = new RadioButton(selectionOneTextureName);
            selectionOne.Color = Color.Black;
            selectionOne.Select();
            controls = Controlling.BUTTONS;
            selectionTwo = new RadioButton(selectionTwoTextureName);
            selectionTwo.Color = Color.Black;
            DestinationRectangle = destRect;
            mode = orientation;
            RecalculateRetangles();
        }

        public void SetSelectionColor(Color color)
        {
            selectionOne.MarkingColor = color;
            selectionTwo.MarkingColor = color;
        }

        private void RecalculateRetangles()
        {
            switch (mode)
            {
                case Mode.Horizontal:
                    selectionOne.DestinationRectangle = new Rectangle(DestinationRectangle.X, DestinationRectangle.Y,
                                                                      DestinationRectangle.Width / 2, DestinationRectangle.Height);
                    selectionOne.DestinationRectChanged();
                    selectionTwo.DestinationRectangle = new Rectangle(DestinationRectangle.X + DestinationRectangle.Width / 2, DestinationRectangle.Y,
                                                                      DestinationRectangle.Width / 2, DestinationRectangle.Height);
                    selectionTwo.DestinationRectChanged();
                    break;
                case Mode.Vertical:
                    selectionOne.DestinationRectangle = new Rectangle(DestinationRectangle.X, DestinationRectangle.Y,
                                                                      DestinationRectangle.Width, DestinationRectangle.Height / 2);
                    selectionOne.DestinationRectChanged();
                    selectionTwo.DestinationRectangle = new Rectangle(DestinationRectangle.X, DestinationRectangle.Y + DestinationRectangle.Height / 2,
                                                                      DestinationRectangle.Width, DestinationRectangle.Height / 2);
                    selectionTwo.DestinationRectChanged();
                    break;
                default:
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                selectionOne.Draw(spriteBatch);
                selectionTwo.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gametime)
        {
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            selectionOne.LoadTexture(contentManager);
            selectionTwo.LoadTexture(contentManager);
        }

        private void ControllingSet()
        {
            if (Controls == Controlling.BUTTONS)
            {
                selectionOne.Select();
                selectionTwo.Selected = false;
            }
            else
            {
                selectionOne.Selected = false; 
                selectionTwo.Select();
            }
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (Visible)
            {
                selectionOne.HandleInput(tapPoint);
                if (selectionOne.Selected)
                {
                    Controls = Controlling.BUTTONS;
                    selectionTwo.Selected = false;
                }
                selectionTwo.HandleInput(tapPoint);
                if (selectionTwo.Selected)
                {
                    Controls = Controlling.GESTURES;
                    selectionOne.Selected = false;
                }
            }
        }
    }
}
