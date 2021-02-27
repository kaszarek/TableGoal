using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.IO.IsolatedStorage;

namespace TableGoal
{
    public sealed class WorldCupProgress
    {
        static readonly string WcDataStorageFile = "WorldCupData.dat";

        List<EmbeddedGroupInformation> groups;
        /// <summary>
        /// Zwraca lub ustawia dane dotyczace grup.
        /// </summary>
        public List<EmbeddedGroupInformation> Groups
        {
            get { return groups; }
            set { groups = value; populated = true; }
        }
        
        bool populated;
        /// <summary>
        /// Czy grupy zosta³y uzupe³nione
        /// </summary>
        public bool Populated
        {
            get { return populated; }
            set { populated = value; }
        }

        Country selectedCountry;
        /// <summary>
        /// Wybrany kraj przez gracza
        /// </summary>
        public Country SelectedCountry
        {
            get { return selectedCountry; }
            set
            {
                selectedCountry = value;
                GameVariables.Instance.SelectedCountry = selectedCountry;
            }
        }

        Country currentOpponet;
        /// <summary>
        /// Przeciwnik gracza
        /// </summary>
        public Country CurrentOpponet
        {
            get { return currentOpponet; }
            set { currentOpponet = value; }
        }

        EndedGame resultWithCurrentOpponent;
        /// <summary>
        /// Wynik ostatniego meczu
        /// </summary>
        public EndedGame ResultWithCurrentOpponent
        {
            get { return resultWithCurrentOpponent; }
            set { resultWithCurrentOpponent = value; }
        }

        StateOfPlay phaseOfTheWorldCup;
        /// <summary>
        /// Stan mistrzostw - grupa, 1/8, 1/4, 1/2, fina³, ma³y fina³.
        /// </summary>
        public StateOfPlay PhaseOfTheWorldCup
        {
            get { return phaseOfTheWorldCup; }
            set { phaseOfTheWorldCup = value; }
        }

        bool worldCupMatchSaved;
        /// <summary>
        /// Ustawia, lub odczytuje czy mecz w trybie World Cup jest zapisany.
        /// </summary>
        public bool WorldCupMatchSaved
        {
            get { return worldCupMatchSaved; }
            set { worldCupMatchSaved = value; }
        }

        /// <summary>
        /// Czyœci zapamiêtane grupy i usuwa dane z IsolatedStorage. Ustawia zmienn¹ populated na false.
        /// </summary>
        public void Clear()
        {
            groups.Clear();
            worldCupMatchSaved = false;
            currentMatchVariables.Clear();

            populated = false;
            ClearIsolatedStorageRelatedData();
        }
        /// <summary>
        /// Dodaj informacje o grupie.
        /// </summary>
        /// <param name="emInfo">Informacje o grupie</param>
        public void Add(EmbeddedGroupInformation emInfo)
        {
            groups.Add(emInfo);
            switch (WorldCupProgress.Instance.PhaseOfTheWorldCup)
            {
                case StateOfPlay.GROUP_PHASE:
                    if (groups.Count == 8)
                        populated = true;
                    else
                        populated = false;
                    break;
                case StateOfPlay.ONE_EIGHT_PHASE:
                    if (groups.Count == 8)
                        populated = true;
                    else
                        populated = false;
                    break;
                case StateOfPlay.QUATER_FINAL_PHASE:
                    if (groups.Count == 4)
                        populated = true;
                    else
                        populated = false;
                    break;
                case StateOfPlay.SEMI_FINAL_PHASE:
                    if (groups.Count == 2)
                        populated = true;
                    else
                        populated = false;
                    break;
                case StateOfPlay.FINAL_PHASE:
                    if (groups.Count == 2)
                        populated = true;
                    else
                        populated = false;
                    break;
                case StateOfPlay.SMALL_FINAL_PHASE:
                    if (groups.Count == 2)
                        populated = true;
                    else
                        populated = false;
                    break;
            }
        }

        EmbeddedGameVariables currentMatchVariables;

