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
    class UIScrollableListGlobal : UIElement
    {
        SpriteFont _mainElement;
        SpriteFont _detailsElements;
        UIPicture _horizontalLine;
        UIPicture _verticalLine;
        UIPicture _empty;
        int _linesWidth = 4;
        int _ymarigin = 5;
        int _xmarigin = 5;
        int _offset = 0;
        int countingDots;

        public int YScrollingOffset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        List<Rectangle> _items;
        Rectangle _selectionRect;
        int _selectedIndex;
        Color _selectionColor;
        List<RoomDetails> _listContent;
        
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            internal set { _selectedIndex = value; }
        }

        public UIScrollableListGlobal(Rectangle destination, List<RoomDetails> listContent)
        {
            this.DestinationRectangle = destination;
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
            _listContent = listContent;
            countingDots = 0;
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
            if (_items.Count > _listContent.Count)
            {
                _items.Clear();
                _selectedIndex = -1;
                _selectionRect = new Rectangle(0, 0, 0, 0);
            }
            if (!GlobalMultiProvider.IsConnected)
            {
                countingDots = gametime.TotalGameTime.Seconds % 3;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!GlobalMultiProvider.IsConnected)
            {
                switch (countingDots)
                {
                    case 0:
                        spriteBatch.DrawString(_mainElement,
                                               "Connecting.",
                                               new Vector2(DestinationRectangle.X + 15,
                                                           DestinationRectangle.Y + 5),
                                               Color.Black);
                        break;
                    case 1:
                        spriteBatch.DrawString(_mainElement,
                                               "Connecting..",
                                               new Vector2(DestinationRectangle.X + 15,
                                                           DestinationRectangle.Y + 5),
                                               Color.Black);
                        break;
                    case 2:
                        spriteBatch.DrawString(_mainElement,
                                               "Connecting...",
                                               new Vector2(DestinationRectangle.X + 15,
                                                           DestinationRectangle.Y + 5),
                                               Color.Black);
                        break;
                    default:
                        spriteBatch.DrawString(_mainElement,
                                               "Connecting",
                                               new Vector2(DestinationRectangle.X + 15,
                                                           DestinationRectangle.Y + 5),
                                               Color.Black);
                        break;
                }
                spriteBatch.DrawString(_detailsElements,
                                       "Please stay tuned, soon you will see rooms' list.",
                                       new Vector2(DestinationRectangle.X + 30,
                                                   DestinationRectangle.Y + 55),
                                       Color.Black);
                return;
            }
            spriteBatch.Draw(_empty.ObjectTexture, new Rectangle(_selectionRect.X, _selectionRect.Y + _offset, _selectionRect.Width, _selectionRect.Height), _selectionColor);
            int i = 0;
            int hostplayers = 0;            
            foreach (RoomDetails rd in _listContent)
            {
                DrawRoomInfo(spriteBatch, rd, i, _offset);
                i++;
                hostplayers++;
                //if (i >= 4) // Limit do 4 pokoi -> jak na razie
                //{
                //    break;
                //}
            }
            if (hostplayers == 0)
            {
                spriteBatch.DrawString(_mainElement,
                                       "No games available ...",
                                       new Vector2(DestinationRectangle.X + 3 * _xmarigin,
                                                   DestinationRectangle.Y + _ymarigin),
                                       Color.Black);
                spriteBatch.DrawString(_detailsElements,
                                       "Host a new game or ask your friend to do it.",
                                       new Vector2(DestinationRectangle.X + 6 * _xmarigin,
                                                   DestinationRectangle.Y + _ymarigin + 50),
                                       Color.Black);
            }
        }

        private void DrawRoomInfo(SpriteBatch spriteBatch, RoomDetails rd, int index, int offset)
        {
            int cellH = 100;
            Rectangle itemBorder = new Rectangle(DestinationRectangle.X + (int)(_xmarigin * 1.5f),
                                                 DestinationRectangle.Y + index * cellH + _ymarigin + offset,
                                                 DestinationRectangle.Width - 3 * _xmarigin,
                                                 80);
            DrawItemFrame(spriteBatch, itemBorder);
            if (_items.Count <= index)
                _items.Add(itemBorder);

                spriteBatch.DrawString(_mainElement,
                                       rd.Name,
                                       new Vector2(DestinationRectangle.X + 3 * _xmarigin,
                                                   DestinationRectangle.Y + index * cellH + _ymarigin + offset),
                                       rd.HostColor);
            
                if (rd.IsGoalLimited)
                    spriteBatch.DrawString(_detailsElements,
                                           String.Format("{0} field | {1} goal(s)", rd.FieldType.ToString(), rd.GameLimit.ToString()),
                                           new Vector2(DestinationRectangle.X + 6 * _xmarigin,
                                                       DestinationRectangle.Y + index * cellH + _ymarigin + 50 + offset),
                                           rd.HostColor);
                else
                    spriteBatch.DrawString(_detailsElements,
                                           String.Format("{0} field | {1} min", rd.FieldType.ToString(), (rd.GameLimit / 60).ToString()),
                                           new Vector2(DestinationRectangle.X + 6 * _xmarigin,
                                                       DestinationRectangle.Y + index * cellH + _ymarigin + 50 + offset),
                                           rd.HostColor);
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

        public RoomDetails SelectedRoom()
        {
            int i = 0;
            foreach (RoomDetails rd in _listContent)
            {

                if (i == _selectedIndex)
                    return rd;
                i++;
            }
            return null;
        }
    }
}
