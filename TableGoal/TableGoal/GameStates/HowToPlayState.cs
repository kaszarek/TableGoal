using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace TableGoal
{
    class HowToPlayState : GameState
    {
        private static readonly string _textureName = "MenusElements/HowToPlay";
        private static readonly string _background = "Backgrounds/field10x8";
        Menu menu;
        Texture2D _HOW_TO_PLAY_TEXTURE;
        UIPicture _previous;
        UIPicture _next;
        UIPicture _close;

        UIPicture _borders;
        UIPicture _startingPoint;
        UIPicture _playersGoal;
        UIPicture _opponentsGoal;

        UIPicture _ObjectivesLabel;
        UIPicture _objGoal;
        UIPicture _objBlock;
        UIPicture _deadlock;
        UIPicture _RulesLabel;
        UIPicture _rulTakeTurns;
        UIPicture _rulMoves;
        UIPicture _rulExtraMove;
        UIPicture _bounce;

        UIPicture _explainationMovesByArrows;
        UIPicture _explainationMovesByGestures;
        UIPicture _tryItOut;
        UIPicture _changeControlls;
        
        List<UIElement> _guideElements;
        Texture2D patternGoal;
        Rectangle firstPlayerGOAL;
        Rectangle secondPlayerGOAL;
        Ball ball;


        CheckBox _fieldPicker;
        UIPicture _gameTypeExplaination;
        CheckBox _gameTypePicker;
        UIPicture _fieldTypeExplaination;

        UIPicture _selectTime;
        UIPicture _selectTimeExplaination;
        UIPicture _selectTimeArrow;
        

        List<GameMove> _movesHistory = new List<GameMove>();
        MovesOnBoard _moves;
        int _wallSize = 55;
        bool _addingMoves = false;
        
        int taps;

        float MOVE_WAIT = 1.2f;
        float _curent_move_time;

        public HowToPlayState()
        {
            _curent_move_time = MOVE_WAIT;
            _movesHistory = new List<GameMove>();
            _moves = new MovesOnBoard("Moves");
            this.EnabledGestures = GestureType.Tap | GestureType.Flick;
            menu = new Menu(_background, new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            taps = 0;
            ball = new Ball("ball");
            ball.Position = new Vector2(400, 240);
            firstPlayerGOAL = new Rectangle(67, 188, 55, 105);
            secondPlayerGOAL = new Rectangle(679, 188, 55, 105);

            _guideElements = new List<UIElement>();



            _previous = new UIPicture(_textureName, new Rectangle(10, 370, 100, 100));
            _previous.SourceRectangle = new Rectangle(475, 175, 100, 100);
            _previous.Color = Color.Black;
            _guideElements.Add(_previous);

            _next = new UIPicture(_textureName, new Rectangle(690, 370, 100, 100));
            _next.SourceRectangle = new Rectangle(575, 175, 100, 100);
            _next.Color = Color.Black;
            _guideElements.Add(_next);

            _close = new UIPicture(_textureName, new Rectangle(690, 10, 100, 100));
            _close.SourceRectangle = new Rectangle(575, 275, 100, 100);
            _close.Color = Color.Black;
            _guideElements.Add(_close);



            _borders = new UIPicture(_textureName, new Rectangle(130, 35, 430, 35));
            _borders.SourceRectangle = new Rectangle(0, 0, 430, 35);
            _borders.Color = Color.Black;
            _guideElements.Add(_borders);

            _playersGoal = new UIPicture(_textureName, new Rectangle(125, 210, 120, 35));
            _playersGoal.SourceRectangle = new Rectangle(435, 0, 120, 35);
            _playersGoal.Color = Color.Black;
            _guideElements.Add(_playersGoal);

            _opponentsGoal = new UIPicture(_textureName, new Rectangle(490, 210, 180, 35));
            _opponentsGoal.SourceRectangle = new Rectangle(0, 40, 180, 35);
            _opponentsGoal.Color = Color.Black;
            _guideElements.Add(_opponentsGoal);

            _startingPoint = new UIPicture(_textureName, new Rectangle(320, 260, 170, 35));
            _startingPoint.SourceRectangle = new Rectangle(190, 40, 170, 35);
            _startingPoint.Color = Color.Black;
            _guideElements.Add(_startingPoint);

            _deadlock = new UIPicture(_textureName, new Rectangle(210, 310, 110, 35));
            _deadlock.SourceRectangle = new Rectangle(550, 415, 110, 35);
            _deadlock.Color = Color.Black;
            _guideElements.Add(_deadlock);



            _ObjectivesLabel = new UIPicture(_textureName, new Rectangle(130, 40, 175, 35));
            _ObjectivesLabel.SourceRectangle = new Rectangle(0, 75, 175, 35);
            _ObjectivesLabel.Color = Color.Black;
            _guideElements.Add(_ObjectivesLabel);

            _objGoal = new UIPicture(_textureName, new Rectangle(140, 80, 500, 35));
            _objGoal.SourceRectangle = new Rectangle(175, 80, 500, 35);
            _objGoal.Color = Color.Black;
            _guideElements.Add(_objGoal);

            _objBlock = new UIPicture(_textureName, new Rectangle(140, 120, 530, 75));
            _objBlock.SourceRectangle = new Rectangle(0, 120, 530, 75);
            _objBlock.Color = Color.Black;
            _guideElements.Add(_objBlock);

            //_RulesLabel = new UIPicture(_textureName, new Rectangle(130, 230, 90, 35));
            _RulesLabel = new UIPicture(_textureName, new Rectangle(130, 40, 90, 35));
            _RulesLabel.SourceRectangle = new Rectangle(365, 40, 90, 35);
            _RulesLabel.Color = Color.Black;
            _guideElements.Add(_RulesLabel);

            //_rulTakeTurns = new UIPicture(_textureName, new Rectangle(140, 270, 450, 35));
            _rulTakeTurns = new UIPicture(_textureName, new Rectangle(140, 80, 450, 35));
            _rulTakeTurns.SourceRectangle = new Rectangle(0, 200, 450, 35);
            _rulTakeTurns.Color = Color.Black;
            _guideElements.Add(_rulTakeTurns);

            //_rulMoves = new UIPicture(_textureName, new Rectangle(140, 310, 435, 75));
            _rulMoves = new UIPicture(_textureName, new Rectangle(140, 120, 435, 75));
            _rulMoves.SourceRectangle = new Rectangle(0, 240, 435, 75);
            _rulMoves.Color = Color.Black;
            _guideElements.Add(_rulMoves);

            //_rulExtraMove = new UIPicture(_textureName, new Rectangle(140, 390, 480, 75));
            _rulExtraMove = new UIPicture(_textureName, new Rectangle(140, 200, 480, 75));
            _rulExtraMove.SourceRectangle = new Rectangle(0, 320, 480, 75);
            _rulExtraMove.Color = Color.Black;
            _guideElements.Add(_rulExtraMove);

            _bounce = new UIPicture(_textureName, new Rectangle(220, 320, 90, 35));
            _bounce.SourceRectangle = new Rectangle(550, 380, 90, 35);
            _bounce.Color = Color.Black;
            _guideElements.Add(_bounce);



            _tryItOut = new UIPicture(_textureName, new Rectangle(130, 40, 175, 35));
            _tryItOut.SourceRectangle = new Rectangle(460, 40, 175, 35);
            _tryItOut.Color = Color.Black;
            _guideElements.Add(_tryItOut);

            _explainationMovesByGestures = new UIPicture(_textureName, new Rectangle(140, 80, 375, 35));
            _explainationMovesByGestures.SourceRectangle = new Rectangle(0, 400, 375, 35);
            _explainationMovesByGestures.Color = Color.Black;
            _guideElements.Add(_explainationMovesByGestures);

            _explainationMovesByArrows = new UIPicture(_textureName, new Rectangle(140, 120, 465, 35));
            _explainationMovesByArrows.SourceRectangle = new Rectangle(0, 440, 465, 35);
            _explainationMovesByArrows.Color = Color.Black;
            _guideElements.Add(_explainationMovesByArrows);

            _changeControlls = new UIPicture(_textureName, new Rectangle(235, 370, 335, 80));
            _changeControlls.SourceRectangle = new Rectangle(0, 475, 335, 80);
            _changeControlls.Color = Color.Black;
            _guideElements.Add(_changeControlls);



            _gameTypePicker = new CheckBox("MenusElements/GameTypeChosing");
            _gameTypePicker.DestinationRectangle = new Rectangle(140, 50, 170, 155);
            _gameTypePicker.Visible = false;

            _gameTypeExplaination = new UIPicture(_textureName, new Rectangle(310, 100, 300, 38));
            _gameTypeExplaination.SourceRectangle = new Rectangle(0, 605, 300, 38);
            _gameTypeExplaination.Color = Color.Black;
            _guideElements.Add(_gameTypeExplaination);


            _fieldPicker = new CheckBox("MenusElements/FieldChosing");
            _fieldPicker.DestinationRectangle = new Rectangle(140, 270, 160, 160);
            _fieldPicker.Visible = false;

            _fieldTypeExplaination = new UIPicture(_textureName, new Rectangle(310, 320, 290, 37));
            _fieldTypeExplaination.SourceRectangle = new Rectangle(0, 565, 290, 37);
            _fieldTypeExplaination.Color = Color.Black;
            _guideElements.Add(_fieldTypeExplaination);



            _selectTime = new UIPicture(_textureName, new Rectangle(150, 22, 500, 300));
            _selectTime.SourceRectangle = new Rectangle(0, 760, 400, 240);
            _guideElements.Add(_selectTime);

            _selectTimeExplaination = new UIPicture(_textureName, new Rectangle(220, 370, 360, 85));
            _selectTimeExplaination.SourceRectangle = new Rectangle(0, 645, 360, 85);
            _selectTimeExplaination.Color = Color.Black;
            _guideElements.Add(_selectTimeExplaination);

            _selectTimeArrow = new UIPicture(_textureName, new Rectangle(380, 100, 40, 260));
            _selectTimeArrow.SourceRectangle = new Rectangle(480, 760, 40, 240);
            _selectTimeArrow.Color = Color.Black;
            _guideElements.Add(_selectTimeArrow);

            SetProperGuidance();
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            ball.LoadTexture(GameManager.Game.Content);
            patternGoal = GameManager.Game.Content.Load<Texture2D>("patternGoal");
            _HOW_TO_PLAY_TEXTURE = GameManager.Game.Content.Load<Texture2D>(_textureName);
            _moves.LoadTexture(GameManager.Game.Content);
            foreach (UIElement element in _guideElements)
                element.ObjectTexture = _HOW_TO_PLAY_TEXTURE;
            _fieldPicker.LoadTexture(GameManager.Game.Content);
            _gameTypePicker.LoadTexture(GameManager.Game.Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || _close.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
                return;
            }
            if (_next.Pressed)
            {
                AudioManager.PlaySound("selected");
                taps++;
                _curent_move_time = MOVE_WAIT;
                _next.Pressed = false;
                SetProperGuidance();
            }
            if (_previous.Pressed)
            {
                AudioManager.PlaySound("selected");
                if (taps > 0)
                    taps--;
                _curent_move_time = MOVE_WAIT;
                _previous.Pressed = false;
                SetProperGuidance();
            }
            MovesAnimation(gameTime);
        }

        private void MovesAnimation(GameTime gameTime)
        {
            if (taps != 2 && taps != 1)
                return;
            _curent_move_time -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_curent_move_time <= 0.0f)
            {
                _curent_move_time = MOVE_WAIT;
                if (taps == 1)
                    PresentBouncing();
                if (taps == 2)
                PresentDeadlock();            
            }
        }

        #region Animacja przyk³adowych uk³adów - odbicia i zablokowania
        private void PresentBouncing()
        {
            _bounce.Visible = false;
            if (_movesHistory.Count == 0)
            {
                _movesHistory.Add(new GameMove(ball.Position, new Vector2(455, 295), TypeOfMove.SE, TypeOfPlayer.First));
                ball.Position = new Vector2(455, 295);
                return;
            }
            if (_movesHistory.Count == 1)
            {
                _movesHistory.Add(new GameMove(new Vector2(455, 295), new Vector2(400, 350), TypeOfMove.SW, TypeOfPlayer.Second));
                ball.Position = new Vector2(400, 350);
                return;
            }
            if (_movesHistory.Count == 2)
            {
                _movesHistory.Add(new GameMove(new Vector2(400, 350), new Vector2(455, 405), TypeOfMove.SE, TypeOfPlayer.First));
                ball.Position = new Vector2(455, 405);
                return;
            }
            if (_movesHistory.Count == 3)
            {
                _movesHistory.Add(new GameMove(new Vector2(455, 405), new Vector2(400, 460), TypeOfMove.SW, TypeOfPlayer.Second));
                ball.Position = new Vector2(400, 460);
                _bounce.Visible = true;
                _bounce.Color = Color.Blue;
                _curent_move_time = 2 * MOVE_WAIT;
                return;
            }
            if (_movesHistory.Count == 4)
            {
                _movesHistory.Add(new GameMove(new Vector2(400, 460), new Vector2(345, 405), TypeOfMove.NW, TypeOfPlayer.Second));
                ball.Position = new Vector2(345, 405);
                return;
            }
            if (_movesHistory.Count == 5)
            {
                _movesHistory.Add(new GameMove(new Vector2(345, 405), new Vector2(400, 350), TypeOfMove.NE, TypeOfPlayer.First));
                ball.Position = new Vector2(400, 350);
                _bounce.Visible = true;
                _bounce.Color = Color.Red;
                _curent_move_time = 2 * MOVE_WAIT;
                return;
            }
            if (_movesHistory.Count == 6)
            {
                _movesHistory.Add(new GameMove(new Vector2(400, 350), new Vector2(455, 350), TypeOfMove.E, TypeOfPlayer.First));
                ball.Position = new Vector2(455, 350);
                return;
            }
            if (_movesHistory.Count == 7)
            {
                _movesHistory.Add(new GameMove(new Vector2(455, 350), new Vector2(455, 405), TypeOfMove.S, TypeOfPlayer.Second));
                ball.Position = new Vector2(455, 405);
                _bounce.Visible = true;
                _bounce.Color = Color.Blue;
                _curent_move_time = 2 * MOVE_WAIT;
                return;
            }
            if (_movesHistory.Count == 8)
            {
                _movesHistory.Add(new GameMove(new Vector2(455, 405), new Vector2(455, 460), TypeOfMove.S, TypeOfPlayer.Second));
                ball.Position = new Vector2(455, 460);
                _bounce.Visible = true;
                _bounce.Color = Color.Blue;
                _curent_move_time = 2 * MOVE_WAIT;
                return;
            }
            if (_movesHistory.Count == 9)
            {
                _movesHistory.Add(new GameMove(new Vector2(455, 460), new Vector2(400, 405), TypeOfMove.NW, TypeOfPlayer.Second));
                ball.Position = new Vector2(400, 405);
                return;
            }
            if (_movesHistory.Count == 10)
            {
                _movesHistory.Add(new GameMove(new Vector2(400, 405), new Vector2(455, 350), TypeOfMove.NE, TypeOfPlayer.First));
                ball.Position = new Vector2(455, 350);
                _bounce.Visible = true;
                _bounce.Color = Color.Red;
                _curent_move_time = 2 * MOVE_WAIT;
                return;
            }
            if (_movesHistory.Count == 11)
            {
                _movesHistory.Add(new GameMove(new Vector2(455, 350), new Vector2(455, 295), TypeOfMove.N, TypeOfPlayer.First));
                ball.Position = new Vector2(455, 295);
                _bounce.Visible = true;
                _bounce.Color = Color.Red;
                _curent_move_time = 2 * MOVE_WAIT;
                return;
            }
            if (_movesHistory.Count == 12)
            {
                _movesHistory.Add(new GameMove(new Vector2(455, 295), new Vector2(510, 295), TypeOfMove.E, TypeOfPlayer.First));
                ball.Position = new Vector2(510, 295);
                _curent_move_time = 3 * MOVE_WAIT;
                return;
            }
            if (_movesHistory.Count == 13)
            {
                _movesHistory.Clear();
                ball.Position = new Vector2(400, 240);
            }
        }

        private void PresentDeadlock()
        {
            if (_movesHistory.Count == 0)
            {
                _movesHistory.Add(new GameMove(ball.Position, new Vector2(345, 295), TypeOfMove.SW, TypeOfPlayer.Second));
                ball.Position = new Vector2(345, 295);
                return;
            }
            if (_movesHistory.Count == 1)
            {
                _movesHistory.Add(new GameMove(new Vector2(345, 295), new Vector2(400, 350), TypeOfMove.SE, TypeOfPlayer.First));
                ball.Position = new Vector2(400, 350);
                return;
            }
            if (_movesHistory.Count == 2)
            {
                _movesHistory.Add(new GameMove(new Vector2(400, 350), new Vector2(345, 405), TypeOfMove.SW, TypeOfPlayer.Second));
                ball.Position = new Vector2(345, 405);
                return;
            }
            if (_movesHistory.Count == 3)
            {
                _movesHistory.Add(new GameMove(new Vector2(345, 405), new Vector2(400, 460), TypeOfMove.SE, TypeOfPlayer.First));
                ball.Position = new Vector2(400, 460);
                return;
            }
            if (_movesHistory.Count == 4)
            {
                _movesHistory.Add(new GameMove(new Vector2(400, 460), new Vector2(455, 405), TypeOfMove.NE, TypeOfPlayer.First));
                ball.Position = new Vector2(455, 405);
                return;
            }
            if (_movesHistory.Count == 5)
            {
                _movesHistory.Add(new GameMove(new Vector2(455, 405), new Vector2(400, 405), TypeOfMove.W, TypeOfPlayer.Second));
                ball.Position = new Vector2(400, 405);
                return;
            }
            if (_movesHistory.Count == 6)
            {
                _movesHistory.Add(new GameMove(new Vector2(400, 405), new Vector2(400, 460), TypeOfMove.S, TypeOfPlayer.First));
                ball.Position = new Vector2(400, 460);
                _deadlock.Visible = true;
                _curent_move_time = 4 * MOVE_WAIT;
                return;
            }
            if (_movesHistory.Count == 7)
            {
                _movesHistory.Clear();
                ball.Position = new Vector2(400, 240);
                _deadlock.Visible = false;
            }
        }
        #endregion

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            spriteBatch.Draw(patternGoal, firstPlayerGOAL, Color.Red);
            spriteBatch.Draw(patternGoal, secondPlayerGOAL, Color.Blue);

            if (!_addingMoves)
                for (int i = 0; i < _movesHistory.Count; i++)
                {
                    GameMove actualMove = _movesHistory[i];
                    // tworzy przesuniêcie fla ka¿dego ruchu, rzy uzyciu statycznej klasy
                    Vector2 offset = Translator.TranslateDirectionToOffset(actualMove.Move);
                    offset *= _wallSize;
                    // przygotowuje prostok¹t do rysowania
                    Rectangle rectPosition = new Rectangle((int)actualMove.EndPosition.X + (int)offset.X,
                                                       (int)actualMove.EndPosition.Y + (int)offset.Y,
                                                       _wallSize,
                                                       _wallSize);
                    // ustawia defaultowy kolor, który NA PEWNO zostanie zmieniony
                    Color moveColor = Color.White;
                    // sprawdza czyj to ruch i wybiera kolor
                    if (actualMove.Player == TypeOfPlayer.First)
                    {
                        moveColor = Color.Red;
                    }
                    else
                    {
                        moveColor = Color.Blue;
                    }
                    // wybiera pozycje konkretnego ruchu z textury
                    Rectangle spritePosition = _moves.MovesSprites[Translator.TranslateMoveToSpriteType(actualMove.Move)];
                    // rysuje ruch
                    spriteBatch.Draw(_moves.Moves, rectPosition, spritePosition, moveColor);
                }


            ball.Draw(spriteBatch);
            foreach (UIElement element in _guideElements)
                element.Draw(spriteBatch);

            _fieldPicker.Draw(spriteBatch);
            _gameTypePicker.Draw(spriteBatch);

            spriteBatch.End();
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (input.Gestures.Count > 0)
            {
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    foreach (UIElement element in _guideElements)
                        element.HandleInput(input.Gestures[0].Position);
                    menu.WasPressed(input.Gestures[0].Position);

                    _fieldPicker.HandleInput(input.Gestures[0].Position);
                    _gameTypePicker.HandleInput(input.Gestures[0].Position);
                }
                if (input.Gestures[0].GestureType == GestureType.Flick)
                {
                    if (input.Gestures[0].Delta.X > 1000)
                    {
                        if (taps > 0)
                        {
                            taps--;
                            _curent_move_time = MOVE_WAIT;
                            SetProperGuidance();
                            AudioManager.PlaySound("selected");
                        }
                    }
                    if (input.Gestures[0].Delta.X < -1000)
                    {
                        if (taps < 5)
                        {
                            taps++;
                            _curent_move_time = MOVE_WAIT;
                            SetProperGuidance();
                            AudioManager.PlaySound("selected");
                        }
                    }
                }
            }
        }
        
        private void SetProperGuidance()
        {
            _movesHistory.Clear();
            ball.Position = new Vector2(400, 240);
            _previous.Visible = true;
            _next.Visible = true;
            if (_guideElements.Count > 3)
                for (int i = 3; i < _guideElements.Count; i++)
                    _guideElements[i].Visible = false;

            _fieldPicker.Visible = false;
            _gameTypePicker.Visible = false;

            if (taps == 0)
            {
                _previous.Visible = false;
                _borders.Visible = true;
                _playersGoal.Visible = true;
                _opponentsGoal.Visible = true;
                _startingPoint.Visible = true;
            }
            if (taps == 1)
            {
                _RulesLabel.Visible = true;
                _rulTakeTurns.Visible = true;
                _rulMoves.Visible = true;
                _rulExtraMove.Visible = true;
                _addingMoves = true;
                _addingMoves = false;
                _curent_move_time = MOVE_WAIT / 2;

            }
            if (taps == 2)
            {
                _ObjectivesLabel.Visible = true;
                _objGoal.Visible = true;
                _objBlock.Visible = true;
                _addingMoves = true;
                _addingMoves = false;
                _curent_move_time = MOVE_WAIT / 2;
            }
            if (taps == 3)
            {
                _tryItOut.Visible = true;
                _explainationMovesByGestures.Visible = true;
                _explainationMovesByArrows.Visible = true;
                _changeControlls.Visible = true;
            }
            if (taps == 4)
            {
                _gameTypePicker.Visible = true;
                _gameTypeExplaination.Visible = true;
                _fieldPicker.Visible = true;
                _fieldTypeExplaination.Visible = true;
            }
            if (taps == 5)
            {
                _selectTime.Visible = true;
                _selectTimeExplaination.Visible = true;
                _selectTimeArrow.Visible = true;
                _next.Visible = false; 
            }
        }
    }
}
