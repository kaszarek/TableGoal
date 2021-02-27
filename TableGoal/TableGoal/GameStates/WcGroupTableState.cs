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
    public class WcGroupTableState : GameState
    {
        Menu menu;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        List<GroupTable> groups;
        UIPicture leftArrow;
        UIPicture rightArrow;
        UIJumpingUIPicture startMatch;
        int index = 0;
        int PLAY_TIME = 300; // [s] = 5 min
        bool gameFinishedForPlayersTeam = false;
        List<Country> _final_four;

        public WcGroupTableState()
        {
            this.EnabledGestures = GestureType.Tap | GestureType.Flick;
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 250, 400, 208));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            groups = new List<GroupTable>();
            /*
             * Je�li s� ju� jakie� informacje o grupach zapami�tane
             */
            if (WorldCupProgress.Instance.Populated)
            {
                /*
                 * Tworzymy grupy z EmbeddedGroupInformation
                 */
                foreach (EmbeddedGroupInformation emInfo in WorldCupProgress.Instance.Groups)
                    groups.Add(new GroupTable(emInfo));
            }
                /*
                 * Je�li nie ma zamapi�tanych informacji o grupach.
                 */
            else
            {
                /*
                 * To tworzymy nowe grupy.
                 */
                List<Country> list = FIFAboard.DrawCountriesToWC();
                groups.Add(new GroupTable("A", list.Take(3).ToList(), WorldCupProgress.Instance.SelectedCountry));
                list.RemoveRange(0, 3);
                groups.Add(new GroupTable("B", list.Take(4).ToList()));
                list.RemoveRange(0, 4);
                groups.Add(new GroupTable("C", list.Take(4).ToList()));
                list.RemoveRange(0, 4);
                groups.Add(new GroupTable("D", list.Take(4).ToList()));
                list.RemoveRange(0, 4);
                groups.Add(new GroupTable("E", list.Take(4).ToList()));
                list.RemoveRange(0, 4);
                groups.Add(new GroupTable("F", list.Take(4).ToList()));
                list.RemoveRange(0, 4);
                groups.Add(new GroupTable("G", list.Take(4).ToList()));
                list.RemoveRange(0, 4);
                groups.Add(new GroupTable("H", list.Take(4).ToList()));
                list.RemoveRange(0, 4);

                WorldCupProgress.Instance.Clear();
                foreach (GroupTable gt in groups)
                {
                    gt.Sort();
                    WorldCupProgress.Instance.Add(gt.ToEmbeddedGroupInformation());
                }
            }
            CheckLastPlayerGame();

            leftArrow = new UIPicture("WC/leftArrow", new Rectangle(30, -10, 100, 90));
            leftArrow.Color = Color.Black;
            rightArrow = new UIPicture("WC/rightArrow", new Rectangle(670, -10, 100, 90));
            rightArrow.Color = Color.Black;
            startMatch = new UIJumpingUIPicture("whistle", new Rectangle(705, 405, 70, 70), 0.4f, 0.8f, 0.5f, .8f);
            _final_four = new List<Country>();
        }

        /// <summary>
        /// Sprawdza odstatnio grany mecz. Je�eli zapami�tany jest przeciwnik i wynik z nim
        /// to przyznaje odpowiednio punkty.
        /// </summary>
        private void CheckLastPlayerGame()
        {
            if (WorldCupProgress.Instance.CurrentOpponet != Country.UNKNOWN)
            {
                switch (WorldCupProgress.Instance.ResultWithCurrentOpponent)
                {
                    case EndedGame.Draw:
                        foreach (CountryTeam ct in groups[0].TeamInGroup)
                        {
                            if (ct.Country == WorldCupProgress.Instance.CurrentOpponet)
                                ct.DrawTheMatch();
                            if (ct.Country == WorldCupProgress.Instance.SelectedCountry)
                                ct.DrawTheMatch();
                        }
                        break;
                    case EndedGame.PlayerLost:
                        foreach (CountryTeam ct in groups[0].TeamInGroup)
                        {
                            if (ct.Country == WorldCupProgress.Instance.CurrentOpponet)
                                ct.WonTheMatch();
                            if (ct.Country == WorldCupProgress.Instance.SelectedCountry)
                                ct.LostTheMatch();
                        }
                        break;
                    case EndedGame.PlayerWon:
                        foreach (CountryTeam ct in groups[0].TeamInGroup)
                        {
                            if (ct.Country == WorldCupProgress.Instance.CurrentOpponet)
                                ct.LostTheMatch();
                            if (ct.Country == WorldCupProgress.Instance.SelectedCountry)
                                ct.WonTheMatch();
                        }
                        break;
                        /*
                         * Jesli nie wiemy jaki jest wynik ostatniego meczu to wynosimy si� st�d.
                         * Nie chcemy aby inni rozegrali mecze a gracz zosta� z ty�u !!.
                         */
                    case EndedGame.Undetermined:
                        return;
                }
                /*
                 * Po rezegranym meczu przez gracza symulujemy wyniki pozosta�ych dru�un w grupie.
                 * Najpierw czy�cimy zapisane dane, symulujemy rozgrywki i zapisujemy EmbeddedInf
                 */
                WorldCupProgress.Instance.Clear();
                foreach (GroupTable gt in groups)
                {
                    gt.NextRount();
                    WorldCupProgress.Instance.Add(gt.ToEmbeddedGroupInformation());
                }

                WorldCupProgress.Instance.CurrentOpponet = Country.UNKNOWN;
                WorldCupProgress.Instance.ResultWithCurrentOpponent = EndedGame.Undetermined;
            }
        }
        /// <summary>
        /// Ustala pary dru�yn do nastepnej fazy rozgrywek.
        /// </summary>
        private void UstalParyDoMeczy()
        {
            List<GroupTable> tempNewGroups = new List<GroupTable>();
            List<Country> tempCountriesList = new List<Country>();
            switch (WorldCupProgress.Instance.PhaseOfTheWorldCup)
            {
                case StateOfPlay.ONE_EIGHT_PHASE:
                    if (groups[0].SortedTeamInGroup[0].Country != WorldCupProgress.Instance.SelectedCountry &&
                        groups[0].SortedTeamInGroup[1].Country != WorldCupProgress.Instance.SelectedCountry)
                    {
                        gameFinishedForPlayersTeam = true;
                        return;
                    }
                    bool pierwszy = false;
                    if (groups[0].SortedTeamInGroup[0].Country == WorldCupProgress.Instance.SelectedCountry)
                        pierwszy = true;
                    /*
                     * Wybieramy osiem par z grup
                     */
                    if (pierwszy)
                    {
                        tempCountriesList.Add(groups[0].SortedTeamInGroup[0].Country);
                        tempCountriesList.Add(groups[1].SortedTeamInGroup[1].Country);
                        tempNewGroups.Add(new GroupTable("1st pair", tempCountriesList));
                        tempCountriesList.Clear();

                        tempCountriesList.Add(groups[0].SortedTeamInGroup[1].Country);
                        tempCountriesList.Add(groups[1].SortedTeamInGroup[0].Country);
                        tempNewGroups.Add(new GroupTable("2nd pair", tempCountriesList));
                        tempCountriesList.Clear();
                    }
                    else
                    {
                        tempCountriesList.Add(groups[0].SortedTeamInGroup[1].Country);
                        tempCountriesList.Add(groups[1].SortedTeamInGroup[0].Country);
                        tempNewGroups.Add(new GroupTable("1st pair", tempCountriesList));
                        tempCountriesList.Clear();

                        tempCountriesList.Add(groups[0].SortedTeamInGroup[0].Country);
                        tempCountriesList.Add(groups[1].SortedTeamInGroup[1].Country);
                        tempNewGroups.Add(new GroupTable("2nd pair", tempCountriesList));
                        tempCountriesList.Clear();
                    }

                    tempCountriesList.Add(groups[2].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[3].SortedTeamInGroup[1].Country);
                    tempNewGroups.Add(new GroupTable("3rd pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[2].SortedTeamInGroup[1].Country);
                    tempCountriesList.Add(groups[3].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("4th pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[4].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[5].SortedTeamInGroup[1].Country);
                    tempNewGroups.Add(new GroupTable("5th pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[4].SortedTeamInGroup[1].Country);
                    tempCountriesList.Add(groups[5].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("6th pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[6].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[7].SortedTeamInGroup[1].Country);
                    tempNewGroups.Add(new GroupTable("7th pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[6].SortedTeamInGroup[1].Country);
                    tempCountriesList.Add(groups[7].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("8th pair", tempCountriesList));
                    tempCountriesList.Clear();
                    /*
                     * Czy�cimy obecny stan grup
                     */
                    groups.Clear();
                    /*
                     * Tworzymy nowe grupy -> osiem par
                     */
                    groups = new List<GroupTable>(tempNewGroups);
                    /*
                     * Je�li gracz sko�czy� jako pierwszy to ustawiamy pierwsz� grup� na grup� gracza
                     */
                    groups[0].PlayersGroup = true;
                    /*
                     * �adujemy tekstury: czcionka, flagi, obramowanie tabeli, przykrycie flag
                     */
                    foreach (GroupTable gt in groups)
                        gt.LoadTexture(GameManager.Game.Content);
                    /*
                     * Czy�cimy stan WorldCup
                     */
                    WorldCupProgress.Instance.Clear();
                    /*
                     * Sortujemy grupy i dodajemy je do WorldCup
                     */
                    foreach (GroupTable gt in groups)
                    {
                        gt.Sort();
                        WorldCupProgress.Instance.Add(gt.ToEmbeddedGroupInformation());
                    }
                    /*
                     * Zapisujemy stan WorldCup
                     */
                    WorldCupProgress.Instance.Serialize();

                    break;
                case StateOfPlay.QUATER_FINAL_PHASE:
                    if (groups[0].SortedTeamInGroup[0].Country != WorldCupProgress.Instance.SelectedCountry &&
                        groups[1].SortedTeamInGroup[0].Country != WorldCupProgress.Instance.SelectedCountry)
                    {
                        gameFinishedForPlayersTeam = true;
                        return;
                    }
                    tempCountriesList.Add(groups[0].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[1].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("1st pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[2].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[3].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("2nd pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[4].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[5].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("3rd pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[6].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[7].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("4th pair", tempCountriesList));
                    tempCountriesList.Clear();

                    groups.Clear();
                    groups = new List<GroupTable>(tempNewGroups);
                    groups[0].PlayersGroup = true;
                    foreach (GroupTable gt in groups)
                        gt.LoadTexture(GameManager.Game.Content);
                    WorldCupProgress.Instance.Clear();
                    foreach (GroupTable gt in groups)
                    {
                        gt.Sort();
                        WorldCupProgress.Instance.Add(gt.ToEmbeddedGroupInformation());
                    }
                    WorldCupProgress.Instance.Serialize();

                    break;
                case StateOfPlay.SEMI_FINAL_PHASE:
                    if (groups[0].SortedTeamInGroup[0].Country != WorldCupProgress.Instance.SelectedCountry)
                    {
                        gameFinishedForPlayersTeam = true;
                        return;
                    }
                    tempCountriesList.Add(groups[0].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[1].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("1st pair", tempCountriesList));
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[2].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[3].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("2nd pair", tempCountriesList));
                    tempCountriesList.Clear();

                    groups.Clear();
                    groups = new List<GroupTable>(tempNewGroups);
                    groups[0].PlayersGroup = true;
                    foreach (GroupTable gt in groups)
                        gt.LoadTexture(GameManager.Game.Content);
                    WorldCupProgress.Instance.Clear();
                    foreach (GroupTable gt in groups)
                    {
                        gt.Sort();
                        WorldCupProgress.Instance.Add(gt.ToEmbeddedGroupInformation());
                    }
                    WorldCupProgress.Instance.Serialize();

                    break;
                case StateOfPlay.FINAL_PHASE:
                    tempCountriesList.Add(groups[0].SortedTeamInGroup[0].Country);
                    tempCountriesList.Add(groups[1].SortedTeamInGroup[0].Country);
                    tempNewGroups.Add(new GroupTable("Final match", tempCountriesList));
                    /*
                     * Je�li gracz jest w pierwszej grupie (tzn. FINA�) to ustawiamy j� na jego
                     */
                    if (tempCountriesList[0] == WorldCupProgress.Instance.SelectedCountry ||
                        tempCountriesList[1] == WorldCupProgress.Instance.SelectedCountry)
                        tempNewGroups[0].PlayersGroup = true;
                    tempCountriesList.Clear();

                    tempCountriesList.Add(groups[0].SortedTeamInGroup[1].Country);
                    tempCountriesList.Add(groups[1].SortedTeamInGroup[1].Country);
                    /*
                     * Je�li gracz jest w drugiej grupie (mecz o 3cie miejsce) to ustawiamy ja na jego
                     * i odwracamy kolejno�� grup -> wy�wietlona zostanie najpierw para w meczu o 3cie miejsce,
                     * a na nastepnej stronie b�dzie para o fina�
                     */
                    tempNewGroups.Add(new GroupTable("3rd place match", tempCountriesList));
                    if (tempCountriesList[0] == WorldCupProgress.Instance.SelectedCountry ||
                        tempCountriesList[1] == WorldCupProgress.Instance.SelectedCountry)
                    {
                        tempNewGroups[1].PlayersGroup = true;
                        tempNewGroups.Reverse();
                        WorldCupProgress.Instance.PhaseOfTheWorldCup = StateOfPlay.SMALL_FINAL_PHASE;
                    }
                    tempCountriesList.Clear();

                    groups.Clear();
                    groups = new List<GroupTable>(tempNewGroups);
                    foreach (GroupTable gt in groups)
                        gt.LoadTexture(GameManager.Game.Content);
                    WorldCupProgress.Instance.Clear();
                    foreach (GroupTable gt in groups)
                    {
                        gt.Sort();
                        WorldCupProgress.Instance.Add(gt.ToEmbeddedGroupInformation());
                    }
                    WorldCupProgress.Instance.Serialize();

                    break;
                default:
                    break;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            groups[index].Draw(spriteBatch);
            leftArrow.Draw(spriteBatch);
            rightArrow.Draw(spriteBatch);
            startMatch.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void LoadContent()
        {
            leftArrow.LoadTexture(GameManager.Game.Content);
            rightArrow.LoadTexture(GameManager.Game.Content);
            startMatch.LoadTexture(GameManager.Game.Content);
            menu.LoadTexture(GameManager.Game.Content);
            foreach (GroupTable gt in groups)
                gt.LoadTexture(GameManager.Game.Content);
        }

        public override void Update(GameTime gameTime)
        {
            startMatch.Update(gameTime);
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }
            /*
             * Koniec gry dla gracza w trybie WC (oznacza� mo�e odpadni�cie, lub po prostu zako�czenie trybu mistrzostw)
             */
            if (gameFinishedForPlayersTeam)
            {
                GameState[] states = GameManager.GetStates();
                foreach (GameState state in states)
                    GameManager.RemoveState(state);
                GameManager.AddState(new WcSummaryState(_final_four));
                startMatch.Pressed = false;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
                WorldCupProgress.Instance.CurrentOpponet = Country.UNKNOWN;
                WorldCupProgress.Instance.ResultWithCurrentOpponent = EndedGame.Undetermined;
                WorldCupProgress.Instance.Serialize();
                GameManager.AddState(new MainMenuState());
            }
            if (startMatch.Pressed)
            {
                /*
                 * Je�li wszystkie mecze zosta�y rozegrane
                 */
                if (groups[0].ThisPhaseMatchesHasEnded)
                {
                    /*
                     * Je�li to jest faza fina�u, lub meczu o 3cie miejsce
                     * to ustalamy ko�cow� kolejno�� zespo��w i ko�czymy tryb WC
                     */
                    if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.FINAL_PHASE)
                    {
                        _final_four.Add(groups[0].SortedTeamInGroup[0].Country);
                        _final_four.Add(groups[0].SortedTeamInGroup[1].Country);
                        _final_four.Add(groups[1].SortedTeamInGroup[0].Country);
                        _final_four.Add(groups[1].SortedTeamInGroup[1].Country);
                        gameFinishedForPlayersTeam = true;
                    }
                    if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.SMALL_FINAL_PHASE)
                    {
                        _final_four.Add(groups[1].SortedTeamInGroup[0].Country);
                        _final_four.Add(groups[1].SortedTeamInGroup[1].Country);
                        _final_four.Add(groups[0].SortedTeamInGroup[0].Country);
                        _final_four.Add(groups[0].SortedTeamInGroup[1].Country);
                        gameFinishedForPlayersTeam = true;
                    }
                    /*
                     * Je�li to by� kt�rykolwiek z innych etap�w mistrzostw to zmieniamy etap na nastepny
                     * i ustalamy nast�pne pary dru�yn do mecz�w.
                     */
                    if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.SEMI_FINAL_PHASE)
                    {
                        WorldCupProgress.Instance.PhaseOfTheWorldCup = StateOfPlay.FINAL_PHASE;
                        UstalParyDoMeczy();
                    }
                    if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.QUATER_FINAL_PHASE)
                    {
                        WorldCupProgress.Instance.PhaseOfTheWorldCup = StateOfPlay.SEMI_FINAL_PHASE;
                        UstalParyDoMeczy();
                    }
                    if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.ONE_EIGHT_PHASE)
                    {
                        WorldCupProgress.Instance.PhaseOfTheWorldCup = StateOfPlay.QUATER_FINAL_PHASE;
                        UstalParyDoMeczy();
                    }
                    if (WorldCupProgress.Instance.PhaseOfTheWorldCup == StateOfPlay.GROUP_PHASE)
                    {
                        WorldCupProgress.Instance.PhaseOfTheWorldCup = StateOfPlay.ONE_EIGHT_PHASE;
                        UstalParyDoMeczy();
                    }
                }
                /*
                 * Je�li jeszcze jakie� mecze musza zosta� rozegrane
                 */
                else
                {
                    /*
                     * Je�li mamy zapisany stan meczu to go wznawiamy
                     * w przeciwnym wypadku tworzymy nowy mecz.
                     */
                    if (!WorldCupProgress.Instance.CurrentMatchVariables.IsEmpty)
                    {
                        WorldCupProgress.Instance.LoadCurrentMatchState();
                        GameVariables.Instance.FirstPlayer.Coach = TeamCoach.HUMAN;
                        GameVariables.Instance.SecondPlayer.Coach = TeamCoach.CPU;
                        GameState[] states = GameManager.GetStates();
                        foreach (GameState state in states)
                            GameManager.RemoveState(state);

                        GameManager.AddState(new GameplayState(true));
                        GameManager.AddState(new PauseState(true));
                    }
                    else
                    {
                        GameState[] states = GameManager.GetStates();
                        foreach (GameState state in states)
                            GameManager.RemoveState(state);

                        /**
                         * Ustalamy kolory dla gracza i CPU
                         */
                        GameVariables.Instance.FirstPlayer.ShirtsColor = Color.Red;
                        GameVariables.Instance.SecondPlayer.ShirtsColor = Color.Green;
                        GameVariables.Instance.FirstPlayer.Coach = TeamCoach.HUMAN;
                        GameVariables.Instance.SecondPlayer.Coach = TeamCoach.CPU;
                        /*
                         * Ustalamy odpowiedni poziom trudno�ci
                         */
                        GameVariables.Instance.DiffLevel = Countries.countriesSkills[groups[0].GiveOpponent()];
                        /*
                         * Ustalamy przeciwnika
                         */
                        WorldCupProgress.Instance.CurrentOpponet = groups[0].GiveOpponent();

                        /*
                         * Je�li poza faz� grupow�, to gramy do limitu od 1 do 3ech bramek
                         * na du�ym boisku.
                         */
                        if (WorldCupProgress.Instance.PhaseOfTheWorldCup != StateOfPlay.GROUP_PHASE)
                        {
                            Random rand = new Random();
                            GameVariables.Instance.TypeOfField = PlayField.large;
                            GameVariables.Instance.IsLimitedByGoals = true;
                            GameVariables.Instance.GoalsLimit = rand.Next(1, 3);
                            GameVariables.Instance.TimeLeft = 0;
                            GameVariables.Instance.TotalTime = 0;
                            Statistics.Instance.ZaczynamKolejnyMecz();
                        }
                        /*
                         * Je�li gramy w fazie grupowej to gramy na ma�ym boisku i na czas -> 5 minut (300 sekund)
                         */
                        else
                        {
                            Random rand = new Random();
                            GameVariables.Instance.TypeOfField = PlayField.classic;
                            GameVariables.Instance.IsLimitedByGoals = false;
                            GameVariables.Instance.TimeLeft = PLAY_TIME;
                            GameVariables.Instance.TotalTime = PLAY_TIME; 
                            Statistics.Instance.ZaczynamKolejnyMecz();
                        }
                        GameManager.AddState(new GameplayState(false));
                    }
                }
                startMatch.Pressed = false;
            }
        }


        public void ButtonClicked(GameTime gameTime)
        {
            menuCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (menuCooldown <= 0.0f)
            {
                menuCooldown = MENUCOOLDOWN;
                clickAnimationOngoing = false;
            }
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (clickAnimationOngoing)
                return;

            if (input.Gestures.Count > 0)
            {
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    menu.WasPressed(input.Gestures[0].Position);
                    if (menu.PressedButton != ButtonType.None)
                    {
                        clickAnimationOngoing = true;
                        AudioManager.PlaySound("selected");
                    }
                    if (startMatch.WasPressed(input.Gestures[0].Position))
                    {
                        if (index != 0)
                            index = 0;
                        else
                            startMatch.Pressed = true;
                    }
                    if (leftArrow.WasPressed(input.Gestures[0].Position))
                    {
                        if (index == 0)
                            index = groups.Count - 1;
                        else
                            index--;
                    }
                    if (rightArrow.WasPressed(input.Gestures[0].Position))
                    {
                        if (index == (groups.Count -1))
                            index = 0;
                        else
                            index++;
                    }              
                }
                if (input.Gestures[0].GestureType == GestureType.Flick)
                {
                    if (input.Gestures[0].Delta.X >= 1000)
                    {
                        if (index == 0)
                            index = groups.Count - 1;
                        else
                            index--;
                        AudioManager.PlaySound("selected");
                    }
                    else if (input.Gestures[0].Delta.X <= -1000)
                    {
                        if (index == (groups.Count - 1))
                            index = 0;
                        else
                            index++;
                        AudioManager.PlaySound("selected");
                    }
                }
            }
        }
    }
}
