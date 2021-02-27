using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;

namespace TableGoal
{
    public abstract class UIElement : DrawableGameObject
    {
        public virtual void Update(GameTime gametime)
        { }

        public virtual void HandleInput(Vector2 tapPoint)
        { }
    }
}
