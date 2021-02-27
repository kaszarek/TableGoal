using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;

namespace TableGoal
{
    class MultiplayerTips : UIElement
    {
        SpriteFont _tipsFont;
        string[] _tips;
        int _actualTipIndex;
        readonly float TIP_TIME_OUT = 18;
        float _tip_lifespan = 0;
        Vector2 _tipSize;
        /// <summary>
        /// Center of the top (up) edge of a tips' box
        /// </summary>
        Vector2 _tipsCenterTopLocation;

        /// <summary>
        /// Creates a multiplayer tips box
        /// </summary>
        /// <param name="topCenterOfTipsBox">Center of the top edge of a tips' box</param>
        public MultiplayerTips(Vector2 topCenterOfTipsBox)
        {
            Initialize(topCenterOfTipsBox);
        }

        private void Initialize(Vector2 topCenterOfTipsBox)
        {
            _tipsCenterTopLocation = topCenterOfTipsBox;
            _tips = new string[10];
            _tips[0] = " stay online during gameplay\notherwise your opponent wins";
            _tips[1] = "you cannot pause realtime multiplayer game";
            _tips[2] = " try to use existing moves -\nbounce from thier endings";
            _tips[3] = "force your opponent to come to a deadlock\n                   that is a point too";
            _tips[4] = "in defence try to block opponent's way\n                  to the goal area";
            _tips[5] = "  if you disconnect by any means\nyour opponent wins automatically";
            _tips[6] = " if needed, exceed screen timeout so screen\ndoes not dim out in the middle of the game";
            _tips[7] = "in WIFI multiplayer both players have to be\n      connected to the same WIFI network";
            _tips[8] = "in multiplayer keep in mind elapsing time\n                       for your turn";
            _tips[9] = "  for the best performance both players\nshould have the same version of the app";
            _tip_lifespan = 2 * TIP_TIME_OUT;
            _tipSize = new Vector2();
            this.Color = Color.Red;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_tipsFont,
                                   _tips[_actualTipIndex],
                                   new Vector2(_tipsCenterTopLocation.X - _tipSize.X / 2, _tipsCenterTopLocation.Y),
                                   this.Color);
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            _tipsFont = contentManager.Load<SpriteFont>("Fonts/TRIAL_font");
            HandleTipsDisplaying(0, 9);
        }

        public override void Update(GameTime gametime)
        {
            HandleTipsDisplaying((float)gametime.ElapsedGameTime.TotalSeconds);
        }

        private void HandleTipsDisplaying(float timeElapse)
        {
            _tip_lifespan += timeElapse;
            if (_tip_lifespan >= TIP_TIME_OUT)
            {
                _tip_lifespan = 0;
                Random r = new Random();
                _actualTipIndex = r.Next(0, _tips.Length);
                _tipSize = _tipsFont.MeasureString(_tips[_actualTipIndex]);
            }
        }

        private void HandleTipsDisplaying(float timeElapse, int preSetIndex)
        {
            if (preSetIndex >= _tips.Length)
            {
                throw new Exception(String.Format("Przekazany indeks ({0}), jest wiêkszy ni¿ rozmiar tabeli ({1})", preSetIndex, _tips.Length));
            }
            _tip_lifespan += timeElapse;
            if (_tip_lifespan >= TIP_TIME_OUT)
            {
                _tip_lifespan = 0;
                _actualTipIndex = preSetIndex;
                _tipSize = _tipsFont.MeasureString(_tips[_actualTipIndex]);
            }
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (Visible)
            {
                _tip_lifespan = 0;
                _actualTipIndex = (_actualTipIndex + 1) % _tips.Length;
                _tipSize = _tipsFont.MeasureString(_tips[_actualTipIndex]);
            }
        }

    }
}
