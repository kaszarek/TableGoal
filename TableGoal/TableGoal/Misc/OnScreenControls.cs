using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    class OnScreenControls:DrawableGameObject
    {
        List<ControllerButton> controllerMoves;
        List<TypeOfMove> availableMoves;
        Team owner;

        public Team Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public void SetRefToAvailableMoves(List<TypeOfMove> moves)
        {
            availableMoves = moves;
        }

        public OnScreenControls()
        {
            availableMoves = new List<TypeOfMove>();
            controllerMoves = new List<ControllerButton>();
            ControllerButton left = new ControllerButton("ControllerMoves", new Rectangle(160, 160, 160, 160), new Rectangle(0, 0, 160, 160), TypeOfMove.W);
            controllerMoves.Add(left);
            ControllerButton right = new ControllerButton("ControllerMoves", new Rectangle(480, 160, 160, 160), new Rectangle(160, 0, 160, 160), TypeOfMove.E);
            controllerMoves.Add(right);
            ControllerButton upLeft = new ControllerButton("ControllerMoves", new Rectangle(160, 0, 160, 160), new Rectangle(320, 0, 160, 160), TypeOfMove.NW);
            controllerMoves.Add(upLeft);
            ControllerButton downRight = new ControllerButton("ControllerMoves", new Rectangle(480, 320, 160, 160), new Rectangle(480, 0, 160, 160), TypeOfMove.SE);
            controllerMoves.Add(downRight);
            ControllerButton upRight = new ControllerButton("ControllerMoves", new Rectangle(480, 0, 160, 160), new Rectangle(640, 0, 160, 160), TypeOfMove.NE);
            controllerMoves.Add(upRight);
            ControllerButton downLeft = new ControllerButton("ControllerMoves", new Rectangle(160, 320, 160, 160), new Rectangle(800, 0, 160, 160), TypeOfMove.SW);
            controllerMoves.Add(downLeft);
            ControllerButton up = new ControllerButton("ControllerMoves", new Rectangle(320, 0, 160, 160), new Rectangle(960, 0, 160, 160), TypeOfMove.N);
            controllerMoves.Add(up);
            ControllerButton down = new ControllerButton("ControllerMoves", new Rectangle(320, 320, 160, 160), new Rectangle(1120, 0, 160, 160), TypeOfMove.S);
            controllerMoves.Add(down);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (ControllerButton btn in controllerMoves)
                if (availableMoves.Contains(btn.Type))
                    spriteBatch.Draw(btn.ObjectTexture, btn.DestinationRectangle, btn.SourceRectangle, owner.ShirtsColor);
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            controllerMoves[0].LoadTexture(contentManager);
            for (int i = 1; i < controllerMoves.Count; i++)
                controllerMoves[i].ObjectTexture = controllerMoves[0].ObjectTexture;
        }

        public TypeOfMove HandleInput(Vector2 tapPoint)
        {
            foreach (ControllerButton btn in controllerMoves)
            {
                if (availableMoves.Contains(btn.Type))
                    if (btn.DestinationRectangle.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                        return btn.Type;
            }
            return TypeOfMove.UNKNOWN;
        }

    }
}