        public EmbeddedGameVariables CurrentMatchVariables
        {
            get { return currentMatchVariables; }
            set { currentMatchVariables = value; }
        }
        /// <summary>
        /// Zapisuje stan meczu do obiektu <code>EmbeddedGameVariables</code> i serializuje.
        /// </summary>
        public void SaveCurrentMatchState()
        {
            currentMatchVariables.FirstTeam = new Team(GameVariables.Instance.FirstPlayer);
            currentMatchVariables.SecondTeam = new Team(GameVariables.Instance.SecondPlayer);
            currentMatchVariables.CurrentTeam = new Team(GameVariables.Instance.CurrentPlayer);
            currentMatchVariables.TypeOfField = GameVariables.Instance.TypeOfField;
            currentMatchVariables.Moves = new List<GameMove>(GameVariables.Instance.PerformedMoves);
            currentMatchVariables.DiffLevel = GameVariables.Instance.DiffLevel;
            currentMatchVariables.TotalTime = GameVariables.Instance.TotalTime;
            currentMatchVariables.TimeLeft = GameVariables.Instance.TimeLeft;
            currentMatchVariables.IsGoalLimited = GameVariables.Instance.IsLimitedByGoals;
            currentMatchVariables.GoalsLimit = GameVariables.Instance.GoalsLimit;
            currentMatchVariables.Opponent = WorldCupProgress.Instance.CurrentOpponet;
            currentMatchVariables.IsEmpty = false;
            worldCupMatchSaved = true;
            Serialize();
        }
        /// <summary>
        /// Jeœli obiekt z zapisem stanu gry jest wype³niony to wczytujemy grê.
        /// </summary>
        public void LoadCurrentMatchState()
        {
            if (currentMatchVariables.IsEmpty)
                return;
            if (!worldCupMatchSaved)
                return;
            /*
             * Najpierw trzeba ustaliæ poziom trudnoœci bo w nim ustalane jest kto ma ruch.
             * To mo¿e byæ b³edne w odniesieniu do tego, ¿e wznawiamy grê !!
             */
            GameVariables.Instance.DiffLevel = currentMatchVariables.DiffLevel;
            GameVariables.Instance.FirstPlayer = currentMatchVariables.FirstTeam;
            GameVariables.Instance.SecondPlayer = currentMatchVariables.SecondTeam;
            GameVariables.Instance.CurrentPlayer = currentMatchVariables.CurrentTeam;
            GameVariables.Instance.TypeOfField = currentMatchVariables.TypeOfField;
            GameVariables.Instance.PerformedMoves = new List<GameMove>(currentMatchVariables.Moves);
            GameVariables.Instance.TotalTime = currentMatchVariables.TotalTime;
            GameVariables.Instance.TimeLeft = currentMatchVariables.TimeLeft;
            GameVariables.Instance.IsLimitedByGoals = currentMatchVariables.IsGoalLimited;
            GameVariables.Instance.GoalsLimit = currentMatchVariables.GoalsLimit;
            WorldCupProgress.Instance.CurrentOpponet = currentMatchVariables.Opponent;
            currentMatchVariables.IsEmpty = true;
            worldCupMatchSaved = false;
        }

        public void ClearCurrentMatchState()
        {
            currentMatchVariables.Clear();
        }

        public void CheckLoadedCurrentMatchState()
        {
            if (currentMatchVariables.FirstTeam.ShirtsColor == Color.Gold ||
                currentMatchVariables.SecondTeam.ShirtsColor == Color.Gold)
            {
                currentMatchVariables.Clear();
            }
        }


        public WorldCupProgress()
        {
            groups = new List<EmbeddedGroupInformation>();
            populated = false;
            CurrentOpponet = Country.UNKNOWN;
            resultWithCurrentOpponent = EndedGame.Undetermined;
            currentMatchVariables = new EmbeddedGameVariables();
        }

        public static WorldCupProgress Instance
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
            internal static WorldCupProgress _gameVars = new WorldCupProgress();
        }
        /// <summary>
        /// Zapisz stan mistrzostw do pliku.
        /// </summary>
        public void Serialize()
        {
            using (IsolatedStorageFile isolatedStorageFile
                   = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //using (IsolatedStorageFileStream fileStream
                //    = isolatedStorageFile.CreateFile(WcDataStorageFile))
                using (IsolatedStorageFileStream fileStream
                    = isolatedStorageFile.OpenFile(WcDataStorageFile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(WorldCupProgress));
                        using (TextWriter tw = streamWriter)
                        {
                            serializer.Serialize(tw, this);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Odczytaj stan mistrzostw z pliku.
        /// </summary>
        public void Deserialize()
        {
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(WcDataStorageFile))
                {
                    using (IsolatedStorageFileStream fileStream
                        = isolatedStorageFile.OpenFile(WcDataStorageFile, FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            //string a = streamReader.ReadToEnd();
                            streamReader.BaseStream.Position = 0;
                            XmlSerializer deserializer = new XmlSerializer(typeof(WorldCupProgress));
                            WorldCupProgress wcp;
                            using (TextReader tr = streamReader)
                            {
                                try
                                {
                                    wcp = (WorldCupProgress)deserializer.Deserialize(tr);
                                    WorldCupProgress.Instance = wcp;
                                    GameVariables.Instance.SelectedCountry = this.selectedCountry;
                                    if (groups.Count != 0)
                                        populated = true;
                                }
                                catch (Exception ex)
                                {
                                    populated = false;
#if DEBUG
                                    Debug.WriteLine(String.Format("WORLD_CUP_PROGRESS:Deserialize - Exception:\n", ex.Message));
#endif
                                    TableGoal.WcProgressOK = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Wyczyœæ plik z zapamiêtanymi danymi mistrzostw.
        /// </summary>
        public void ClearIsolatedStorageRelatedData()
        {
            using (IsolatedStorageFile isolatedStorageFile
                   = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (isolatedStorageFile.FileExists(WcDataStorageFile))
                        isolatedStorageFile.DeleteFile(WcDataStorageFile);
                }
                catch (Exception ex)
                {
                    /*
                     * Mo¿e po prostu jak nie mo¿na usun¹æ pliku
                     * to otworzyæ i zapisaæ do niego spacjê?
                     * Albo zostawiæ i przy zapisie dane bêd¹ po prostu nadpisane.
                     */
#if DEBUG
                    Debug.WriteLine(String.Format("WORLD_CUP_PROGRESS:Clear storage - Exception:\n", ex.Message));
#endif
                }
            }
            TableGoal.WcProgressOK = true;
        }
    }
}
