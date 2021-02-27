using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace TableGoal
{
    public enum DifficultyLevel
    {
        EASY,
        MEDIUM,
        HARD
    }

    public class AIPlayer
    {
        private enum MOVE
        {
            MIN,
            MAX
        }

        private Board board;
        private int BOUNCING_COUNTER = 0;
        private int MAXIMUM_BOUNCES = 1;
        private int HORIZON = 0;
        private readonly int WINNING_MOVE = 1000000;
        private readonly int LOSSING_MOVE = -1000000;
        private readonly int COST_OF_DISTANCE = 100;
        //private readonly int BONUS_FOR_BOUNCE = 50;
        private readonly int _3rdLINE_OF_DEFENCE = 500000;
        private readonly int _2ndLINE_OF_DEFENCE = 250000;
        private readonly int _1stLINE_OF_DEFENCE = 100000;
        Dictionary<TypeOfMove, int> movesValues;
        BackgroundWorker thinker;
        private bool thinkingWasCanceled = false;
        Random rand;

        public event AiFinishedTurnEventHandler AiFinishedTurn;
        public delegate void AiFinishedTurnEventHandler();

        
        public bool isThinking()
        {
            return thinker.IsBusy;
        }

        public void CancelMove()
        {
            thinker.CancelAsync();
            thinkingWasCanceled = true;
        }

        public void CheckDifficultyLevel()
        {
            switch (GameVariables.Instance.DiffLevel)
            {
                case DifficultyLevel.EASY:
                    MAXIMUM_BOUNCES = 1;
                    HORIZON = 0;
                    break;
                case DifficultyLevel.MEDIUM:
                    MAXIMUM_BOUNCES = 3;
                    HORIZON = 0;
                    break;
                case DifficultyLevel.HARD:
                    MAXIMUM_BOUNCES = 2;
                    HORIZON = 1;
                    break;
                default:
                    break;
            }
        }

        public AIPlayer(ref Board board)
        {
            this.board = board;
            thinker = new BackgroundWorker();
            thinker.DoWork += new DoWorkEventHandler(thinker_DoWork);
            thinker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(thinker_RunWorkerCompleted);
            thinker.WorkerSupportsCancellation = true;
            CheckDifficultyLevel();
            thinkingWasCanceled = false;
            rand = new Random(DateTime.Now.Millisecond);
        }

        private bool CheckWhetherMoveIsWinning(Vector2 endPosition)
        {
            if (board.WinningPoints[0] == endPosition ||
                board.WinningPoints[1] == endPosition ||
                board.WinningPoints[2] == endPosition)
                return true;
            if (board.PartiallySafePoints[0] == endPosition ||
                board.PartiallySafePoints[1] == endPosition)
                return true;
            return false;
        }

        private int GetMeRandomFromZeroToInt(int Upperlimit)
        {
            int multiplier = 10000;
            double rezult = 0;
            rezult = rand.Next(Upperlimit * multiplier) / multiplier;
            int rezultINT = (int)rezult;
            return rezultINT;
        }

        private bool CheckWhetherMoveIsLossing(Vector2 endPosition, MOVE minmax)
        {
            if (board.WinningPoints[3] == endPosition ||
                board.WinningPoints[4] == endPosition ||
                board.WinningPoints[5] == endPosition)
                return true;
            if (board.CornerPoints.Contains(endPosition))
                return true;
            if (minmax == MOVE.MIN)
                if (board.PartiallySafePoints[2] == endPosition ||
                    board.PartiallySafePoints[3] == endPosition)
                    return true;
            return false;
        }

        public void MakeMove()
        {
            /*
             *  1. czy gol
             *  2. czy strata bramki
             *  3. w przeciwnym wypadku = odl * mno¿nik + 50 (jeœli nie mo¿na sie odbiæ) +
             *                          + d³ugoœæ ruchu (jeœli na po³owie przeciwnika)
             */

            thinker.RunWorkerAsync();
        }

        void thinker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // sprawdzenie czy nie pojawi³ sie jakiœ problem
            if (e.Error != null)
            {
                return;
            }
            // jeœli proces zosta³ cancelowany to wychodzimy
            if (e.Cancelled || thinkingWasCanceled)
            {
                thinkingWasCanceled = false;
                return;
            }
            List<TypeOfMove> availableMoves =new List<TypeOfMove>();
            // wybraæ najlepszy teraz
            TypeOfMove THEBEST = TypeOfMove.UNKNOWN;
            int BiggestValue = LOSSING_MOVE * 2;
            int tempVal = 0;
            for (int i = 0; i < movesValues.Keys.Count; i++)
            {
                tempVal = movesValues[movesValues.Keys.ElementAt(i)];
                if (tempVal == movesValues.Values.Max())
                {
                    availableMoves.Add(movesValues.Keys.ElementAt(i));
                }
                if (tempVal >= BiggestValue)
                {
                    THEBEST = movesValues.Keys.ElementAt(i);
                    BiggestValue = tempVal;
                }
            }
            var sortedMovesByValue = (from entry in movesValues
                                      orderby entry.Value
                                      descending
                                      select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
#if DEBUG
            foreach (TypeOfMove move in sortedMovesByValue.Keys)
            {
                Debug.WriteLine(String.Format("Key {0}, Value {1}", move.ToString(), movesValues[move].ToString()));
            }
#endif

            List<TypeOfMove> equalMoves = new List<TypeOfMove>();
            for (int i = 0; i < sortedMovesByValue.Values.Count; i++)
            {
                if (sortedMovesByValue.Values.ElementAt(i) == BiggestValue)
                {
                    equalMoves.Add(sortedMovesByValue.Keys.ElementAt(i));
                }
            }


            if (equalMoves.Count > 1)
            {
                int index = GetMeRandomFromZeroToInt(equalMoves.Count - 1);
                THEBEST = equalMoves[index];
                Debug.WriteLine(String.Format("\n\n\t\t----- !! MORE MOVES !! -----\n\n\n"));
            }

            if (board.AddMove(THEBEST) == 1)
            {
                if (AiFinishedTurn != null)
                {
                    AiFinishedTurn();
                }
            }
            AudioManager.PlaySound("kick");
            BOUNCING_COUNTER = 0;
            movesValues.Clear();
        }

        void thinker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (thinkingWasCanceled)
                e.Cancel = true;
            
            List<GameMove> totalMovesHistory = board.MovesHistory;
            Vector2 actualBallPosition = board.ActualBallPosition;
            List<TypeOfMove> availableMoves = board.CheckAvailableMoves();
            
            movesValues = new Dictionary<TypeOfMove, int>();
            /*
             * Dla ruchu MAX wykonujemy wszystkie mo¿liwe ruchy.
             * Otrzymujemy (z rozwiniêcia drzewa) ich wartoœci i wybieramy maksymaln¹.
             * 
             */
            for (int i = 0; i < availableMoves.Count; i++)
            {
                int value = MakeAndEvaluateMove(totalMovesHistory, actualBallPosition, availableMoves[i], MOVE.MAX, HORIZON);
                movesValues.Add(availableMoves[i], value);
                Debug.WriteLine(String.Format("Ruch {0} jest wart {1}", availableMoves[i].ToString(), value.ToString()));
            }
        }


        /*
         * Gdy ruch ma MAX to wybiera on taki ruch, który daje mu najwiêksz¹ wartoœæ.
         * Po ruchu MAXa nastêpujê ruch MINa. MIN zawsze wykona najmniej korzystny ruch (z punktu widzenia MAXA)
         * a zarazem najlepszy dla siebie - generalnie MIN wybierze ruch o najmniejszej wartoœci punktów.
         * Oceniamy planszê zawsze z punktu widzenia MAXa. St¹d oceniaj¹c planszê lepiej dla MAXa, tym samym
         * sytuacja MINa ulega pogorszeniu. Dlatego MIN zawsze bêdzie wybiera³ takie ruchy gdzie wartoœæ dla 
         * MAXa jest najmniejsza !!
         */
        private int MakeAndEvaluateMove(List<GameMove> history, Vector2 ball, TypeOfMove move, MOVE minmax, int horizon)
        {
            if (thinkingWasCanceled)
            {
                return 0;
            }
            Debug.WriteLine(String.Format("Ruch {0} dla {1}", move.ToString(), minmax.ToString()));
            Debug.WriteLine(String.Format("\t\thorizon = {0}, \n\t\t\tBOUNCE_COUNTER = {1}", horizon, BOUNCING_COUNTER));
            int VALUE = 0;
            // tworzy now¹ historiê ruchów
            history = ReturnNewHistoryAfterMove(history, move, ball);
            // pobiera nowa pozycje pi³ki
            Vector2 newBallPosition = history[history.Count - 1].EndPosition;

            /*
             * Sprawdzanie czy punkt jest wygrywaj¹cy, czy przegrywaj¹cy odbywa siê tutaj,
             * a nie w funkcji oceniaj¹cej (EvaluateBall) poniewa¿ gdy wiemy czy wygrywamy, czy przegrywamy
             * nie musimy wchodzic dalej w funkcjê i od razu mo¿emy zwróciæ wartoœci.
             * wracamy z wartoœci¹ WINNING_MOVE tylko jeœli to jest ruch dla MAXa w przeciwnym wypadku by³ BUG!!
             * bo zak³adaliœmy, ¿e MIN zrobi tuch po którym bêdziemy mieæ punkt.
             */
            if (CheckWhetherMoveIsWinning(newBallPosition) && minmax == MOVE.MAX)
            {
                Debug.WriteLine(String.Format("\t WYGRANA !!!!"));
                VALUE = WINNING_MOVE;
                return VALUE;
            }

            if (CheckWhetherMoveIsLossing(newBallPosition, minmax))
            {
                    Debug.WriteLine(String.Format("\t PRZEGRANA !!!!!!!!!!"));
                    VALUE = LOSSING_MOVE;
                    return VALUE;
            }
            
            // jeœli po tym ruchu nie ma juz dostêpnych wiêcej ruchów -> BLOK
            // to MAX przegra³ -> MIN zawsze bêdzie chcia³ wybraæ t¹ wartoœæ, równ¹ LOSSING_MOVE
            List<TypeOfMove> availableMoves = CheckAvailableMoves(history, newBallPosition);
            if (availableMoves.Count == 0)
            {
                Debug.WriteLine(String.Format("\t BLOK"));
                VALUE = LOSSING_MOVE;
                return VALUE;
            }

            // sprawdzamy czy mo¿emy sie odbiæ w pozycji w jakiej siê znajdujemy
            bool extraMove = CanItBounced(newBallPosition, history);
             
            if (BOUNCING_COUNTER == MAXIMUM_BOUNCES) { extraMove = false; }

            MOVE whosNext = MOVE.MIN;
            switch (minmax)
            {
                case MOVE.MIN:
                    whosNext = MOVE.MAX;
                    break;
                case MOVE.MAX:
                    whosNext = MOVE.MIN;
                    break;
            }
            // jeœli mozna siê odbiæ
            // czyli ma siê dodatkowy ruch i !
            // iloœæ odbiæ do tej pory jest mniejsza od maksymalnej iloœci odbiæ
            if (extraMove && BOUNCING_COUNTER < MAXIMUM_BOUNCES)
            {
                Debug.WriteLine(String.Format("\t-> !! Odbijam siê."));
                ++BOUNCING_COUNTER;
                whosNext = minmax;
                List<TypeOfMove> nextRoundAvailableMoves = availableMoves;
                // jeœli ruch wykonuje MAX, to bêdzie chcia³ znaleŸæ wartoœæ maksymaln¹
                int maximumOrMinimumValue = LOSSING_MOVE * 2;
                if (minmax == MOVE.MIN) // jeœli ruch wykonuje MIN to bêdzie chcia³ znaleŸæ wartoœæ minimaln¹
                    maximumOrMinimumValue = WINNING_MOVE * 2;
                int tempValue = 0;

                for (int i = 0; i < nextRoundAvailableMoves.Count; i++)
                {
                    tempValue = MakeAndEvaluateMove(history, newBallPosition, nextRoundAvailableMoves[i], whosNext, horizon);
                    switch (minmax)
                    {
                        case MOVE.MIN:
                            if (tempValue <= maximumOrMinimumValue)
                            {
                                maximumOrMinimumValue = tempValue;
                            }
                            break;
                        case MOVE.MAX:
                            if (tempValue >= maximumOrMinimumValue)
                            {
                                maximumOrMinimumValue = tempValue;
                            }
                            break;
                    }
                }

                Debug.WriteLine(String.Format("\t-> !! Wracam z odbijania"));
                //int Score = maximumOrMinimumValue + BOUNCING_COUNTER * BONUS_FOR_NO_BOUNCING;
                int Score = maximumOrMinimumValue;
                if (newBallPosition.X <= 400)
                    Score += BOUNCING_COUNTER * COST_OF_DISTANCE / 2;
                //int extraCost = IsAlmostGoalInTheRight(newBallPosition);
                //Score -= extraCost;

                BOUNCING_COUNTER--;
                return Score;
            }
            
            // jeœli po tym ruchu nie przegrywamy, ani nie wygrywamy,
            // ani nie mamy dodatkowego ruchu to oceniamy planszê
            VALUE = EvaluateBall(newBallPosition);

            if (horizon == 0)
            {
                /*
                 * jeœli jesteœmy blisko bramki AI to wtedy sprawdzamy jak blisko i odpowiednio oceniamy.
                 * Im bli¿ej bramki tym wiêcej minusowych punktów (bo odejmujemy wartoœæ któr¹ dostaniemy) dostajemy do pozycji.
                 */
                int extraCost = IsAlmostGoalInTheRight(newBallPosition);
                VALUE -= extraCost;
                Debug.WriteLine(String.Format("<-Wracam z {0}", VALUE));
                return VALUE;
            }
            else
            {
                horizon--;
                if (horizon >= 0)
                {
                    List<TypeOfMove> nextRoundAvailableMoves = availableMoves;
                    int maximumOrMinimumValue = LOSSING_MOVE * 2;
                    if (minmax == MOVE.MIN)
                        maximumOrMinimumValue = WINNING_MOVE * 2;
                    int tempValue = 0;

                    for (int i = 0; i < nextRoundAvailableMoves.Count; i++)
                    {
                        tempValue = MakeAndEvaluateMove(history, newBallPosition, nextRoundAvailableMoves[i], whosNext, horizon);
                        switch (minmax)
                        {
                            case MOVE.MIN:
                                if (tempValue <= maximumOrMinimumValue)
                                {
                                    maximumOrMinimumValue = tempValue;
                                }
                                break;
                            case MOVE.MAX:
                                if (tempValue >= maximumOrMinimumValue)
                                {
                                    maximumOrMinimumValue = tempValue;
                                }
                                break;
                        }
                    }

                    VALUE = maximumOrMinimumValue;
                }
            }

            VALUE -= IsAlmostGoalInTheRight(newBallPosition);
            
            return VALUE;
        }

        private int IsAlmostGoalInTheRight(Vector2 actualBallPosition)
        {
            int cost = 0;
            /*
             * ostatnia, trzecia linia obrony
             */
            if (actualBallPosition == board.StillSafePoints[1])     // na œrodku bramki
                cost = _3rdLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.StillSafePoints[1].X - board.WallSize &&  // centralnie przed bramk¹
                actualBallPosition.Y == board.StillSafePoints[1].Y)
                cost = _3rdLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[2].X - board.WallSize &&  // z lewej Górnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[2].Y)
                cost = _3rdLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[3].X - board.WallSize &&  // z lewej Dolnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[3].Y)
                cost = _3rdLINE_OF_DEFENCE;

            /*
             * druga linia obrony
             */
            if (actualBallPosition.X == board.StillSafePoints[1].X - 2 * board.WallSize &&  // centralnie przed bramk¹ o dwa pola
                actualBallPosition.Y == board.StillSafePoints[1].Y)
                cost = _2ndLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[2].X - board.WallSize &&  // z lewej, w górê od Górnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[2].Y - board.WallSize)
                cost = _2ndLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[2].X - 2 * board.WallSize &&  // 2x z lewej Górnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[2].Y)
                cost = _2ndLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[3].X - 2 * board.WallSize &&  // 2x z lewej Dolnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[3].Y)
                cost = _2ndLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[3].X - board.WallSize &&  // z lewej, w dó³ Dolnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[3].Y + board.WallSize)
                cost = _2ndLINE_OF_DEFENCE;

            /*
             * Pierwsza linia obrony
             */
            if (actualBallPosition.X == board.PartiallySafePoints[2].X - board.WallSize &&  // z lewej, 2x w górê od Górnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[2].Y - 2 * board.WallSize)
                cost = _1stLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[3].X - 2 * board.WallSize &&  // 2x z lewej, 2x w górê Dolnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[3].Y - 2 * board.WallSize)
                cost = _1stLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[2].X - 2 * board.WallSize &&  // 2x z lewej, w górê od Górnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[2].Y - board.WallSize)
                cost = _1stLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[3].X - 2 * board.WallSize &&  // 2x z lewej, w dó³ Dolnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[3].Y + board.WallSize)
                cost = _1stLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[3].X - 2 * board.WallSize &&  // 2x z lewej, 2x w dó³ Dolnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[3].Y + 2 * board.WallSize)
                cost = _1stLINE_OF_DEFENCE;
            if (actualBallPosition.X == board.PartiallySafePoints[3].X - board.WallSize &&  // z lewej, 2x w dó³ Dolnego naro¿nika
                actualBallPosition.Y == board.PartiallySafePoints[3].Y + 2 * board.WallSize)
                cost = _1stLINE_OF_DEFENCE;

            return cost;
        }

        private int EvaluateBall(Vector2 actualBallPosition)
        {
            double ValueToCast = 800 - Math.Sqrt(Math.Pow(actualBallPosition.X - board.WinningPoints[1].X, 2.0) +
                                             Math.Pow(actualBallPosition.Y - board.WinningPoints[1].Y, 2.0));
            ValueToCast = ValueToCast * COST_OF_DISTANCE / board.WallSize;
            int value = (int)ValueToCast;
            int variesDecision = GetMeRandomFromZeroToInt(1000); // getting desicion 
            if (variesDecision <= 550)// if decision value is less or equal to 550 
            {
                value += GetMeRandomFromZeroToInt(100); // then add extra random value
            }
            return value;
        }

        private List<GameMove> ReturnNewHistoryAfterMove(List<GameMove> histoty, TypeOfMove move, Vector2 ballPos)
        {
            Vector2 position = ReturnNewBallPositionAfterMove(move, ballPos);
            TypeOfPlayer player = TypeOfPlayer.First;
            List<GameMove> nHistory = histoty.ToList();
            nHistory.Add(new GameMove(ballPos, position, move, player));
            return nHistory;
        }

        /// <summary>
        /// Zwraca now¹ pozycjê pi³ki po wykonaniu ruchu.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="actualBallPosition"></param>
        /// <returns></returns>
        private Vector2 ReturnNewBallPositionAfterMove(TypeOfMove move, Vector2 actualBallPosition)
        {
            Vector2 position = new Vector2();
            switch (move)
            {
                case TypeOfMove.N:
                    position = actualBallPosition + new Vector2(0, -board.WallSize);
                    break;
                case TypeOfMove.NE:
                    position = actualBallPosition + new Vector2(board.WallSize, -board.WallSize);
                    break;
                case TypeOfMove.E:
                    position = actualBallPosition + new Vector2(board.WallSize, 0);
                    break;
                case TypeOfMove.SE:
                    position = actualBallPosition + new Vector2(board.WallSize, board.WallSize);
                    break;
                case TypeOfMove.S:
                    position = actualBallPosition + new Vector2(0, board.WallSize);
                    break;
                case TypeOfMove.SW:
                    position = actualBallPosition + new Vector2(-board.WallSize, board.WallSize);
                    break;
                case TypeOfMove.W:
                    position = actualBallPosition + new Vector2(-board.WallSize, 0);
                    break;
                case TypeOfMove.NW:
                    position = actualBallPosition + new Vector2(-board.WallSize, -board.WallSize);
                    break;
            }
            return position;
        }

        // sprawdza czy nie jest sie na lini i chce sie po niej graæ
        // lub czy nie jest sie na œrodku bramki, slupkach, albo w bramce
        private bool ObeyedRules(TypeOfMove nextMove, Vector2 actualBallPosition)
        {
            if (actualBallPosition == board.StillSafePoints[0] || actualBallPosition == board.StillSafePoints[1])
                return true;
            if (actualBallPosition == board.WinningPoints[0] || actualBallPosition == board.WinningPoints[1])
                return true;
            if (actualBallPosition == board.PartiallySafePoints[0] && (nextMove != TypeOfMove.N && nextMove != TypeOfMove.NW && nextMove != TypeOfMove.W))
                return true;
            if (actualBallPosition == board.PartiallySafePoints[1] && (nextMove != TypeOfMove.S && nextMove != TypeOfMove.SW && nextMove != TypeOfMove.W))
                return true;
            if (actualBallPosition == board.PartiallySafePoints[2] && (nextMove != TypeOfMove.N && nextMove != TypeOfMove.NE && nextMove != TypeOfMove.E))
                return true;
            if (actualBallPosition == board.PartiallySafePoints[3] && (nextMove != TypeOfMove.S && nextMove != TypeOfMove.SE && nextMove != TypeOfMove.E))
                return true;

            bool forbidenMove = false;
            if (actualBallPosition.Y == board.Borders[0].Y)
                if (nextMove != TypeOfMove.S && nextMove != TypeOfMove.SE && nextMove != TypeOfMove.SW)
                    forbidenMove = true;
            if (actualBallPosition.Y == board.Borders[1].Y)
                if (nextMove != TypeOfMove.N && nextMove != TypeOfMove.NE && nextMove != TypeOfMove.NW)
                    forbidenMove = true;
            if (actualBallPosition.X == board.Borders[2].X)
                if (nextMove == TypeOfMove.E || nextMove == TypeOfMove.NE || nextMove == TypeOfMove.SE)
                    forbidenMove = false;
                else
                    forbidenMove = true;
            if (actualBallPosition.X == board.Borders[3].X)
                if (nextMove == TypeOfMove.W || nextMove == TypeOfMove.NW || nextMove == TypeOfMove.SW)
                    forbidenMove = false;
                else
                    forbidenMove = true;

            return !forbidenMove;
        }

        /// <summary>
        /// Sprawdza czy ruch jest dostêpny do wykoania - czyli, czy nic nie stoi na drodze by go wykonaæ.
        /// </summary>
        /// <param name="start">Pocz¹tkowy punkt ruchu.</param>
        /// <param name="end">Koñcowy punkt ruchu.</param>
        /// <param name="movesHistory">Historia juz wykonanych ruchów.</param>
        /// <returns>True jeœli ruch jest mozliwy, False jeœli ruchu nie mozna wykonaæ.</returns>
        private bool PathClear(Vector2 start, Vector2 end, List<GameMove> movesHistory)
        {
            bool clear = true;
            for (int i = 0; i < movesHistory.Count; i++)
            {
                if (movesHistory[i].StartPosition == start && movesHistory[i].EndPosition == end)
                    clear = false;
                if (movesHistory[i].StartPosition == end && movesHistory[i].EndPosition == start)
                    clear = false;
            }
            return clear;
        }

        /// <summary>
        /// Sprawdza, czy po wykonaniu ruchu mozliwe jest odbicie, czyli dodatkowy ruch.
        /// </summary>
        /// <param name="ballPosition"></param>
        /// <param name="movesHistory"></param>
        /// <returns></returns>
        private bool CanItBounced(Vector2 ballPosition, List<GameMove> movesHistory)
        {
            for (int i = movesHistory.Count - 1; i >= 0; i--)
            {
                if (movesHistory[i].StartPosition == ballPosition || movesHistory[i].EndPosition == ballPosition &&
                    movesHistory[movesHistory.Count - 1].EndPosition != ballPosition)
                    return true;
            }

            if (ballPosition == board.StillSafePoints[0] || ballPosition == board.StillSafePoints[1])
                return false;

            /*
             * nie ma potrzeby sprawdzania naro¿ników bramek, bo w nastêpnym warunku sprawdzamy, czy odbijamy siê
             * od granic boiska, na których w³asnie znajduj¹ siê naro¿niki.
             */
            //if (ballPosition == board.PartiallySafePoints[0] || ballPosition == board.PartiallySafePoints[1] ||
            //    ballPosition == board.PartiallySafePoints[2] || ballPosition == board.PartiallySafePoints[3])
            //    return true;

            if (ballPosition.Y == board.Borders[0].Y || ballPosition.Y == board.Borders[1].Y ||
                ballPosition.X == board.Borders[2].X || ballPosition.X == board.Borders[3].X)
                return true;

            return false;
        }

        /// <summary>
        /// Sprawdza jakie ruchy mo¿na jeszcze wykonaæ z punktu gdzie znajduje siê pi³ka.
        /// </summary>
        /// <param name="movesHistory"></param>
        /// <param name="actualBallPosition"></param>
        /// <returns>Zwraca tylko dostepne ruchy zgodne z zasadami gry.</returns>
        public List<TypeOfMove> CheckAvailableMoves(List<GameMove> movesHistory, Vector2 actualBallPosition)
        {
            List<TypeOfMove> list = new List<TypeOfMove>();
            TypeOfMove move = TypeOfMove.UNKNOWN;
                        
            for (int i = 0; i < board.AvailableMoves.Length; i++)
            {
                move = Translator.TranslateVectorToMove(board.AvailableMoves[i]);
                /*
                 * nie bierzemy pod uwagê ostatniego wykonanego ruchu
                 */
                if (movesHistory[movesHistory.Count - 1].Move == move)
                    continue;
                Vector2 desiredMove = actualBallPosition + board.AvailableMoves[i];
                if (PathClear(actualBallPosition, desiredMove, movesHistory) && ObeyedRules(move, actualBallPosition))
                    list.Add(move);
            }

            return list;
        }
    }
}
