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
    public class Board : DrawableGameObject
    {
        Vector2[] winningPoints;

        public Vector2[] WinningPoints
        {
            get { return winningPoints; }
            set { winningPoints = value; }
        }
        Vector2[] borders;

        public Vector2[] Borders
        {
            get { return borders; }
            set { borders = value; }
        }
        Vector2[] stillSafePoints;

        public Vector2[] StillSafePoints
        {
            get { return stillSafePoints; }
            set { stillSafePoints = value; }
        }
        Vector2[] partiallySafePoints;

        public Vector2[] PartiallySafePoints
        {
            get { return partiallySafePoints; }
            set { partiallySafePoints = value; }
        }
        Vector2[] availableMoves;

        public Vector2[] AvailableMoves
        {
            get { return availableMoves; }
            set { availableMoves = value; }
        }

        List<TypeOfMove> validMovesInCurrentPosition;

        public List<TypeOfMove> ValidMovesInCurrentPosition
        {
            get { return validMovesInCurrentPosition; }
            set { validMovesInCurrentPosition = value; }
        }

        Vector2[] cornerPoints;

        public Vector2[] CornerPoints
        {
            get { return cornerPoints; }
            set { cornerPoints = value; }
        }

        List<GameMove> _movesHistory = new List<GameMove>();

        public List<GameMove> MovesHistory
        {
            get { return _movesHistory; }
            set { _movesHistory = value; }
        }

        Vector2 _actualBallPosition;

        public Vector2 ActualBallPosition
        {
            get { return _actualBallPosition; }
            set { _actualBallPosition = value; }
        }
        //bool extraMove = false;
        Texture2D coachPict;
        Texture2D patternGoal;
        Ball ball;
        MovesOnBoard moves;
        Texture2D _empty;
        Color firstPlayerColor;
        Color secondPlayerColor;

        Rectangle firstPlayerGOAL;
        Rectangle secondPlayerGOAL;

        Field fields;
        int wallSize;

        public int WallSize
        {
            get { return wallSize; }
            set { wallSize = value; }
        }

        public Board(string textureName)
        {
            this.TextureName = textureName;
            Origin = Vector2.Zero;
            LayerDepth = 1.0f;
            this.Color = Color.White;
            this.DestinationRectangle = new Rectangle(0, 0,
                                                      800,
                                                      480);

            fields = new Field();
            Field.CreateFields();

            wallSize = fields.GetWallSize();
            firstPlayerGOAL = fields.GetGoals()[0];
            secondPlayerGOAL = fields.GetGoals()[1];

            availableMoves = new Vector2[8];
            availableMoves[0] = new Vector2(0, wallSize);
            availableMoves[1] = new Vector2(0, -wallSize);
            availableMoves[2] = new Vector2(wallSize, 0);
            availableMoves[3] = new Vector2(-wallSize, 0);
            availableMoves[4] = new Vector2(wallSize, wallSize);
            availableMoves[5] = new Vector2(wallSize, -wallSize);
            availableMoves[6] = new Vector2(-wallSize, wallSize);
            availableMoves[7] = new Vector2(-wallSize, -wallSize);
            winningPoints = new Vector2[6];
            winningPoints = fields.GetWinningPoints();
            borders = new Vector2[4];
            borders = fields.GetBorders();
            cornerPoints = new Vector2[4];
            cornerPoints = fields.GetCornerPoints();
            stillSafePoints = new Vector2[2];
            stillSafePoints = fields.GetStillSafePoints();
            partiallySafePoints = new Vector2[4];
            partiallySafePoints = fields.GetPartiallySafePoints();
            ball = new Ball("ball");
            moves = new MovesOnBoard("Moves");
            firstPlayerColor = GameVariables.Instance.FirstPlayer.ShirtsColor;
            secondPlayerColor = GameVariables.Instance.SecondPlayer.ShirtsColor;
            _movesHistory = new List<GameMove>();
            if (GameVariables.Instance.PerformedMoves.Count == 0)
                GameVariables.Instance.PerformedMoves = _movesHistory;
            validMovesInCurrentPosition = CheckAvailableMoves();

            Reset();
        }

        public void Resuming()
        {
            firstPlayerColor = GameVariables.Instance.FirstPlayer.ShirtsColor;
            secondPlayerColor = GameVariables.Instance.SecondPlayer.ShirtsColor;
            _movesHistory = GameVariables.Instance.PerformedMoves;
            if (_movesHistory.Count > 0)
            {
                _actualBallPosition = _movesHistory[_movesHistory.Count - 1].EndPosition;
            }
            GameVariables.Instance.PerformedMoves = _movesHistory;
            ball.SetProperScale();
            CheckWhoHasMoveNow();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // rysuje t³o
            spriteBatch.Draw(ObjectTexture,
                             DestinationRectangle,
                             SourceRectangle,
                             Color,
                             Roation,
                             Origin,
                             SpriteEffects.None,
                             LayerDepth);
            // rusyje wszystkie ruchy na planszy
            for (int i = 0; i < MovesHistory.Count; i++)
            {
                GameMove actualMove = MovesHistory[i];
                // tworzy przesuniêcie fla ka¿dego ruchu, rzy uzyciu statycznej klasy
                Vector2 offset = Translator.TranslateDirectionToOffset(actualMove.Move);
                offset *= wallSize;
                // przygotowuje prostok¹t do rysowania
                Rectangle rectPosition = new Rectangle((int)actualMove.EndPosition.X + (int)offset.X,
                                                   (int)actualMove.EndPosition.Y + (int)offset.Y,
                                                   wallSize,
                                                   wallSize);
                // ustawia defaultowy kolor, który NA PEWNO zostanie zmieniony
                Color moveColor = Color.White;
                // sprawdza czyj to ruch i wybiera kolor
                if (actualMove.Player == TypeOfPlayer.First)
                {
                    moveColor = firstPlayerColor;
                }
                else
                {
                    moveColor = secondPlayerColor;
                }
                // wybiera pozycje konkretnego ruchu z textury
                Rectangle spritePosition = moves.MovesSprites[Translator.TranslateMoveToSpriteType(actualMove.Move)];
                // rysuje ruch
                spriteBatch.Draw(moves.Moves, rectPosition, spritePosition, moveColor);
            }

            // ustala pozycje pilki do jej œrodka
            ball.Position = new Vector2(_actualBallPosition.X,
                                         _actualBallPosition.Y );
            // rysuje pi³kê
            ball.Draw(spriteBatch);

            //rysuje wype³nienie bramek
            spriteBatch.Draw(patternGoal, firstPlayerGOAL, firstPlayerColor);
            spriteBatch.Draw(patternGoal, secondPlayerGOAL, secondPlayerColor);
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            this.ObjectTexture = contentManager.Load<Texture2D>(TextureName);
            ball.LoadTexture(contentManager);
            moves.LoadTexture(contentManager);
            _empty = contentManager.Load<Texture2D>("empty4x4");
            coachPict = contentManager.Load<Texture2D>("coach");
            patternGoal = contentManager.Load<Texture2D>("patternGoal");
        }

        public void LoadExtraFieldTexture(ContentManager contentManager)
        {
            string properTextureName = Field.GetPathToBackground();
            if (properTextureName != this.TextureName)
            {
                this.TextureName = properTextureName;
                this.ObjectTexture = contentManager.Load<Texture2D>(properTextureName);

                wallSize = fields.GetWallSize();
                firstPlayerGOAL = fields.GetGoals()[0];
                secondPlayerGOAL = fields.GetGoals()[1];

                availableMoves[0] = new Vector2(0, wallSize);
                availableMoves[1] = new Vector2(0, -wallSize);
                availableMoves[2] = new Vector2(wallSize, 0);
                availableMoves[3] = new Vector2(-wallSize, 0);
                availableMoves[4] = new Vector2(wallSize, wallSize);
                availableMoves[5] = new Vector2(wallSize, -wallSize);
                availableMoves[6] = new Vector2(-wallSize, wallSize);
                availableMoves[7] = new Vector2(-wallSize, -wallSize);

                winningPoints = fields.GetWinningPoints();
                borders = fields.GetBorders();
                cornerPoints = fields.GetCornerPoints();
                stillSafePoints = fields.GetStillSafePoints();
                partiallySafePoints = fields.GetPartiallySafePoints();
            }
        }

        // resetuje historiê ruchów, ustaiw pozycjê wyjœciow¹ pi³ki, i gwi¿dŸe
        public void Reset()
        {
            _movesHistory.Clear();
            _actualBallPosition = new Vector2(400, 240);
            //_actualBallPosition = new Vector2(565, 405);
            //_movesHistory.Add(new GameMove(new Vector2(510, 295), new Vector2(565, 350), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(565, 350), new Vector2(565, 405), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(510, 350), new Vector2(565, 405), TypeOfMove.SE, TypeOfPlayer.First));

            //_movesHistory.Add(new GameMove(new Vector2(510, 405), new Vector2(565, 460), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(510, 405), new Vector2(565, 405), TypeOfMove.E, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(510, 405), new Vector2(510, 460), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(510, 460), new Vector2(565, 405), TypeOfMove.NE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(565, 405), new Vector2(565, 460), TypeOfMove.S, TypeOfPlayer.First));

            //_movesHistory.Add(new GameMove(new Vector2(455, 350), new Vector2(510, 405), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 350), new Vector2(510, 350), TypeOfMove.E, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 350), new Vector2(455, 405), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 405), new Vector2(510, 350), TypeOfMove.NE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(510, 350), new Vector2(510, 405), TypeOfMove.S, TypeOfPlayer.First));

            //_movesHistory.Add(new GameMove(new Vector2(455, 295), new Vector2(510, 350), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 295), new Vector2(510, 295), TypeOfMove.E, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 295), new Vector2(455, 350), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 350), new Vector2(510, 295), TypeOfMove.NE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(510, 295), new Vector2(510, 350), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 460), new Vector2(510, 405), TypeOfMove.NE, TypeOfPlayer.First));

            //_movesHistory.Add(new GameMove(new Vector2(400, 350), new Vector2(455, 405), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 350), new Vector2(455, 350), TypeOfMove.E, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 350), new Vector2(400, 405), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 405), new Vector2(455, 350), TypeOfMove.NE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 350), new Vector2(455, 405), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 405), new Vector2(455, 460), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 405), new Vector2(455, 405), TypeOfMove.E, TypeOfPlayer.First));

            //_movesHistory.Add(new GameMove(new Vector2(400, 295), new Vector2(455, 350), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 295), new Vector2(455, 295), TypeOfMove.E, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 295), new Vector2(400, 350), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 350), new Vector2(455, 295), TypeOfMove.NE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(455, 295), new Vector2(455, 350), TypeOfMove.S, TypeOfPlayer.First));

            //_movesHistory.Add(new GameMove(new Vector2(345, 350), new Vector2(400, 405), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 350), new Vector2(400, 350), TypeOfMove.E, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 350), new Vector2(345, 405), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 405), new Vector2(400, 350), TypeOfMove.NE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 350), new Vector2(400, 405), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 405), new Vector2(400, 460), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 405), new Vector2(400, 405), TypeOfMove.E, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 460), new Vector2(400, 405), TypeOfMove.NE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 405), new Vector2(400, 460), TypeOfMove.S, TypeOfPlayer.First));

            //_movesHistory.Add(new GameMove(new Vector2(345, 295), new Vector2(400, 350), TypeOfMove.SE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 295), new Vector2(400, 295), TypeOfMove.E, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 295), new Vector2(345, 350), TypeOfMove.S, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(345, 350), new Vector2(400, 295), TypeOfMove.NE, TypeOfPlayer.First));
            //_movesHistory.Add(new GameMove(new Vector2(400, 295), new Vector2(400, 350), TypeOfMove.S, TypeOfPlayer.First));

            //if (GameVariables.Instance.CurrentPlayer.Coach == TeamCoach.CPU)
            //    GameVariables.Instance.NextMoveForFirstPlayer();
            validMovesInCurrentPosition = CheckAvailableMoves();
        }
        
        // ustala w jakim kierunku chce poruszaæ siê user
        private TypeOfMove DetermineTypeOfMove(Vector2 position)
        {
            Vector2 delta = position - _actualBallPosition;
            return GameRules.DetermineTypeOfMoveFromDelta(delta);
        }
        // funkcja która dodaje ruch wykonany przez cpu
        /// <summary>
        /// Adds move in the place where ball actually lies.
        /// </summary>
        /// <param name="move">Type of the move.</param>
        /// <returns>0 when move cannot be added, 1 when move was added and player connot bounce, 2 when move was added and player can move further (bounces)</returns>
        public int AddMove(TypeOfMove move)
        {
            return AddMove(_actualBallPosition, move);
        }

        // dodaje ruch do historii
        /// <summary>
        /// Adds move in the specified position of Vector2.
        /// </summary>
        /// <param name="position">Position where move will be added.</param>
        /// <param name="move">Type of the move</param>
        /// <returns>0 when move cannot be added, 1 when move was added and player connot bounce, 2 when move was added and player can move further (bounces)</returns>
        public int AddMove(Vector2 position, TypeOfMove move)
        {
            int finalResult = 0;
            if (move == TypeOfMove.UNKNOWN)
            {
                move = DetermineTypeOfMove(position);
            }

            // jesli ci¹gle jest nieznany typ
            if (move == TypeOfMove.UNKNOWN)
                return finalResult;

            if (!ObeyedRules(move))
                return finalResult;

            switch (move)
            {
                case TypeOfMove.N:
                    position = _actualBallPosition + new Vector2(0, -wallSize);
                    break;
                case TypeOfMove.NE:
                    position = _actualBallPosition + new Vector2(wallSize, -wallSize);
                    break;
                case TypeOfMove.E:
                    position = _actualBallPosition + new Vector2(wallSize, 0);
                    break;
                case TypeOfMove.SE:
                    position = _actualBallPosition + new Vector2(wallSize, wallSize);
                    break;
                case TypeOfMove.S:
                    position = _actualBallPosition + new Vector2(0, wallSize);
                    break;
                case TypeOfMove.SW:
                    position = _actualBallPosition + new Vector2(-wallSize, wallSize);
                    break;
                case TypeOfMove.W:
                    position = _actualBallPosition + new Vector2(-wallSize, 0);
                    break;
                case TypeOfMove.NW:
                    position = _actualBallPosition + new Vector2(-wallSize, -wallSize);
                    break;
            }
            if (!PathClear(_actualBallPosition, position))
                return finalResult;

            TypeOfPlayer player = TypeOfPlayer.First;
            if (GameVariables.Instance.FirstPlayer.HaveMoveNow)
                player = TypeOfPlayer.First;
            else
                player = TypeOfPlayer.Second;

            // jesli moze sie odbic to znaczy, ¿e ten gracz nastepnym razem dostanie extra ruch i nie zmieniany gracza
            if (!CanItBounced(position))
            {

                if (player == TypeOfPlayer.First)                       // jesli teraz ruch wykona³ pierwszy
                    GameVariables.Instance.NextMoveForSecondPlayer();   // to teraz wykonuje ruch grugi
                else
                    GameVariables.Instance.NextMoveForFirstPlayer();
                bounceCounter = 0;
                finalResult = 1;
            }
            else
            {
                /*
                 * Jeœli gra siê z CPU lub przez Wifi, a obecnym graczem jest cz³owiek
                 */
                if (GameVariables.Instance.CurrentPlayer.Coach == TeamCoach.HUMAN &&
                    GameVariables.Instance.SecondPlayer.Coach == TeamCoach.CPU)
                {
                    bounceCounter++;
                    Statistics.Instance.WielokrotneOdbicie(bounceCounter);
                    finalResult = 2;
                }
                if (GameVariables.Instance.FirstPlayer.HaveMoveNow && GameVariables.Instance.SecondPlayer.Coach == TeamCoach.REMOTEOPPONENT ||
                    GameVariables.Instance.SecondPlayer.HaveMoveNow && GameVariables.Instance.FirstPlayer.Coach == TeamCoach.REMOTEOPPONENT)
                {
                    bounceCounter++;
                    Statistics.Instance.WielokrotneOdbicie(bounceCounter);
                    finalResult = 2;
                }
                finalResult = 2;
            }

            _movesHistory.Add(new GameMove(_actualBallPosition, position, move, player));
            _actualBallPosition = position;
            validMovesInCurrentPosition = CheckAvailableMoves();
            return finalResult;
        }

        int bounceCounter = 0;
        
        // sprawdza czy nie jest sie na lini i chce sie po niej graæ
        // lub czy nie jest sie na œrodku bramki, slupkach, albo w bramce
        private bool ObeyedRules(TypeOfMove nextMove)
        {
            if (_actualBallPosition == stillSafePoints[0] || _actualBallPosition == stillSafePoints[1])
                return true;
            if (_actualBallPosition == winningPoints[0] || _actualBallPosition == winningPoints[1])
                return true;
            if (_actualBallPosition == partiallySafePoints[0] && (nextMove != TypeOfMove.N && nextMove != TypeOfMove.NW && nextMove != TypeOfMove.W))
                return true;
            if (_actualBallPosition == partiallySafePoints[1] && (nextMove != TypeOfMove.S && nextMove != TypeOfMove.SW && nextMove != TypeOfMove.W))
                return true;
            if (_actualBallPosition == partiallySafePoints[2] && (nextMove != TypeOfMove.N && nextMove != TypeOfMove.NE && nextMove != TypeOfMove.E))
                return true;
            if (_actualBallPosition == partiallySafePoints[3] && (nextMove != TypeOfMove.S && nextMove != TypeOfMove.SE && nextMove != TypeOfMove.E))
                return true;

            bool forbidenMove = false;
            if (_actualBallPosition.Y == borders[0].Y)
                if (nextMove != TypeOfMove.S && nextMove != TypeOfMove.SE && nextMove != TypeOfMove.SW)
                    forbidenMove = true;
            if (_actualBallPosition.Y == borders[1].Y)
                if (nextMove != TypeOfMove.N && nextMove != TypeOfMove.NE && nextMove != TypeOfMove.NW)
                    forbidenMove = true;
            if (_actualBallPosition.X == borders[2].X)
                if (nextMove == TypeOfMove.E || nextMove == TypeOfMove.NE || nextMove == TypeOfMove.SE)
                    forbidenMove = false;
                else
                    forbidenMove = true;
            if (_actualBallPosition.X == borders[3].X)
                if (nextMove == TypeOfMove.W || nextMove == TypeOfMove.NW || nextMove == TypeOfMove.SW)
                    forbidenMove = false;
                else
                    forbidenMove = true;

            return !forbidenMove;
        }

        // sprawdza czy œcie¿ka jest juz zajêta
        private bool PathClear(Vector2 start, Vector2 end)
        {
            bool clear = true;
            for (int i = 0; i < _movesHistory.Count; i++)
            {
                if (_movesHistory[i].StartPosition == start && _movesHistory[i].EndPosition == end)
                    clear = false;
                if (_movesHistory[i].StartPosition == end && _movesHistory[i].EndPosition == start)
                    clear = false;
            }
            return clear;
        }

        public bool CpuCanItBounced(TypeOfMove move)
        {
            return CanItBounced(_actualBallPosition + (Translator.TranslateDirectionToVector(move) * wallSize));
        }

        // sprawdza czy sie odbije i dostanie dodatkowy ruch
        /// <summary>
        /// Sprawdza czy mo¿na siê dalej odbiæ z podanego punktu.
        /// </summary>
        /// <param name="end">Punkt który bêdzie sprawdzany, czy istnieje z niego mo¿liwoœc odbicia.</param>
        /// <returns>Prawda jeœli mozna siê odbiæ, lub fa³sz jeœli nie mo¿na siê dalej odbijaæ.</returns>
        private bool CanItBounced(Vector2 end)
        {
            for (int i = 0; i < _movesHistory.Count; i++)
            {
                if (_movesHistory[i].StartPosition == end || _movesHistory[i].EndPosition == end)
                    return true;
            }

            if (end == stillSafePoints[0] || end == stillSafePoints[1])
                return false;

            if (end.Y == borders[0].Y || end.Y == borders[1].Y ||
                end.X == borders[2].X || end.X == borders[3].X)
                return true;

            return false;
        }
        /// <summary>
        /// Sprawdza, czy mo¿na siê odbic z ostatniego ruchu jaki zosta³ zrobiony.
        /// </summary>
        /// <returns>Prawda jeœli mozna sie odbiæ, fa³sz jeœli nie mo¿na siê ju¿ dalej odbijaæ.</returns>
        private bool CanItBounced()
        {
            GameMove lastMove = _movesHistory.Last();
            for (int i = 0; i < _movesHistory.Count; i++)
            {
                if (_movesHistory[i] == lastMove)
                    continue;

                if (_movesHistory[i].StartPosition == lastMove.EndPosition || _movesHistory[i].EndPosition == lastMove.EndPosition)
                    return true;

                if (lastMove.EndPosition == stillSafePoints[0] || lastMove.EndPosition == stillSafePoints[1])
                    return false;

                if (lastMove.EndPosition.Y == borders[0].Y || lastMove.EndPosition.Y == borders[1].Y ||
                    lastMove.EndPosition.X == borders[2].X || lastMove.EndPosition.X == borders[3].X)
                    return true;
            }
            return false;
        }

        // sprawdza jakie ruchy mozna jeszcze wykonaæ -> do zrobienia jest sprawdzanie legalnoœci tych ruchów
        /// <summary>
        /// Sprawdza jakie ruchy mozna wykonaæ z pozycji gdzie znajduje sie pi³ka
        /// </summary>
        /// <returns>Lista dotepnych ruchów.</returns>
        public List<TypeOfMove> CheckAvailableMoves()
        {
            List<TypeOfMove> list = new List<TypeOfMove>();
            TypeOfMove move = TypeOfMove.UNKNOWN;
            for (int i = 0; i < availableMoves.Length; i++)
            {
                move = Translator.TranslateVectorToMove(availableMoves[i]);
                Vector2 desiredMove = _actualBallPosition + availableMoves[i];
                if (PathClear(_actualBallPosition, desiredMove) && ObeyedRules(move))
                    list.Add(move);
            }
            return list;
        }

        public void CheckWhoHasMoveNow()
        {
            if (_movesHistory.Count == 0)
                return;

            TypeOfPlayer ownerOfLastMove = _movesHistory.Last().Player;

            if (CanItBounced())     // jeœli mozna sie odbiæ
            {
                    if (ownerOfLastMove == TypeOfPlayer.First)              // i ostatni ruch wykonywa³ gracz pierwszy
                        GameVariables.Instance.NextMoveForFirstPlayer();    // to teraz dalej on wykonuje ruch
                    else                                                    // w przeciwnym razie jeœli ostatni ruch wykona³ gracz drugi
                        GameVariables.Instance.NextMoveForSecondPlayer();   // to dalej on wykonuje ruch
            }
            else                    // jeœli nie mo¿na siê odbiæ
            {
                if (ownerOfLastMove == TypeOfPlayer.First)              // i ostatni ruch wykonywa³ gracz pierwszy
                    GameVariables.Instance.NextMoveForSecondPlayer();   // to teraz gracz DRUGI wykonuje ruch
                else                                                    // w przeciwnym razie jeœli ostatni ruch wykona³ gracz drugi
                    GameVariables.Instance.NextMoveForFirstPlayer();    // to teraz gracz pierwszy ma ruch
            }
        }
    }
}
