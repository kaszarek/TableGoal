using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    public static class GameRules
    {
        public static TypeOfMove DetermineTypeOfMoveFromDelta(Vector2 delta)
        {
            double length = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
            Vector2 direction = Vector2.Zero;

            double Pi6 = Math.PI / 6;
            double Pi12 = Math.PI / 12;
            double angle = Math.Asin(delta.Y / length);
            int checkDistance = 5;
            if (delta.X < -checkDistance && (angle < Pi12 && angle > -Pi12))  // W
            {
                direction.X = -1;
                direction.Y = 0;
            }
            if (delta.X > checkDistance && (angle < Pi12 && angle > -Pi12))  //E
            {
                direction.X = 1;
                direction.Y = 0;
            }
            if (delta.Y < -checkDistance && (angle < (-Math.PI / 2 + Pi12)))  // N
            {
                direction.X = 0;
                direction.Y = 1;
            }
            if (delta.Y > checkDistance && (angle > (Math.PI / 2 - Pi12)))  // S
            {
                direction.X = 0;
                direction.Y = -1;
            }
            if (delta.Y > checkDistance && (angle > Pi6 - Pi12 && angle < (Math.PI / 2 - Pi6 - Pi12)) && delta.X < 0)  // SW
            {
                direction.X = -1;
                direction.Y = -1;
            }
            if (delta.Y > checkDistance && (angle > Pi6 - Pi12 && angle < (Math.PI / 2 - Pi6 - Pi12)) && delta.X > 0)  // SE
            {
                direction.X = 1;
                direction.Y = -1;
            }
            if (delta.Y < -checkDistance && (angle < -Pi6 + Pi12 && angle > (-Math.PI / 2 + Pi6 - Pi12)) && delta.X < 0)  // NW
            {
                direction.X = -1;
                direction.Y = 1;
            }
            if (delta.Y < -checkDistance && (angle < -Pi6 + Pi12 && angle > (-Math.PI / 2 + Pi6 - Pi12)) && delta.X > 0)  // NE
            {
                direction.X = 1;
                direction.Y = 1;
            }

            delta.X = direction.X;
            delta.Y = direction.Y;
            if (delta.X == 0 && delta.Y == 1)
                return TypeOfMove.N;
            if (delta.X == 0 && delta.Y == -1)
                return TypeOfMove.S;
            if (delta.X == 1 && delta.Y == 0)
                return TypeOfMove.E;
            if (delta.X == -1 && delta.Y == 0)
                return TypeOfMove.W;
            if (delta.X == 1 && delta.Y == 1)
                return TypeOfMove.NE;
            if (delta.X == 1 && delta.Y == -1)
                return TypeOfMove.SE;
            if (delta.X == -1 && delta.Y == 1)
                return TypeOfMove.NW;
            if (delta.X == -1 && delta.Y == -1)
                return TypeOfMove.SW;
            return TypeOfMove.UNKNOWN;
        }
    }
}
