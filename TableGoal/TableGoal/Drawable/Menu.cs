using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    class Menu : DrawableGameObject
    {
        List<UIElement> menuElements;
        private ButtonType pressedButton = ButtonType.None;
        private Rectangle surfaceOfMenu;
        private int mariginBetweenButtons = 5;

        public int MariginBetweenButtons
        {
            get { return mariginBetweenButtons; }
            set { mariginBetweenButtons = value; }
        }

        public Rectangle SurfaceOfMenu
        {
            get { return surfaceOfMenu; }
            set { surfaceOfMenu = value; CalculateButtonsRectangles(); }
        }

        public ButtonType PressedButton
        {
            get { return pressedButton; }
            set 
            { 
                pressedButton = value;
                if (pressedButton == ButtonType.None)
                {
                    foreach (UIElement element in menuElements)
                    {
                        if (element is MenuButton)
                        {
                            MenuButton menuButt = element as MenuButton;
                            menuButt.Pressed = false;
                        }
                    }
                }
            }
        }

        public Menu(string backgroungTextureName, Rectangle menusplaceOnScreen)
        {
            this.TextureName = backgroungTextureName;
            this.surfaceOfMenu = menusplaceOnScreen;
            menuElements = new List<UIElement>();
        }

        public void AddElement(UIElement element)
        {
            menuElements.Add(element);
            //if (element is MenuButton)
            //    CalculateButtonsRectangles();
        }

        public void AddButton(string textureName, ButtonType type)
        {
            MenuButton button = new MenuButton(textureName, type);
            menuElements.Add(button);
            //CalculateButtonsRectangles();
        }

        public void AddButton(MenuButton button)
        {
            menuElements.Add(button);
            //CalculateButtonsRectangles();
        }

        public void RemoveButton(ButtonType type)
        {
            foreach (UIElement el in menuElements)
            {
                if (el is MenuButton)
                {
                    MenuButton button = (MenuButton)el;
                    if (button.Type == type)
                    {
                        el.Visible = false;
                        //menuElements.Remove(el);
                        CalculateButtonsRectangles();
                    }
                }
            }
        }

        public void AddButton(string textureName, ButtonType type, Color color)
        {
            MenuButton button = new MenuButton(textureName, type, color);
            menuElements.Add(button);
            //CalculateButtonsRectangles();
        }

        public void AddButton(string textureName, string context, ButtonType type)
        {
            MenuButton button = new MenuButton(textureName, context, type);
            menuElements.Add(button);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.ObjectTexture, this.DestinationRectangle, this.Color);
            foreach (UIElement element in menuElements)
            {
                if (element.Visible)
                    element.Draw(spriteBatch);
            }
        }

        public void Update(GameTime gametime)
        {
            foreach (UIElement element in menuElements)
            {
                element.Update(gametime);
            }
        }             

        public override void LoadTexture(ContentManager contentManager)
        {
            this.ObjectTexture = contentManager.Load<Texture2D>(TextureName);
            foreach (UIElement element in menuElements)
            {
                element.LoadTexture(contentManager);
            }
            CalculateButtonsRectangles();
        }

        public bool WasPressed(Vector2 touchPoint)
        {
            ButtonType typePressed = ButtonType.None;
            foreach (UIElement element in menuElements)
            {
                if (!element.Visible)
                    continue;
                if (element is MenuButton)
                {
                    MenuButton btn = (MenuButton)element;
                    typePressed = btn.WasPressed(touchPoint);
                    if (typePressed != ButtonType.None)
                    {
                        pressedButton = typePressed;
                        return true;
                    }
                }
                else
                    element.HandleInput(touchPoint);
            }
                return false;
        }

        public void CalculateButtonsRectangles()
        {
            //int buttonAmount = menuElements.Count;
            int buttonAmount = 0;
            foreach (UIElement el in menuElements)
                if (el is MenuButton || el is CheckBox)
                    if (el.Visible)
                        buttonAmount++;
            if (buttonAmount == 0)
                return;
            int eachHeight = (int)(surfaceOfMenu.Height - buttonAmount * mariginBetweenButtons) / buttonAmount;
            int i = 0;
            foreach (UIElement element in menuElements)
            {
                if (element.Visible)
                    if (element is MenuButton || element is CheckBox)
                    {
                        int x = surfaceOfMenu.X;
                        int y = i * mariginBetweenButtons + surfaceOfMenu.Y + i * eachHeight;
                        if (element is MenuButton)
                            element.DestinationRectangle = new Rectangle(x, y, element.ObjectTexture.Width, eachHeight);
                        if (element is CheckBox)
                            element.DestinationRectangle = new Rectangle(x, y, surfaceOfMenu.Width, eachHeight);
                        i++;
                    }
            }
        }
    }
}