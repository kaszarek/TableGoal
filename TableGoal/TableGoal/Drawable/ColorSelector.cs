using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;

namespace TableGoal
{
    public enum ColorSelOrientation
    {
        HORIZONTAL,
        VERTICAL
    }

    class ColorSelector : UIElement
    {
        List<UIElement> colorElements;
        Color selectedColor;
        private Color[] availableColors = { Color.Red,
                                            Color.Blue,
                                            Color.Green,
                                            Color.Fuchsia,
                                            Color.Gray,
                                            Color.DarkRed,
                                            Color.Orange,
                                            Color.DarkViolet,
                                            Color.Lime,
                                            Color.CornflowerBlue };
        Color forbiddenColor;
        UIPicture notAvailableColorMask;
        UIPicture pickedColorMask;
        ColorSelOrientation orientation;
        int bok = 60;
        int leftMarigin = 0;
        int upMarigin = 0;
        int horizontalMarigin = 10;
        int verticalMarigin = 10;
        internal static int selecterColorIndex = 0;

        public Color ForbiddenColor
        {
            get { return forbiddenColor; }
            set { forbiddenColor = value; SetMaskToNotAvailableColor(); }
        }

        public Color SelectedColor
        {
            get { return selectedColor; }
            set { selectedColor = value; SetMaskToPickedColor(); }
        }

        public ColorSelector(Rectangle screenRect, ColorSelOrientation onScreenOrientation)
        {
            colorElements = new List<UIElement>();
            DestinationRectangle = screenRect;
            orientation = onScreenOrientation;
            //int bok = 60;
            //int leftMarigin = 0;
            //int upMarigin = 0;
            //int horizontalMarigin = 10;
            //int verticalMarigin = horizontalMarigin;

            int j = -1;
            if (orientation == ColorSelOrientation.VERTICAL)
                for (int i = 0; i < availableColors.Length; i++)
                {
                    if (i % 2 == 0)
                        j++;
                    UIPicture btn = new UIPicture("empty4x4", new Rectangle(screenRect.X + leftMarigin + (i % 2) * (horizontalMarigin + bok),
                                                                            screenRect.Y + upMarigin + j * (verticalMarigin + bok), bok, bok));
                    btn.Color = availableColors[i];
                    colorElements.Add(btn);
                }
            else
            {
                for (int i = 0; i < availableColors.Length; i++)
                {
                    if (i % 2 == 0)
                        j++;
                    UIPicture btn = new UIPicture("empty4x4", new Rectangle(screenRect.X + leftMarigin + j * (horizontalMarigin + bok),
                                                                            screenRect.Y + upMarigin + (i % 2)  * (verticalMarigin + bok), bok, bok));
                    btn.Color = availableColors[i];
                    colorElements.Add(btn);
                }
            }

            selectedColor = colorElements[selecterColorIndex].Color;
            selecterColorIndex = (++selecterColorIndex) % 8;
            notAvailableColorMask = new UIPicture("notActiveMask", new Rectangle());
            notAvailableColorMask.Color = Color.Black;
            pickedColorMask = new UIPicture("pickedColorMask", new Rectangle());
            pickedColorMask.Color = Color.Black;
            SetMaskToPickedColor();
        }

        private void SetMaskToNotAvailableColor()
        {
            if (selectedColor == forbiddenColor)
            {
                selecterColorIndex = (++selecterColorIndex) % 8;
                selectedColor = colorElements[selecterColorIndex].Color;
                SetMaskToPickedColor();
            }
            foreach (UIElement el in colorElements)
            {
                if (el.Color == forbiddenColor)
                    notAvailableColorMask.DestinationRectangle = el.DestinationRectangle;
            }
        }

        private void SetMaskToPickedColor()
        {
            foreach (UIElement el in colorElements)
            {
                if (el.Color == selectedColor)
                    pickedColorMask.DestinationRectangle = el.DestinationRectangle;
            }
        }

        public Color GiveNextFreeColor()
        {
            return availableColors[(selecterColorIndex + 1) % 8];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (UIElement el in colorElements)
                el.Draw(spriteBatch);
            notAvailableColorMask.Draw(spriteBatch);
            pickedColorMask.Draw(spriteBatch);
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            foreach (UIElement el in colorElements)
                el.LoadTexture(contentManager);
            notAvailableColorMask.LoadTexture(contentManager);
            pickedColorMask.LoadTexture(contentManager);
            //SetMaskToSelectedColor();
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            Point point = new Point((int)tapPoint.X, (int)tapPoint.Y);
            foreach (UIElement el in colorElements)
            {
                if (el.DestinationRectangle.Contains(point))
                {
                    if (el.Color != forbiddenColor)
                    {
                        selectedColor = el.Color;
                        AudioManager.PlaySound("selected");
                    }
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (availableColors[i] == selectedColor)
                    selecterColorIndex = i;
            }
            SetMaskToPickedColor();
            //SetMaskToSelectedColor();
        }
    }
}
