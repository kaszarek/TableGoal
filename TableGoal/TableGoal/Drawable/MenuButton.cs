using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    public enum ButtonType
    {
        NewGame,
        ResumeGame,
        OnePlayerGame,
        TwoPlayerGame,
        MultiLocal,
        MultiWifiHost,
        MultiWifiJoin,
        MultiGlobalHost,
        MultiGlobalJoin,
        ShowMainMenu,
        Options,
        Exit,
        Back,
        BuyFullVersion,
        Retry,
        HowToPlay,
        Controlls,
        WorldCup,
        WCNewGame,
        WCContinue,
        WCNextMatch,
        WCShowTable,
        None
    }

    class MenuButton : UIElement
    {
        private string buttonText;
        private bool pressed;

        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }
        private ButtonType type;

        public ButtonType Type
        {
            get { return type; }
            set { type = value; }
        }

        public string ButtonText
        {
            get { return buttonText; }
            set { buttonText = value; }
        }

        public MenuButton(string textureName, ButtonType type)
        {
            this.TextureName = textureName;
            this.type = type;
            this.Color = Color.Black;
        }

        public MenuButton(string textureName, ButtonType type, Color color)
        {
            this.TextureName = textureName;
            this.type = type;
            this.Color = color;
        }

        public MenuButton(string textureName, string context, ButtonType type)
        {
            this.TextureName = textureName;
            this.buttonText = context;
            this.type = type;
            this.Color = Color.Black;
        }

        public MenuButton(string textureName, string context, Rectangle targetRectangle, ButtonType type)
        { }

        public MenuButton(string textureName, string context, Vector2 position, Version size, ButtonType type)
        { }
        
        public ButtonType WasPressed(Vector2 touchPoint)
        {
            if (DestinationRectangle.Contains(new Point((int)touchPoint.X, (int)touchPoint.Y)))
            {
                pressed = true;
                return this.type;
            }
            return ButtonType.None;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!pressed)
            {
                spriteBatch.Draw(this.ObjectTexture,
                                 this.DestinationRectangle,
                                 this.Color);
            }
            else
            {
                spriteBatch.Draw(this.ObjectTexture,
                                 this.DestinationRectangle,
                                 Color.Gray);
                //pressed = false;
            }
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            base.LoadTexture(contentManager);
        }
    }
}
