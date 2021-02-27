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
    public abstract class DrawableGameObject
    {
        Texture2D objectTexture;

        public Texture2D ObjectTexture
        {
            get { return objectTexture; }
            set { objectTexture = value; }
        }

        Vector2 position;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        Rectangle destinationRectangle;

        public Rectangle DestinationRectangle
        {
            get { return destinationRectangle; }
            set { destinationRectangle = value; }
        }

        Rectangle? sourceRectangle;

        public Rectangle? SourceRectangle
        {
            get { return sourceRectangle; }
            set { sourceRectangle = value; }
        }

        Color color = Color.White;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        float layerDepth = 0.0f;

        public float LayerDepth
        {
            get { return layerDepth; }
            set { layerDepth = value; }
        }

        float roation = 0.0f;

        public float Roation
        {
            get { return roation; }
            set { roation = value; }
        }

        Vector2 origin;

        public Vector2 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        float scale = 1.0f;

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        string textureName;

        public string TextureName
        {
            get { return textureName; }
            set { textureName = value; }
        }

        bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;
                //if (!visible)
                //    active = false;
            }
        }

        bool active = true;

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }


        public virtual void LoadTexture(ContentManager contentManager)
        {
            this.ObjectTexture = contentManager.Load<Texture2D>(TextureName);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        { }
    }
}
