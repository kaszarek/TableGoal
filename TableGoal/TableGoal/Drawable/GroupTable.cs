using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace TableGoal
{
    public class EmbeddedGroupInformation
    {
        public string Group { get; set; }
        public int Round { get; set; }
        public bool PlayerGroup { get; set; }
        public bool PhaseEnded { get; set; }
        public List<CountryTeam> TeamInThisGroup { get; set; }
        public List<CountryTeam> OpponentsOfThePlayer { get; set; }
    }

    public class GroupTable
    {
        internal int UP_MARIGIN = 110;
        internal int x = 30;
        internal int y = 18;
        internal int dx = 90;
        internal int dy = 55;
        internal static int MAX_ILOSC_KOLEJEK__GRUPY = 3;
        internal static int MAX_ILOSC_KOLEJEK__DALSZE_MECZE = 1;
        string group;
        int currentRound;
        bool thisPhaseMatchesHasEnded;
        bool playersGroup;
        List<CountryTeam> teamInGroup;
        List<CountryTeam> opponetsOfThePlayer;
        List<CountryTeam> sortedTeamInGroup;
        List<UIPicture> teamsFlags;
        UIPicture _V_line;
        UIPicture _H_line;
        SpriteFont _scoreFont;
        UIPicture _dimmedCover;

        public List<CountryTeam> SortedTeamInGroup
        {
            get { return sortedTeamInGroup; }
        }

        public List<CountryTeam> TeamInGroup
        {
            get { return teamInGroup; }
            set { teamInGroup = value; }
        }

        public bool PlayersGroup
        {
            get { return playersGroup; }
            set
            {
                playersGroup = value;
                ExtractOpponentToSeparateList();
            }
        }

        public bool ThisPhaseMatchesHasEnded
        {
            get { return thisPhaseMatchesHasEnded; }
            set { thisPhaseMatchesHasEnded = value; }
        }
        /// <summary>
        /// Kontury tabeli z wynikami. Ka¿da linia w tabeli jest zdefiniowana
        /// jako prostok¹t, gdzie szerokoœc takiej lini to 4 pixele.
        /// </summary>
        static Rectangle[] tablesCountur = new Rectangle[13] 
        {
            new Rectangle(10, 50, 770, 4),
            new Rectangle(10, 100, 770, 4),
            new Rectangle(10, 170, 770, 4),
            new Rectangle(10, 245, 770, 4),
            new Rectangle(10, 320, 770, 4),
            new Rectangle(10, 395, 770, 4),

            new Rectangle(10, 50, 4, 347),
            new Rectangle(160, 50, 4, 347),
            new Rectangle(284, 50, 4, 347),
            new Rectangle(408, 50, 4, 347),
            new Rectangle(532, 50, 4, 347),
            new Rectangle(656, 50, 4, 347),
            new Rectangle(780, 50, 4, 347)
        };
        /// <summary>
        /// Tworzy tabelê o padanej zawie i z podanej listy zespo³ów.
        /// </summary>
        /// <param name="groupName">Nazwa grupy</param>
        /// <param name="teams">Lista zespo³ow nale¿¹cych do grupy</param>
        public GroupTable(string groupName, List<Country> teams)
        {
            currentRound = 0;
            group = groupName;
            thisPhaseMatchesHasEnded = false;
            teamInGroup = new List<CountryTeam>();
            teamsFlags = new List<UIPicture>();
            int counter = 0;
            foreach (Country team in teams)
            {
                teamInGroup.Add(new CountryTeam(team));
                teamsFlags.Add(new UIPicture(Countries.pathToFlags[team], new Rectangle(x, UP_MARIGIN + (dy + y) * counter, dx, dy)));
                counter++;
            }
            InitializeDrawableObjects();
            playersGroup = false;
        }
        /// <summary>
        /// Tworzy tabelê o padanej zawie i dodaje do niej podan¹ listê zespo³ów oraz zespó³ gracza.
        /// </summary>
        /// <param name="groupName">Nazwa grupy</param>
        /// <param name="teams">Lista zespo³ów</param>
        /// <param name="one">Zespó³ gracza.</param>
        public GroupTable(string groupName, List<Country> teams, Country one)
        {
            currentRound = 0;
            group = groupName;
            thisPhaseMatchesHasEnded = false;
            teamInGroup = new List<CountryTeam>();
            sortedTeamInGroup = new List<CountryTeam>();
            teamsFlags = new List<UIPicture>();
            opponetsOfThePlayer = new List<CountryTeam>();
            int counter = 0;
            foreach (Country team in teams)
            {
                teamInGroup.Add(new CountryTeam(team));
                opponetsOfThePlayer.Add(teamInGroup.Last());
                teamsFlags.Add(new UIPicture(Countries.pathToFlags[team], new Rectangle(x, UP_MARIGIN + (dy + y) * counter, dx, dy)));
                counter++;
            }
            teamInGroup.Add(new CountryTeam(one));
            teamsFlags.Add(new UIPicture(Countries.pathToFlags[one], new Rectangle(x, UP_MARIGIN + (dy + y) * counter, dx, dy)));
            InitializeDrawableObjects();
            playersGroup = true;
        }
        /// <summary>
        /// Wtorzy tabelê z obiektu <code>EmbeddedGroupInformation</code>.
        /// </summary>
        /// <param name="embeddeInfo">Dane potrzebne do stworzenia tabeli.</param>
        public GroupTable(EmbeddedGroupInformation embeddeInfo)
        {
            currentRound = embeddeInfo.Round;
            if (currentRound >= MAX_ILOSC_KOLEJEK__GRUPY)
                thisPhaseMatchesHasEnded = true;
            thisPhaseMatchesHasEnded = embeddeInfo.PhaseEnded;
            group = embeddeInfo.Group;
            playersGroup = embeddeInfo.PlayerGroup;
            teamInGroup = embeddeInfo.TeamInThisGroup;
            if (playersGroup)
            {
                opponetsOfThePlayer = new List<CountryTeam>();
                for (int i = 0; i < teamInGroup.Count; i++)
                    for (int j = 0; j < embeddeInfo.OpponentsOfThePlayer.Count; j++)
                        if (teamInGroup[i].Country == embeddeInfo.OpponentsOfThePlayer[j].Country)
                        {
                            opponetsOfThePlayer.Add(teamInGroup.ElementAt(i));
                            continue;
                        }

                foreach (CountryTeam teamCT in teamInGroup)
                    Debug.WriteLine(String.Format("{0}", teamCT.ToString()));
                foreach (CountryTeam oppCT in embeddeInfo.OpponentsOfThePlayer)
                    Debug.WriteLine(String.Format("{0}", oppCT.ToString()));
            }
            teamsFlags = new List<UIPicture>();
            sortedTeamInGroup = new List<CountryTeam>();

            int counter = 0;
            foreach (CountryTeam ct in teamInGroup)
            {
                teamsFlags.Add(new UIPicture(Countries.pathToFlags[ct.Country], new Rectangle(x, UP_MARIGIN + (dy+y) * counter, dx, dy)));
                counter++;
            }
            InitializeDrawableObjects();
            Sort();
        }
        /// <summary>
        /// Inicjalizacja rysowalnych obiektów.
        /// </summary>
        private void InitializeDrawableObjects()
        {
            _V_line = new UIPicture("WC/ver_line", new Rectangle(0, 0, 0, 0));
            _V_line.Color = Color.Black;
            _H_line = new UIPicture("WC/hor_line", new Rectangle(0, 0, 0, 0));
            _H_line.Color = Color.Black;
            _dimmedCover = new UIPicture("empty4x4", new Rectangle(0, 0, 0, 0));
            Color kolor = Color.Black;
            kolor.A = 180;
            _dimmedCover.Color = kolor;
        }
        /// <summary>
        /// Symuluje nastepn¹ rundê meczy jeœli nie zosta³ osi¹gniêty limit gier w rundzie.
        /// Po rozegranych meczach sortuje grupy.
        /// </summary>
        public void NextRount()
        {
            if (thisPhaseMatchesHasEnded)
            {
                Sort();
                return;
            }

            if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.GROUP_PHASE)
            {
                currentRound++;
                if (currentRound >= MAX_ILOSC_KOLEJEK__GRUPY)
                {
                    thisPhaseMatchesHasEnded = true;
                }

                if (playersGroup)
                {
                    if (currentRound <= MAX_ILOSC_KOLEJEK__GRUPY)
                    {
                        switch (currentRound)
                        {
                            case 1:
                                opponetsOfThePlayer[1].PlayMatch(opponetsOfThePlayer[2]);
                                break;
                            case 2:
                                opponetsOfThePlayer[0].PlayMatch(opponetsOfThePlayer[2]);
                                break;
                            case 3:
                                opponetsOfThePlayer[0].PlayMatch(opponetsOfThePlayer[1]);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                        thisPhaseMatchesHasEnded = true;
                }
                else
                {
                    if (currentRound <= MAX_ILOSC_KOLEJEK__GRUPY)
                    {
                        switch (currentRound)
                        {
                            case 1:
                                teamInGroup[0].PlayMatch(teamInGroup[1]);
                                teamInGroup[2].PlayMatch(teamInGroup[3]);
                                break;
                            case 2:
                                teamInGroup[0].PlayMatch(teamInGroup[2]);
                                teamInGroup[1].PlayMatch(teamInGroup[3]);
                                break;
                            case 3:
                                teamInGroup[0].PlayMatch(teamInGroup[3]);
                                teamInGroup[1].PlayMatch(teamInGroup[2]);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                        thisPhaseMatchesHasEnded = true;
                }

            }
            else
            {
                if (!playersGroup)
                {
                    teamInGroup[0].PlayMatch(teamInGroup[1]);
                }
                currentRound++;
                thisPhaseMatchesHasEnded = true;
            }
            this.Sort();
        }
        /// <summary>
        /// Wydobywa przeciników gracza z tabeli i umieszcza ich w liœcie <code>opponentsOfThePlayer</code>.
        /// </summary>
        private void ExtractOpponentToSeparateList()
        {
            if (playersGroup)
                if (opponetsOfThePlayer == null)
                {
                    opponetsOfThePlayer = new List<CountryTeam>();
                    foreach (CountryTeam ct in teamInGroup)
                        if (ct.Country != WorldCupProgress.Instance.SelectedCountry)
                            opponetsOfThePlayer.Add(ct);
                }
        }
        /// <summary>
        /// Sortuje dru¿yny. Gdy jeszcze ¿aden mecz nie zost¹³ rozegrany to
        /// sortuje wed³ug nazwy. W przeciwnym wypadku wed³ug zdobytych punktów
        /// </summary>
        public void Sort()
        {
            if (currentRound == 0)
            {
                sortedTeamInGroup = (from CountryTeam ct in teamInGroup
                                     orderby ct.Country
                                     select ct).ToList();
            }
            else
            {
                sortedTeamInGroup = (from CountryTeam ct in teamInGroup
                                     orderby ct.CollectedPoints descending
                                     select ct).ToList();
                /*
                 * Jeœli to jest grupa gracza i to jeszcze jest faze grupowa (tyle, ¿e zakoñczona)
                 */
                if (playersGroup && thisPhaseMatchesHasEnded && WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.GROUP_PHASE)
                {
                    /*
                     * Jeœli drugi i trzeci kraj maj¹ tyle samo punktów i krajem 3 jest kraj gracza to zamieniamy kolejnoœæ -> ¿eby gracz nie by³ pokrzywdzony przez sortowanie
                     */
                    if (sortedTeamInGroup[1].CollectedPoints == sortedTeamInGroup[2].CollectedPoints && sortedTeamInGroup[2].Country == WorldCupProgress.Instance.SelectedCountry)
                    {
                        CountryTeam tempToSwap = sortedTeamInGroup[1];
                        sortedTeamInGroup[1] = sortedTeamInGroup[2];
                        sortedTeamInGroup[2] = tempToSwap;
                    }
                }
            }
            RecalculateDestinationRectanglesOfFlags();
        }
        /// <summary>
        /// Przelicza na nowo miejsca flag w tabeli.
        /// </summary>
        public void RecalculateDestinationRectanglesOfFlags()
        {
            int counter = 0;
            if (currentRound <= MAX_ILOSC_KOLEJEK__GRUPY)
            {
                for (int i = 0; i < sortedTeamInGroup.Count; i++)
                {
                    for (int j = 0; j < teamsFlags.Count; j++)
                    {
                        if (teamsFlags[j].TextureName.Contains(sortedTeamInGroup[i].Country.ToString()))
                        {
                            teamsFlags[j].DestinationRectangle = new Rectangle(x, UP_MARIGIN + (dy + y) * counter, dx, dy);
                            break;
                        }
                    }
                    counter++;
                }
            }
            counter = 0;
            if (WorldCupProgress.Instance.PhaseOfTheWorldCup != StateOfPlay.GROUP_PHASE)
            {
                for (int i = 0; i < sortedTeamInGroup.Count; i++)
                {
                    for (int j = 0; j < teamsFlags.Count; j++)
                    {
                        if (teamsFlags[j].TextureName.Contains(sortedTeamInGroup[i].Country.ToString()))
                        {
                            teamsFlags[j].DestinationRectangle = new Rectangle(100 + counter * 400, 180, 200, 125);
                            break;
                        }
                    }
                    counter++;
                } 
            }
        }
        /// <summary>
        /// Zwraca dwa najlepsze zespo³y. Jeœli w grupie jest mniej ni¿ dwa zespo³y to zwraca pust¹ listê.
        /// </summary>
        /// <returns>Lista zespo³ów.</returns>
        public List<CountryTeam> GiveTwoBest()
        {
            List<CountryTeam> bestTwo = new List<CountryTeam>();
            if (sortedTeamInGroup.Count >= 2)
            {
                Sort();
                bestTwo = sortedTeamInGroup.GetRange(0, 2);
            }
            return bestTwo;
        }
        /// <summary>
        /// Zwraca przeciwnika gracza. W razie braku zespo³u zwróci <code>Country.UNKNOWN</code>.
        /// </summary>
        /// <returns>Nazwa zespo³u, który teraz bêdzie rozgrywa³ mecz z graczem.</returns>
        public Country GiveOpponent()
        {
            if (currentRound >= MAX_ILOSC_KOLEJEK__GRUPY)
            {
                thisPhaseMatchesHasEnded = true;
                return Country.UNKNOWN;
            }
            /*
             * Poza faz¹ grupow¹ jest tylko jeden przeciwnik, poniewa¿ grupami s¹
             * tak naprawdê pary dru¿yn. Czyli przeciwnik jest jeden i to jego trzeba zwróciæ.
             */
            if (WorldCupProgress.Instance.PhaseOfTheWorldCup != StateOfPlay.GROUP_PHASE)
                return opponetsOfThePlayer[0].Country;
            if (opponetsOfThePlayer.Count > 0)
                return opponetsOfThePlayer[currentRound].Country;
            return Country.UNKNOWN;
        }
        /// <summary>
        /// Parsuje grupê do <code>EmbeddedGroupInformation</code>.
        /// </summary>
        /// <returns>Obiekt <code>EmbeddedGroupInformation</code> reprezentuj¹cy grupê.</returns>
        public EmbeddedGroupInformation ToEmbeddedGroupInformation()
        {
            EmbeddedGroupInformation embeddedInfo = new EmbeddedGroupInformation()
            {
                Group = this.group,
                PlayerGroup = this.playersGroup,
                OpponentsOfThePlayer = this.opponetsOfThePlayer,
                Round = this.currentRound,
                TeamInThisGroup = this.teamInGroup,
                PhaseEnded = thisPhaseMatchesHasEnded
            };
            return embeddedInfo;
        }

        public void LoadTexture(ContentManager contentManager)
        {
            _scoreFont = contentManager.Load<SpriteFont>("Fonts/SketchRockwell");
            _V_line.LoadTexture(contentManager);
            _H_line.LoadTexture(contentManager);
            _dimmedCover.LoadTexture(contentManager);
            foreach (UIPicture flag in teamsFlags)
                flag.LoadTexture(contentManager);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.GROUP_PHASE)
            {
                Vector2 titleWidth = _scoreFont.MeasureString(String.Format("Group {0}", this.group));
                /*
                 * Nag³ówek ekranu/tabeli
                 */
                spriteBatch.DrawString(_scoreFont,
                                       String.Format("Group {0}", this.group),
                                       new Vector2(400 - titleWidth.X/2, -5),
                                       Color.Black);
                /*
                 * Usupe³nienie nazw kolumn tabeli
                 */
                spriteBatch.DrawString(_scoreFont,
                                       String.Format("Team    M      W       L      D     Pts"),
                                       new Vector2(20, 55),
                                       Color.Black);
                int counter = 0;
                Color color = Color.Black;
                foreach (CountryTeam ct in sortedTeamInGroup)
                {
                    if (ct.Country == WorldCupProgress.Instance.SelectedCountry)
                    {
                        color = Color.Red;
                    }
                    else
                    {
                        color = Color.Black;
                    }
                    /*
                     * Uzupe³nianie tabeli wynikami dru¿yn
                     */
                    spriteBatch.DrawString(_scoreFont,
                                           String.Format("{0}", ct.MatchesPlayed),
                                           new Vector2(x + dx + 90, UP_MARIGIN + (dy + y) * counter),
                                           color);
                    spriteBatch.DrawString(_scoreFont,
                                           String.Format("{0}", ct.WonMatches),
                                           new Vector2(x + dx + 214, UP_MARIGIN + (dy + y) * counter),
                                           color);
                    spriteBatch.DrawString(_scoreFont,
                                           String.Format("{0}", ct.LostMatches),
                                           new Vector2(x + dx + 338, UP_MARIGIN + (dy + y) * counter),
                                           color);
                    spriteBatch.DrawString(_scoreFont,
                                           String.Format("{0}", ct.DrawMatches),
                                           new Vector2(x + dx + 462, UP_MARIGIN + (dy + y) * counter),
                                           color);
                    spriteBatch.DrawString(_scoreFont,
                                           String.Format("{0}", ct.CollectedPoints),
                                           new Vector2(x + dx + 586, UP_MARIGIN + (dy + y) * counter),
                                           color);
                    counter++;
                }
                /*
                 * Rysownie flag
                 */
                foreach (UIPicture flag in teamsFlags)
                    flag.Draw(spriteBatch);
                counter = 0;
                /*
                 * Rysowanie samej tabeli
                 */
                foreach (Rectangle rect in tablesCountur)
                {
                    if (counter < 6)
                    {
                        _H_line.DestinationRectangle = rect;
                        _H_line.Draw(spriteBatch);
                    }
                    else
                    {
                        _V_line.DestinationRectangle = rect;
                        _V_line.Draw(spriteBatch);
                    }
                    counter++;
                }
                if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.GROUP_PHASE)
                {
                    if (thisPhaseMatchesHasEnded)
                    {
                        spriteBatch.DrawString(_scoreFont, String.Format("Go to the 1/8th phase"), new Vector2(180, 420), Color.Black);
                        /*
                         * Zaciemnienie dwóch zespo³ów, które odpadaj¹ z grupy i z mistrzostw
                         */
                        for (int i = 0; i < sortedTeamInGroup.Count; i++)
                        {
                            if (i >= 2)
                            {
                                for (int j = 0; j < teamsFlags.Count; j++)
                                {
                                    if (teamsFlags[j].TextureName.Contains(sortedTeamInGroup[i].Country.ToString()))
                                    {
                                        _dimmedCover.DestinationRectangle = teamsFlags[j].DestinationRectangle;
                                        _dimmedCover.Draw(spriteBatch);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Vector2 titleWidth = _scoreFont.MeasureString(String.Format("{0}", this.group));
                spriteBatch.DrawString(_scoreFont, String.Format("{0}", this.group), new Vector2(400 - titleWidth.X/2, -5), Color.Black);
                foreach (UIPicture flag in teamsFlags)
                    flag.Draw(spriteBatch);
                spriteBatch.DrawString(_scoreFont, String.Format("vs"), new Vector2(380, 225), Color.Black);

                if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.ONE_EIGHT_PHASE)
                {
                    //spriteBatch.DrawString(_scoreFont, "1/8th", new Vector2(0, 100), Color.Black);
                    if (currentRound > 0)
                    {
                        Vector2 stringWidth = _scoreFont.MeasureString(String.Format("Go to the 1/4th phase"));
                        spriteBatch.DrawString(_scoreFont, String.Format("Go to the 1/4th phase"), new Vector2(400 - stringWidth.X / 2, 420), Color.Black);
                        if (teamsFlags[0].TextureName.Contains(sortedTeamInGroup[1].Country.ToString()))
                        {
                            _dimmedCover.DestinationRectangle = teamsFlags[0].DestinationRectangle;
                            _dimmedCover.Draw(spriteBatch);
                        }
                        else
                        {
                            _dimmedCover.DestinationRectangle = teamsFlags[1].DestinationRectangle;
                            _dimmedCover.Draw(spriteBatch);
                        }
                    }
                    else
                    {
                        string phaseState = "1/8th phase";
                        Vector2 phaseStateSize = _scoreFont.MeasureString(phaseState);
                        spriteBatch.DrawString(_scoreFont, phaseState, new Vector2(400 - phaseStateSize.X / 2, 420), Color.Black);
                    }
                }
                else if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.QUATER_FINAL_PHASE)
                {
                    //spriteBatch.DrawString(_scoreFont, "1/4th", new Vector2(0, 100), Color.Black);
                    if (currentRound > 0)
                    {
                        Vector2 stringWidth = _scoreFont.MeasureString(String.Format("Go to the semi final"));
                        spriteBatch.DrawString(_scoreFont, String.Format("Go to the semi final"), new Vector2(400 - stringWidth.X / 2, 420), Color.Black);
                        if (teamsFlags[0].TextureName.Contains(sortedTeamInGroup[1].Country.ToString()))
                        {
                            _dimmedCover.DestinationRectangle = teamsFlags[0].DestinationRectangle;
                            _dimmedCover.Draw(spriteBatch);
                        }
                        else
                        {
                            _dimmedCover.DestinationRectangle = teamsFlags[1].DestinationRectangle;
                            _dimmedCover.Draw(spriteBatch);
                        }
                    }
                    else
                    {
                        string phaseState = "1/4th phase";
                        Vector2 phaseStateSize = _scoreFont.MeasureString(phaseState);
                        spriteBatch.DrawString(_scoreFont, phaseState, new Vector2(400 - phaseStateSize.X / 2, 420), Color.Black);
                    }   
                }
                else if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.SEMI_FINAL_PHASE)
                {
                    //spriteBatch.DrawString(_scoreFont, "semi finals", new Vector2(0, 100), Color.Black);
                    if (currentRound > 0)
                    {
                        Vector2 stringWidth = _scoreFont.MeasureString(String.Format("Go to the final phase"));
                        spriteBatch.DrawString(_scoreFont, String.Format("Go to the final phase"), new Vector2(400 - stringWidth.X / 2, 420), Color.Black);
                        if (teamsFlags[0].TextureName.Contains(sortedTeamInGroup[1].Country.ToString()))
                        {
                            _dimmedCover.DestinationRectangle = teamsFlags[0].DestinationRectangle;
                            _dimmedCover.Draw(spriteBatch);
                        }
                        else
                        {
                            _dimmedCover.DestinationRectangle = teamsFlags[1].DestinationRectangle;
                            _dimmedCover.Draw(spriteBatch);
                        }    
                    }
                    else
                    {
                        string phaseState = "semi finals";
                        Vector2 phaseStateSize = _scoreFont.MeasureString(phaseState);
                        spriteBatch.DrawString(_scoreFont, phaseState, new Vector2(400 - phaseStateSize.X / 2, 420), Color.Black);
                    }   
                }
                else if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.FINAL_PHASE ||
                         WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.SMALL_FINAL_PHASE)
                {
                    //spriteBatch.DrawString(_scoreFont, "finals", new Vector2(0, 100), Color.Black);
                    if (currentRound > 0)
                    {
                        Vector2 stringWidth = _scoreFont.MeasureString(String.Format("Summary"));
                        spriteBatch.DrawString(_scoreFont, String.Format("Summary"), new Vector2(400 - stringWidth.X / 2, 420), Color.Black);
                        if (teamsFlags[0].TextureName.Contains(sortedTeamInGroup[1].Country.ToString()))
                        {
                            _dimmedCover.DestinationRectangle = teamsFlags[0].DestinationRectangle;
                            _dimmedCover.Draw(spriteBatch);
                        }
                        else
                        {
                            _dimmedCover.DestinationRectangle = teamsFlags[1].DestinationRectangle;
                            _dimmedCover.Draw(spriteBatch);
                        }    
                    }
                    else
                    {
                        string phaseState = "final phase";
                        Vector2 phaseStateSize = _scoreFont.MeasureString(phaseState);
                        spriteBatch.DrawString(_scoreFont, phaseState, new Vector2(400 - phaseStateSize.X / 2, 420), Color.Black);
                    }   
                }
            }
            /*
             * Jesli grupa gracza to rysuje flagi.
             * Dotyczy to tylko fazy grupowej i gry iloœæ kolejek jest mniejsza od makzymalnej iloœci kolejek.
             */
            if (playersGroup)
            {
                if (currentRound < MAX_ILOSC_KOLEJEK__GRUPY)
                {
                    if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.GROUP_PHASE)
                    {
                        spriteBatch.DrawString(_scoreFont, String.Format("vs"), new Vector2(380, 410), Color.Black);
                        foreach (UIPicture flag in teamsFlags)
                        {
                            if (flag.TextureName.Contains(WorldCupProgress.Instance.SelectedCountry.ToString()))
                            {
                                Rectangle backup = flag.DestinationRectangle;
                                flag.DestinationRectangle = new Rectangle(250, 405, 100, 60);
                                flag.Draw(spriteBatch);
                                flag.DestinationRectangle = backup;
                            }
                            if (flag.TextureName.Contains(opponetsOfThePlayer[currentRound].Country.ToString()))
                            {
                                Rectangle backup = flag.DestinationRectangle;
                                flag.DestinationRectangle = new Rectangle(450, 405, 100, 60);
                                flag.Draw(spriteBatch);
                                flag.DestinationRectangle = backup;
                            }
                        }
                    }
                }
            }
        }
    }
}
