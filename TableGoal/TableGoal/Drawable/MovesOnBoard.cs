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
    class MovesOnBoard: DrawableGameObject
    {
        Texture2D _moves;

        public Texture2D Moves
        {
            get { return _moves; }
            set { _moves = value; }
        }
        Dictionary<TypeOfMoveSprites, Rectangle> _movesSprites;

        public Dictionary<TypeOfMoveSprites, Rectangle> MovesSprites
        {
            get { return _movesSprites; }
            set { _movesSprites = value; }
        }

        public MovesOnBoard(string textureName)
        {
            this.TextureName = textureName;
            _movesSprites = new Dictionary<TypeOfMoveSprites, Rectangle>();
            _movesSprites.Add(TypeOfMoveSprites.HORIZONTAL, new Rectangle(0, 0, 64, 64));
            _movesSprites.Add(TypeOfMoveSprites.VERTICAL, new Rectangle(192, 0, 64, 64));
            _movesSprites.Add(TypeOfMoveSprites.SLASH, new Rectangle(64, 0, 64, 64));
            _movesSprites.Add(TypeOfMoveSprites.BACKSLASH, new Rectangle(128, 0, 64, 64));
            _movesSprites.Add(TypeOfMoveSprites.NONE, new Rectangle());
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void LoadTexture(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            _moves = contentManager.Load<Texture2D>(TextureName);
        }
    }
}
