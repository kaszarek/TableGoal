using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    public enum TypeOfMove
    {
        N,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW,
        UNKNOWN
    }

    public enum TypeOfMoveSprites
    {
        VERTICAL = 1,
        HORIZONTAL = 2,
        SLASH = 3,
        BACKSLASH = 4,
        NONE = 5
    }

    public enum TypeOfPlayer
    {
        First,
        Second
    }

    public class GameMove
    {
        public Vector2 StartPosition { get; set; }
        public Vector2 EndPosition { get; set; }
        public TypeOfMove Move { get; set; }
        public TypeOfPlayer Player { get; set; }
        public Team OwnerOfMove { get; set; }

        public GameMove()
        { }

        public GameMove(Vector2 startPosition, Vector2 endPosition, TypeOfMove move, TypeOfPlayer player)
        {
            this.StartPosition = startPosition;
            this.EndPosition = endPosition;
            this.Move = move;
            this.Player = player;
        }

        public GameMove(Vector2 startPosition, Vector2 endPosition, TypeOfMove move, TypeOfPlayer player, Team ownerOfMove)
        {
            this.StartPosition = startPosition;
            this.EndPosition = endPosition;
            this.Move = move;
            this.Player = player;
            this.OwnerOfMove = ownerOfMove;
        }
    }
}
