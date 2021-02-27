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
    public static class Translator
    {
        public static float TranslateDiractionToAngle(TypeOfMove move)
        {
            switch (move)
            {
                case TypeOfMove.N:
                    return (float)(-90.0f * Math.PI / 180);
                case TypeOfMove.NE:
                    return (float)(-45.0f * Math.PI / 180);
                case TypeOfMove.E:
                    return 0.0f;
                case TypeOfMove.SE:
                    return (float)(45.0f * Math.PI / 180);
                case TypeOfMove.S:
                    return (float)(90.0f * Math.PI / 180);
                case TypeOfMove.SW:
                    return (float)(135.0f * Math.PI / 180);
                case TypeOfMove.W:
                    return (float)(180.0f * Math.PI / 180);
                case TypeOfMove.NW:
                    return (float)(-135.0f * Math.PI / 180);
            }
            return 0.0f;
        }

        public static Vector2 TranslateDirectionToOffset(TypeOfMove move)
        {
            switch (move)
            {
                case TypeOfMove.N:
                    return new Vector2(-0.0625f, 0);// offset ¿eby kreska by³a równo - przesuniêcie w lewo
                case TypeOfMove.NE:
                    return new Vector2(-1f, 0);
                case TypeOfMove.E:
                    return new Vector2(-1f, -1f + 0.0625f); // offset ¿eby kreska by³a równo - przesuniêcie ku górze
                case TypeOfMove.SE:
                    return new Vector2(-1f, -1f);
                case TypeOfMove.S:
                    return new Vector2(-0.0625f, -1f);// offset ¿eby kreska by³a równo - przesuniêcie w lewo
                case TypeOfMove.SW:
                    return new Vector2(0, -1f);
                case TypeOfMove.W:
                    return new Vector2(0, -1f + 0.0625f);// offset ¿eby kreska by³a równo - przesuniêcie w dó³ i troszkê w górê
                case TypeOfMove.NW:
                    return Vector2.Zero;
            }
            return Vector2.Zero;
        }

        public static Vector2 TranslateDirectionToVector(TypeOfMove move)
        {
            switch (move)
            {
                case TypeOfMove.N:
                    return new Vector2(0, -1);
                case TypeOfMove.NE:
                    return new Vector2(1, -1);
                case TypeOfMove.E:
                    return new Vector2(1, 0);
                case TypeOfMove.SE:
                    return new Vector2(1, 1);
                case TypeOfMove.S:
                    return new Vector2(0, 1);
                case TypeOfMove.SW:
                    return new Vector2(-1, 1);
                case TypeOfMove.W:
                    return new Vector2(-1, 0);
                case TypeOfMove.NW:
                    return new Vector2(-1, -1);
            }
            return Vector2.Zero;
        }

        public static TypeOfMove TranslateVectorToMove(Vector2 vector)
        {
            if (vector.X == 0.0f)
            {
                if (vector.Y > 0.0f)
                    return TypeOfMove.S;
                if (vector.Y < 0.0f)
                    return TypeOfMove.N;
            }
            if (vector.X > 0.0f)
            {
                if (vector.Y > 0.0f)
                    return TypeOfMove.SE;
                if (vector.Y < 0.0f)
                    return TypeOfMove.NE;
            }
            if (vector.X < 0.0f)
            {
                if (vector.Y > 0.0f)
                    return TypeOfMove.SW;
                if (vector.Y < 0.0f)
                    return TypeOfMove.NW;
            }
            if (vector.Y == 0.0f)
            {
                if (vector.X > 0.0f)
                    return TypeOfMove.E;
                if (vector.X < 0.0f)
                    return TypeOfMove.W;
            }
            return TypeOfMove.UNKNOWN;
        }

        public static TypeOfMoveSprites TranslateMoveToSpriteType(TypeOfMove move)
        {
            switch (move)
            {
                case TypeOfMove.N:
                    return TypeOfMoveSprites.VERTICAL;
                case TypeOfMove.NE:
                    return TypeOfMoveSprites.SLASH;
                case TypeOfMove.E:
                    return TypeOfMoveSprites.HORIZONTAL;
                case TypeOfMove.SE:
                    return TypeOfMoveSprites.BACKSLASH;
                case TypeOfMove.S:
                    return TypeOfMoveSprites.VERTICAL;
                case TypeOfMove.SW:
                    return TypeOfMoveSprites.SLASH;
                case TypeOfMove.W:
                    return TypeOfMoveSprites.HORIZONTAL;
                case TypeOfMove.NW:
                    return TypeOfMoveSprites.BACKSLASH;
            }
            return TypeOfMoveSprites.NONE;
        }
             
    }
}
