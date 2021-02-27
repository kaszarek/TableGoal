using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;

namespace TableGoal
{
    class GlobalMultiRoom : UIElement
    {
        private bool pressed;

        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }
        SpriteFont _mainElement;
        SpriteFont _detailsElements;
        UIPicture _horizontalLine;
        UIPicture _verticalLine;
        UIPicture _empty;
        int _linesWidth = 4;
        int _ymarigin = 5;
        int _xmarigin = 5;
        RoomDetails _rd;
        int _index;
        Rectangle _originalRect;

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        int _yDisplacement;

        internal RoomDetails FullRoomDetails
        {
            get { return _rd; }
            set { _rd = value; }
        }
        Color _selectionColor;
        int cellH = 100;

        public GlobalMultiRoom(Rectangle destination, RoomDetails details, ref SpriteFont mainFont, ref SpriteFont detailsFont, ref UIPicture hLine, ref UIPicture vLine, ref UIPicture empty, int index, int yDisplacement)
        {
            _rd = details;
            _index = index;
            _yDisplacement = yDisplacement;
            _originalRect = destination;
            this.DestinationRectangle = new Rectangle(destination.X, destination.Y + _index * cellH + yDisplacement, destination.Width, destination.Height);
            _mainElement = mainFont;
            _detailsElements = detailsFont;
            _horizontalLine = hLine;
            _verticalLine = vLine;
            _empty = empty;
            _selectionColor = Color.Yellow;
            Active = true;
            pressed = false;
        }

        public void AdjustPosition(int yDisplacement)
        {
            _yDisplacement = yDisplacement;
            this.DestinationRectangle = new Rectangle(_originalRect.X , _originalRect.Y + _index * cellH + _yDisplacement, DestinationRectangle.Width, DestinationRectangle.Height);
        }

        public void SpecialAdjustments()
        {
            this.DestinationRectangle = new Rectangle(_originalRect.X, _originalRect.Y + _index * cellH + _yDisplacement, DestinationRectangle.Width, DestinationRectangle.Height);
        }

        public override void LoadTexture(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
        }

        public void DecreaseIndex()
        {
            _index--;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawRoomInfo(spriteBatch);
        }

        private void DrawRoomInfo(SpriteBatch spriteBatch)
        {
            Rectangle itemBorder = new Rectangle(DestinationRectangle.X + (int)(_xmarigin * 1.5f),
                                                 DestinationRectangle.Y + _ymarigin,
                                                 DestinationRectangle.Width - 3 * _xmarigin,
                                                 80);
            if (pressed)
            {
                spriteBatch.Draw(_empty.ObjectTexture, itemBorder, _selectionColor);
            }
            DrawItemFrame(spriteBatch, itemBorder);
            spriteBatch.DrawString(_mainElement,
                                   _rd.Name,
                                   new Vector2(DestinationRectangle.X + 3 * _xmarigin,
                                               DestinationRectangle.Y + _ymarigin),
                                   _rd.HostColor);

            if (_rd.IsGoalLimited)
                spriteBatch.DrawString(_detailsElements,
                                       String.Format("{0} field | {1} goal(s)", _rd.FieldType.ToString(), _rd.GameLimit.ToString()),
                                       new Vector2(DestinationRectangle.X + 6 * _xmarigin,
                                                   DestinationRectangle.Y + _ymarigin + 50),
                                       _rd.HostColor);
            else
                spriteBatch.DrawString(_detailsElements,
                                       String.Format("{0} field | {1} min", _rd.FieldType.ToString(), (_rd.GameLimit / 60).ToString()),
                                       new Vector2(DestinationRectangle.X + 6 * _xmarigin,
                                                   DestinationRectangle.Y + _ymarigin + 50),
                                       _rd.HostColor);
        }

        private void DrawItemFrame(SpriteBatch spriteBatch, Rectangle rect)
        {
            _horizontalLine.DestinationRectangle = new Rectangle(rect.X,
                                                                 rect.Y,
                                                                 rect.Width,
                                                                 _linesWidth);
            _horizontalLine.Draw(spriteBatch);
            _horizontalLine.DestinationRectangle = new Rectangle(rect.X,
                                                                 rect.Bottom,
                                                                 rect.Width,
                                                                 _linesWidth);
            _horizontalLine.Draw(spriteBatch);

            _verticalLine.DestinationRectangle = new Rectangle(rect.X,
                                                               rect.Y,
                                                               _linesWidth,
                                                               rect.Height);
            _verticalLine.Draw(spriteBatch);
            _verticalLine.DestinationRectangle = new Rectangle(rect.Right,
                                                               rect.Y,
                                                               _linesWidth,
                                                               rect.Height);
            _verticalLine.Draw(spriteBatch);
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (Visible && Active)
            {
                if (DestinationRectangle.Contains((int)tapPoint.X, (int)tapPoint.Y))
                {
                    pressed = !pressed;
                }
            }
        }

        public bool WasPressed(Vector2 tapPoint)
        {
            if (Visible && Active)
            {
                if (DestinationRectangle.Contains((int)tapPoint.X, (int)tapPoint.Y))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
