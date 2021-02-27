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
    class UIScrollableList : UIElement
    {
        SpriteFont _mainElement;
        SpriteFont _detailsElements;
        UIPicture _horizontalLine;
        UIPicture _verticalLine;
        UIPicture _empty;
        int _linesWidth = 4;
        int _ymarigin = 5;
        int _xmarigin = 5;
        string _avoidedName;
        int _offset = 0;
        List<Rectangle> _items;
        Rectangle _selectionRect;
        int _selectedIndex;
        Color _selectionColor;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            internal set { _selectedIndex = value; }
        }

        public UIScrollableList(Rectangle destination, string me)
        {
            this.DestinationRectangle = destination;
            _avoidedName = me;
            _horizontalLine = new UIPicture("WC/hor_line", new Rectangle(0, 0, 0, 0));
            _horizontalLine.Color = Color.Black;
            _verticalLine = new UIPicture("WC/ver_line", new Rectangle(0, 0, 0, 0));
            _verticalLine.Color = Color.Black;
            _empty = new UIPicture("halftrans4x4", new Rectangle(0, 0, 0, 0));
            _empty.Color = Color.White;
            _items = new List<Rectangle>();
            _selectionRect = new Rectangle(0, 0, 0, 0);
            _selectedIndex = -1;
            _selectionColor = Color.Yellow;
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            _mainElement = contentManager.Load<SpriteFont>("Fonts/SketchRockwell");
            _detailsElements = contentManager.Load<SpriteFont>("Fonts/Sketch15");
            _horizontalLine.LoadTexture(contentManager);
            _verticalLine.LoadTexture(contentManager);
            _empty.LoadTexture(contentManager);
        }

        public override void Update(GameTime gametime)
        {
            if (_items.Count > TableGoal.Players.Count)
            {
                _items.Clear();
                _selectedIndex = -1;
                _selectionRect = new Rectangle(0, 0, 0, 0);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //DrawTableBorders(spriteBatch);
            spriteBatch.Draw(_empty.ObjectTexture, _selectionRect, _selectionColor);
            int i = 0;
            int hostplayers = 0;
            foreach (PlayerInfo pi in TableGoal.Players)
            {
                if (pi.GameLimit != 0)
                {
                    DrawPlayerInfo(spriteBatch, pi, i, _offset);
                    i++;
                    hostplayers++;
                }
                if (i >= 4)
                    break;
            }
            if (hostplayers == 0)
            {
                spriteBatch.DrawString(_mainElement,
                                       "No games available ...",
                                       new Vector2(DestinationRectangle.X + 3 * _xmarigin,
                                                   DestinationRectangle.Y + _ymarigin),
                                       Color.Black);
                spriteBatch.DrawString(_detailsElements,
                                       "Host a new Wifi game or ask your friend to do it.",
                                       new Vector2(DestinationRectangle.X + 6 * _xmarigin,
                                                   DestinationRectangle.Y + _ymarigin + 50),
                                       Color.Black);
                if (_selectedIndex != -1)
                {
                    _items.Clear();
                    _selectedIndex = -1;
                    _selectionRect = new Rectangle(0, 0, 0, 0);
                }
            }
        }

        private void DrawPlayerInfo(SpriteBatch spriteBatch, PlayerInfo pi, int index, int offset)
        {
            int cellH = 100;
            Rectangle itemBorder = new Rectangle(DestinationRectangle.X + (int)(_xmarigin * 1.5f),
                                                 DestinationRectangle.Y + index * cellH + _ymarigin + offset,
                                                 DestinationRectangle.Width - 3 * _xmarigin,
                                                 80);
            DrawItemFrame(spriteBatch, itemBorder);
            if (_items.Count <= index)
                _items.Add(itemBorder);

            //if (pi.PlayerName != _avoidedName)
            //{
                spriteBatch.DrawString(_mainElement,
                                       pi.PlayerName,
                                       new Vector2(DestinationRectangle.X + 3 * _xmarigin,
                                                   DestinationRectangle.Y + index * cellH + _ymarigin + offset),
                                       pi.PlayerColor);

                if (pi.IsGoalLimited)
                    spriteBatch.DrawString(_detailsElements,
                                           String.Format("{0} field | {1} goal(s)", pi.Field.ToString(), pi.GameLimit.ToString()),
                                           new Vector2(DestinationRectangle.X + 6 * _xmarigin,
                                                       DestinationRectangle.Y + index * cellH + _ymarigin + 50 + offset),
                                           pi.PlayerColor);
                else
                    spriteBatch.DrawString(_detailsElements,
                                           String.Format("{0} field | {1} min", pi.Field.ToString(), (pi.GameLimit / 60).ToString()),
                                           new Vector2(DestinationRectangle.X + 6 * _xmarigin,
                                                       DestinationRectangle.Y + index * cellH + _ymarigin + 50 + offset),
                                           pi.PlayerColor);
            //}
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

        private void DrawTableBorders(SpriteBatch spriteBatch)
        {
            _horizontalLine.DestinationRectangle = new Rectangle(this.DestinationRectangle.X,
                                                                 this.DestinationRectangle.Y,
                                                                 this.DestinationRectangle.Width,
                                                                 _linesWidth);
            _horizontalLine.Draw(spriteBatch);
            _horizontalLine.DestinationRectangle = new Rectangle(this.DestinationRectangle.X,
                                                                 this.DestinationRectangle.Bottom,
                                                                 this.DestinationRectangle.Width,
                                                                 _linesWidth);
            _horizontalLine.Draw(spriteBatch);

            _verticalLine.DestinationRectangle = new Rectangle(this.DestinationRectangle.X,
                                                               this.DestinationRectangle.Y,
                                                               _linesWidth,
                                                               this.DestinationRectangle.Height);
            _verticalLine.Draw(spriteBatch);
            _verticalLine.DestinationRectangle = new Rectangle(this.DestinationRectangle.Right,
                                                               this.DestinationRectangle.Y,
                                                               _linesWidth,
                                                               this.DestinationRectangle.Height);
            _verticalLine.Draw(spriteBatch);

        }

        public override void HandleInput(Vector2 tapPoint)
        {
            bool clickedOutside = true;
            int i = 0;
            foreach (Rectangle r in _items)
            {
                if (r.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    _selectionRect = r;
                    clickedOutside = false;
                    _selectedIndex = i;
                    AudioManager.PlaySound("selected");
                    break;
                }
                i++;
            }
            if (clickedOutside)
            {
                _selectionRect = new Rectangle(0, 0, 0, 0);
                _selectedIndex = -1;
            }
        }

        public Rectangle SelectedArea()
        {
            if (_selectedIndex >= 0)
            {
                return _items[_selectedIndex];
            }
            return Rectangle.Empty;
        }

        public PlayerInfo SelectedPlayer()
        {
            int i = 0;
            foreach (PlayerInfo pi in TableGoal.Players)
            {
                if (pi.GameLimit != 0)
                {
                    if (i == _selectedIndex)
                        return pi;
                    i++;
                }
            }

            return null;
        }
    }
}
