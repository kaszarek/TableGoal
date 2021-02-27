using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace TableGoal
{
    /// <summary>
    /// Klasa przetrzymuj¹ca dane odnoœnie meczu - wykorzystywana w trybie World Cup do zapisu meczów.
    /// </summary>
    public class EmbeddedGameVariables
    {
        public bool IsEmpty { get; set; }
        public Team FirstTeam { get; set; }
        public Team SecondTeam { get; set; }
        public Team CurrentTeam { get; set; }
        public PlayField TypeOfField { get; set; }
        public List<GameMove> Moves { get; set; }
        public DifficultyLevel DiffLevel { get; set; }
        public int TotalTime { get; set; }
        public int TimeLeft { get; set; }
        public bool IsGoalLimited { get; set; }
        public int GoalsLimit { get; set; }
        public Country Opponent { get; set; }
        //public Country SelectedCountry { get; set; }

        public EmbeddedGameVariables()
        {
            Moves = new List<GameMove>();
        }
        
        public void Clear()
        {
            Moves.Clear();
            IsEmpty = true;
        }
    }

    public sealed class GameVariables
    {
        bool musicOn = true;
        /// <summary>
        /// Zwraca, lub ustala muzykê. W razie ustawienia muzyki na true zaczyna j¹ odtwarzaæ. W razie ustalenia muzyki na false zatrzymuje j¹.
        /// </summary>
        public bool MusicOn
        {
            get { return musicOn; }
            set
            {
                bool before = musicOn;
                musicOn = value;
                if (musicOn != before)
                    if (!musicOn)
                        AudioManager.StopMusic();
                    else
                        AudioManager.PlayMusic("crowd");
            }
        }

        bool soundsOn = true;
        /// <summary>
        /// W³¹cza, lub wy³acza dŸwieki.
        /// </summary>
        public bool SoundsOn
        {
            get { return soundsOn; }
            set { soundsOn = value; }
        }
        Team firstPlayer;
        /// <summary>
        /// Pierwszy gracz - zawsze jst nim czlowiek.
        /// </summary>
        public Team FirstPlayer
        {
            get { return firstPlayer; }
            set { firstPlayer = value; }
        }
        Team secondPlayer;
        /// <summary>
        /// Drugi gracz - przeciwnik pierwszego. Bywa nim CPU, lub inny cz³owiek.
        /// </summary>
        public Team SecondPlayer
        {
            get { return secondPlayer; }
            set { secondPlayer = value; }
        }

        Team currentPlayer;
        /// <summary>
        /// Zespó³ który teraz posiada ruch.
        /// </summary>
        public Team CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; }
        }

        List<GameMove> performedMoves;
        /// <summary>
        /// Ustala, lub zwraca listê wykonanych ruchów.
        /// </summary>
        public List<GameMove> PerformedMoves
        {
            get { return performedMoves; }
            set { performedMoves = value; }
        }

        DifficultyLevel selectedDifficultyLevel;
        /// <summary>
        /// Zwraca, lub ustala poziom trudnoœci. Przy przypisaniu poziomu trudnoœci ustala równie¿ kto zaczyna rozgrywkê.
        /// </summary>
        public DifficultyLevel DiffLevel
        {
            get { return selectedDifficultyLevel; }
            set 
            { 
                selectedDifficultyLevel = value;
                switch (selectedDifficultyLevel)
                {
                    case DifficultyLevel.EASY:
                        NextMoveForFirstPlayer();
                        break;
                    case DifficultyLevel.MEDIUM:
                        NextMoveForFirstPlayer();
                        break;
                    case DifficultyLevel.HARD:
                        NextMoveForSecondPlayer();
                        break;
                    default:
                        break;
                }
            }
        }

        int totalTime;
        /// <summary>
        /// Ca³kowity czas meczu wa¿ny dla gry na limit czasu.
        /// </summary>
        public int TotalTime
        {
            get { return totalTime; }
            set { totalTime = value; }
        }

        int timeLeft;
        /// <summary>
        /// Czas, który pozosta³ w przypadku gry na czas. W przypadku gry na limit bramek jest to czas, który up³yn¹³!!
        /// </summary>
        public int TimeLeft
        {
            get { return timeLeft; }
            set { timeLeft = value; }
        }

        PlayField typeOfField;
        /// <summary>
        /// Ustala, lub zwraca rodzaj boiska: <code>classic</code>, <code>large</code>.
        /// </summary>
        public PlayField TypeOfField
        {
            get { return typeOfField; }
            set { typeOfField = value; }
        }

        bool isLimitedByGoals;
        /// <summary>
        /// Zrwaca, lub ustala czy gra jest z limitem bramek. Jeœli nie jest z limitem bramek to limitem jest czas.
        /// </summary>
        public bool IsLimitedByGoals
        {
            get { return isLimitedByGoals; }
            set { isLimitedByGoals = value; }
        }

        int goalsLimit;
        /// <summary>
        /// Zwraca, lub ustala limit bramek.
        /// </summary>
        public int GoalsLimit
        {
            get { return goalsLimit; }
            set { goalsLimit = value; }
        }

        Country selectedCountry;
        /// <summary>
        /// Zwraca, lub ustala wbrany kraj przez gracza.
        /// </summary>
        public Country SelectedCountry
        {
            get { return selectedCountry; }
            set { selectedCountry = value; }
        }

        bool worldCupStarted;
        /// <summary>
        /// Zwraca, lub ustala czy gracz zacz¹³ tryb World Cup.
        /// </summary>
        public bool WorldCupStarted
        {
            get { return worldCupStarted; }
            set { worldCupStarted = value; }
        }

        bool activeWorldCupMatch;
        /// <summary>
        /// Zwraca, lub ustala czy trwa teraz mecz w trybie World Cup.
        /// </summary>
        public bool ActiveWorldCupMatch
        {
            get { return activeWorldCupMatch; }
            set { activeWorldCupMatch = value; }
        }

        public GameVariables()
        {
            firstPlayer = new Team();
            firstPlayer.Controler = Controlling.BUTTONS;
            secondPlayer = new Team();
            secondPlayer.Controler = Controlling.BUTTONS;
            secondPlayer.Coach = TeamCoach.HUMAN;
            performedMoves = new List<GameMove>();
            selectedDifficultyLevel = DifficultyLevel.EASY;
            typeOfField = PlayField.classic;
            isLimitedByGoals = false;
            SelectedCountry = Country.UNKNOWN;
            worldCupStarted = false;
            activeWorldCupMatch = false;
        }
        /// <summary>
        /// Czyœci listê wykonanych ruchów, ustala poziom na EASY i ustala boisko na klasyczne.
        /// </summary>
        public void ResetVariables()
        {
            performedMoves = new List<GameMove>();
            selectedDifficultyLevel = DifficultyLevel.EASY;
            typeOfField = PlayField.classic;
            firstPlayer.Coach = TeamCoach.HUMAN;
            secondPlayer.Coach = TeamCoach.HUMAN;
        }

        public static GameVariables Instance
        {
            get
            {
                return Nested._gameVars;
            }
            internal set
            { Nested._gameVars = value; }
        }             

        private class Nested
        {
            static Nested() { }
            internal static GameVariables _gameVars = new GameVariables();
        }

        public string GetScore()
        {
            if (this.isLimitedByGoals)
            {
                return String.Format("{0}({1}) - {2}({3})", firstPlayer.Goals, goalsLimit, secondPlayer.Goals, goalsLimit);
            }
            else
            {
                return String.Format("{0} - {1}", firstPlayer.Goals, secondPlayer.Goals);
            }
        }
        /// <summary>
        /// Ustala liczbê zdobytych bramek na 0 i ustala ruch dla pierwszego gracza. Jesli poziomem prudnoœci jest poziom HARD to zaczyna gracz drugi (CPU).
        /// </summary>
        public void RestartGame()
        {
            this.firstPlayer.Goals = 0;
            this.secondPlayer.Goals = 0;
            if (this.selectedDifficultyLevel == DifficultyLevel.HARD)
                NextMoveForSecondPlayer();
            else
                NextMoveForFirstPlayer();
        }
        /// <summary>
        /// Ustala nastêpny ruch dla gracza pierwszego i tym samym dobiera ruch drugiemu graczowi.
        /// </summary>
        public void NextMoveForFirstPlayer()
        {
            firstPlayer.HaveMoveNow = true;
            currentPlayer = firstPlayer;
            secondPlayer.HaveMoveNow = false;
        }
        /// <summary>
        /// Ustala nastêpny ruch dla gracza drugiego i tym samym odbiera ruch pierwszemu graczowi.
        /// </summary>
        public void NextMoveForSecondPlayer()
        {
            firstPlayer.HaveMoveNow = false;
            secondPlayer.HaveMoveNow = true;
            currentPlayer = secondPlayer;
        }
        /// <summary>
        /// Dodaje bramkê graczowi pierwszemu, ustala ruch dla drugiego gracza i w razie gry z CPU aktualizuje statystyki.
        /// </summary>
        public void FirstScored()
        {
            firstPlayer.Scored();
            firstPlayer.HaveMoveNow = false;
            secondPlayer.HaveMoveNow = true;
            currentPlayer = secondPlayer;
            if (secondPlayer.Coach != TeamCoach.HUMAN)
                Statistics.Instance.StrzelonyGol();
            if (firstPlayer.Coach == TeamCoach.REMOTEOPPONENT)
                Statistics.Instance.StraconaBramka();
        }
        /// <summary>
        /// Dodaje bramke graczowi drugiemu, ustala ruch dla gracza pierwszego i w razie gry z CPU aktualizuje statystyki.
        /// </summary>
        public void SecondScored()
        {
            secondPlayer.Scored();
            secondPlayer.HaveMoveNow = false;
            firstPlayer.HaveMoveNow = true;
            currentPlayer = firstPlayer;
            if (secondPlayer.Coach != TeamCoach.HUMAN)
                Statistics.Instance.StraconaBramka();
            if (firstPlayer.Coach == TeamCoach.REMOTEOPPONENT)
                Statistics.Instance.StrzelonyGol();
        }
        /// <summary>
        /// Zwraza <code>Team</code> z podanego jako parametr <code>TypeOfPlayer</code>.
        /// </summary>
        /// <param name="player">Gracz - First albo Second.</param>
        /// <returns><code>Team</code>, czyli: kolor koszulki, iloœc bramek, metoda kontroli, trener i posiada ruch.</returns>
        public Team GiveTeamFromPlayer(TypeOfPlayer player)
        {
            if (player == TypeOfPlayer.First)
                return this.firstPlayer;
            else
                return this.secondPlayer;
        }
        /// <summary>
        /// Zapisuje dane do pliku.
        /// </summary>
        /// <param name="streamWriter"></param>
        public void Serialize(StreamWriter streamWriter)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameVariables));
            using (TextWriter tw = streamWriter)
            {
                serializer.Serialize(tw, this);
            }
        }
        /// <summary>
        /// Odczytuje dane z pliku
        /// </summary>
        /// <param name="streamReader"></param>
        public void Deserialize(StreamReader streamReader)
        {
            //string a = streamReader.ReadToEnd();
            streamReader.BaseStream.Position = 0;
            XmlSerializer deserializer = new XmlSerializer(typeof(GameVariables));
            GameVariables gv;
            using (TextReader tr = streamReader)
            {
                try
                {
                    gv = (GameVariables)deserializer.Deserialize(tr);
                    GameVariables.Instance = gv;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine(" =========  EXCEPTION when sending email  =============");
                    Debug.WriteLine(ex.Message);
#endif
                    TableGoal.GameVarOK = false;
                }
            }
        }
        /// <summary>
        /// Ustala <code>currentPlayer</code> na pierwszego, lub drugiego w zale¿noœci od tego kto ma teraz ruch.
        /// </summary>
        public void UpdateCurrentPlayer()
        {
            if (firstPlayer.HaveMoveNow)
            {
                currentPlayer = firstPlayer;
            }
            else
            {
                currentPlayer = secondPlayer;
            }
        }

        public Color ColorOfWifiGame()
        {
            Color c = Color.Black;
            if (secondPlayer.Coach == TeamCoach.REMOTEOPPONENT)
            {
                c = firstPlayer.ShirtsColor;
            }
            else if (firstPlayer.Coach == TeamCoach.REMOTEOPPONENT)
            {
                c = secondPlayer.ShirtsColor;
            }

            return c;
        }

        public int Limitation()
        {
            if (this.isLimitedByGoals)
                return this.goalsLimit;
            return timeLeft;
        }

        public bool IsWiFiGame()
        {
            bool result = false;
            if (firstPlayer.Coach == TeamCoach.REMOTEOPPONENT || secondPlayer.Coach == TeamCoach.REMOTEOPPONENT)
                result = true;
            return result;
        }

    }
}
