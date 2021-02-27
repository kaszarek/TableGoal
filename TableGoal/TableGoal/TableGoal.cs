using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone;
using System.Windows;
using Microsoft.Advertising.Mobile.Xna;

namespace TableGoal
{
    public static class ApplicationLicence
    {
        public static bool IsTrialMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return Guide.IsTrialMode;
#endif
            }
        }

    }
    
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TableGoal : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GameStatesManager gameStatesManager;
        private readonly string storageFile = "TableSoccer.dat";

        public TableGoal()
        {
            AudioManager.Initialize(this);

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            // Frame rate is 26 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(384615);
            
            graphics.IsFullScreen = true;

            gameStatesManager = new GameStatesManager(this);
            Components.Add(gameStatesManager);

            AdGameComponent.Initialize(this, "you need your AD id");
            Components.Add(AdGameComponent.Current);
            
            if (PhoneApplicationService.Current.StartupMode == StartupMode.Launch)
            {
                gameStatesManager.AddState(new SplashScreenState());
            }
            else
            {
                gameStatesManager.AddState(new SplashScreenState());
            }

            AudioManager.LoadSounds();
            AudioManager.LoadMusic();
            PhoneApplicationService.Current.Activated += GameActivated;
            PhoneApplicationService.Current.Deactivated += GameDeactivated;
            PhoneApplicationService.Current.Closing += GameClosing;
            PhoneApplicationService.Current.Launching += GameLaunching;
        }

        private void RemoveAllStates()
        {
            GameState[] states = gameStatesManager.GetStates();
            foreach (GameState state in states)
                gameStatesManager.RemoveState(state);
        }
        
        private void RemoveMainMenuState()
        {
            GameState[] states = gameStatesManager.GetStates();
            foreach (GameState state in states)
                if (state is MainMenuState)
                {
                    gameStatesManager.RemoveState(state);
                    break;
                }
        }

        private void RemovePauseState()
        {
            GameState[] states = gameStatesManager.GetStates();
            foreach (GameState state in states)
                if (state is PauseState)
                {
                    gameStatesManager.RemoveState(state);
                    break;
                }
        }

        public static bool StatsOK = true;
        public static bool GameVarOK = true;
        public static bool WcProgressOK = true;

        public static List<PlayerInfo> Players = new List<PlayerInfo>();

        private static MultiGamePlay _gamePlay = null;
        public static MultiGamePlay GamePlay
        {
            get
            {
                // Delay creation of GamePlay until necessary
                if (_gamePlay == null)
                    _gamePlay = new MultiGamePlay();

                return _gamePlay;
            }
            set
            {
                _gamePlay = value;
            }
        }

        #region Tombstoning

        /// <summary>
        /// Saves the full state to the state object and the persistent state to 
        /// isolated storage.
        /// </summary>
        void GameDeactivated(object sender, DeactivatedEventArgs e)
        {
            if (_gamePlay != null)
            {
                GamePlay.Leave(true);
                _gamePlay = null;
            }
            if (GlobalMultiplayerContext.warpClient != null)
            {
                GlobalMultiplayerContext.roomsIDs.Clear();
                GameState[] states = gameStatesManager.GetStates();
                foreach (GameState state in states)
                {
                    if (state is GlobalMultiGameplayState)
                    {
                        (state as GlobalMultiGameplayState).UnregisterEvents();
                        continue;
                    }
                    if (state is GlobalMultiEndGameState)
                    {
                        (state as GlobalMultiEndGameState).UnregisterEvents();
                        continue;
                    }
                    if (state is GlobalMultiRoomsState)
                    {
                        (state as GlobalMultiRoomsState).UnregisterEvents();
                        continue;
                    }
                    if (state is GlobalMultiLobbyState)
                    {
                        (state as GlobalMultiLobbyState).UnregisterEvents(true);
                        continue;
                    }
                    if (state is GlobalMultiJoinState)
                    {
                        (state as GlobalMultiJoinState).UnregisterEvents();
                        continue;
                    }
                    if (state is GlobalMultiHostState)
                    {
                        (state as GlobalMultiHostState).UnregisterEvents();
                        continue;
                    }
                }
                GlobalMultiplayerContext.warpClient.Disconnect();
                GlobalMultiProvider.IsConnected = false;
            }

            /*
             * Przed zapisaniem stanu do State i do Isolated Storage
             * czy�cimy w�a�nie te dwie rzeczy.
             */
            PhoneApplicationService.Current.State.Clear();
            CleanIsolatedStorage();
            GameplayState gameplayState = GetGameplayState();
            if (gameplayState != null)
            {
                gameplayState.UnregisterEventsFromRemotePlayer();
            }
            
            /*
             * Jesli gra ju� si� zacz�a i nie by� to multi przez WIFI, to zapisujemy i do State, i do Isolated Storage
             * Jak gra otrzyma wydarzenie GameActivated to wtedy wczyta tylko stan ze State
             * Druga opca to taka, �e gra mo�e zosta� w ko�cu zamkni�ta przez system (przez otwarcie wi�kszej ilo�ci 3rd party apps).
             * Wtedy przy uruchomieniu otrzyma wydarzenie GameLaunching i wczyta stan gry z pliku.
             */
            if (gameplayState != null && !GameVariables.Instance.IsWiFiGame())
            {
                SaveToStateObject();
                SaveToIsolatedStorage();
            }
                /*
                 * Je�li nie ma w bierz�cej sesji aplikacji gry na biosku to ustawiamy kolor gracza i przeciwnika
                 * na zloty. Co zonacza, �e nie ma rozpocz�tej gry i mozna zacz�� od Main menu.
                 */
            else
            {
                GameVariables.Instance.FirstPlayer.ShirtsColor = Color.Gold;
                GameVariables.Instance.SecondPlayer.ShirtsColor = Color.Gold;
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
                SaveToStateObject();
                SaveToIsolatedStorage();
            }

            WorldCupProgress.Instance.ClearIsolatedStorageRelatedData();
            WorldCupProgress.Instance.Serialize();
            Statistics.Instance.Serialize();
        }

        /// <summary>
        /// Loads the full state from the state object.
        /// </summary>
        void GameActivated(object sender, ActivatedEventArgs e)
        {
            if (e.IsApplicationInstancePreserved)
            {
                if (Players.Count > 0)
                {
                    Players.Clear();
                    RemoveAllStates();
                    gameStatesManager.AddState(new MainMenuState());
                }
                if (GlobalMultiplayerContext.warpClient != null)
                {
                    RemoveAllStates();
                    gameStatesManager.AddState(new MainMenuState());
                }
                goto cleaning;
            }
            /*
             * Wczytujemy statystyki aby by� gotowym na jakiekolwiek ich modyfikacje.
             */
            Statistics.Instance.Deserialize();
            if (!StatsOK)
                Statistics.Instance.ClearIsolatedStorageRelatedData();
            LoadFromStateObject();
            if (GameVariables.Instance.WorldCupStarted)
            {
                WorldCupProgress.Instance.Deserialize();
                if (!WcProgressOK)
                    WorldCupProgress.Instance.ClearIsolatedStorageRelatedData();
            }
            /*
             * Sprawdzamy czy kt�ry� z kolor�w nie jest z�oty. W gruncie rzeczy albo obydwa, albo �aden b�d� z�ote.
             * Je�li kt�rys jest z�oty, znaczy to, �e nie ma co wznawia� gameplay'u i zaczynamy od main menu.
             * Po tym czy�cimy State
             */
            if (GameVariables.Instance.FirstPlayer.ShirtsColor == Color.Gold ||
                GameVariables.Instance.SecondPlayer.ShirtsColor == Color.Gold)
            {
                goto cleaning;
            }
            /*
             * Je�li dotarli�my tutaj to znaczy, �e jednak mecz si� toczy.
             * Usuwamy wszystkie stany, kt�re s� na li�cie przed dodaniem Gamepley'u i Pause
             */
            RemoveAllStates();

            gameStatesManager.AddState(new GameplayState(true));
            gameStatesManager.AddState(new PauseState(true));

            if (!GameVariables.Instance.IsLimitedByGoals)
            {
                /*
                 * Je�li czas si� sko�czy� to usuwamy PauseState, je�li jest ju� -> 
                 * co znaczy, �e gra tez jest, i teraz gra sama sie zakonczy pokazuj�c ko�cz�cy ekran.
                 */
                if (GameVariables.Instance.TimeLeft <= 0)
                {
                    RemovePauseState();
                }
            }
            else
            {
                /*
                 * Je�li osi�gni�to limit bramek to usuwamy PauseState, je�li jest ju� -> 
                 * co znaczy, �e gra tez jest, i teraz gra sama rozkmini, �e sie sko�czy�a 
                 * i poka�e ko�cz�cy ekran.
                 */
                if (GameVariables.Instance.FirstPlayer.Goals == GameVariables.Instance.GoalsLimit ||
                    GameVariables.Instance.SecondPlayer.Goals == GameVariables.Instance.GoalsLimit)
                {
                    RemovePauseState();
                }
            }
            cleaning:
            /*
             * Czy�cimy teraz Stan.
             */
            //PhoneApplicationService.Current.State.Clear();
            /*
             * I czy�cimy isolated storage
             */
            CleanIsolatedStorage();
            if (!AudioManager.Instance.PlaybackAlreadyStarted)
                AudioManager.PlayMusic("crowd");
        }

        /// <summary>
        /// Saves persistent state to isolated storage.
        /// </summary>
        void GameClosing(object sender, ClosingEventArgs e)
        {
            if (_gamePlay != null)
            {
                GamePlay.Leave(true);
                _gamePlay = null;
            }
            if (GlobalMultiplayerContext.warpClient != null)
            {
                if (GlobalMultiProvider.IsConnected)
                {
                    GlobalMultiplayerContext.warpClient.Disconnect();
                }
            }
            WorldCupProgress.Instance.Serialize();
            GameplayState gameplayState = GetGameplayState();
            if (gameplayState == null)
            {
                GameVariables.Instance.FirstPlayer.ShirtsColor = Color.Gold;
                GameVariables.Instance.SecondPlayer.ShirtsColor = Color.Gold;
            }
            Statistics.Instance.Serialize();
            SaveToIsolatedStorage();
        }

        /// <summary>
        /// Loads persistent state from isolated storage.
        /// </summary>
        void GameLaunching(object sender, LaunchingEventArgs e)
        {
            /*
             * Wczytujemy statystyki aby by� gotowym na jakiekolwiek ich modyfikacje.
             */
            Statistics.Instance.Deserialize();
            if (!StatsOK)
                Statistics.Instance.ClearIsolatedStorageRelatedData();
            LoadFromIsolatedStorage();
            if (!GameVarOK)
                CleanIsolatedStorage();
            /*
             * To w razie jakby poprzednio jakim� cudem wystapi� wyj�tek podczas gry przez WiFi
             * i dane z GameVariables zosta�y zapisane.
             * Je�li uruchamiamy gr� i okazuje si�, �e by� ostatnio grany tryb Multi przez WiFi to
             * resetujemy 
             */
            if (GameVariables.Instance.IsWiFiGame())
            {
                GameVariables.Instance.RestartGame();
                GameVariables.Instance.ResetVariables();
            }

            if (GameVariables.Instance.WorldCupStarted)
            {
                WorldCupProgress.Instance.Deserialize();
                if (!WcProgressOK)
                    WorldCupProgress.Instance.ClearIsolatedStorageRelatedData();
            }
            /*
             * Je�li, k�try� z graczy ma kolor koszulki z�oty to czy�cimy IsolatedStarage i nic nie wznawiamy.
             */
            if (GameVariables.Instance.FirstPlayer.ShirtsColor == Color.Gold ||
                GameVariables.Instance.SecondPlayer.ShirtsColor == Color.Gold)
                goto cleaning;
            /*
             * Je�li tu doatarlismy to znaczy, �e mecz si� toczy.
             * Usuwamy dodany przez konstruktor MainMenu stan.
             */
            RemoveMainMenuState();
            /*
             * Je�li gra nie jest do limitu bramek => tylko do limitu czasu
             */
            if (!GameVariables.Instance.IsLimitedByGoals)
            {
                /*
                 * je�li ilo�� ruch�w jest r�na od zera (co� juz wykonano) LUB
                 * czas kt�ry pozosta� jest mniejszy, r�wny ca�kowitemu czasowi (czyli gra co najmniej si� zacz�a)
                 * to wznawiamy do gry + pause
                 */
                if (GameVariables.Instance.PerformedMoves.Count != 0 ||
                    GameVariables.Instance.TimeLeft <= GameVariables.Instance.TotalTime &&
                    GameVariables.Instance.TotalTime > 0)
                {
                    GameState[] states = gameStatesManager.GetStates();
                    foreach (GameState state in states)
                        gameStatesManager.RemoveState(state);
                    gameStatesManager.AddState(new GameplayState(true));
                    gameStatesManager.AddState(new PauseState(true));

                    /*
                     * Je�li jednak czas min��, czyli gra si� sko�czy�a to usuwamy PauseState
                     * i pokazujemy ko�cowy ekran gry.
                     * */
                    if (GameVariables.Instance.TimeLeft <= 0)
                    {
                        states = gameStatesManager.GetStates();
                        foreach (GameState state in states)
                            if (state is PauseState)
                            {
                                gameStatesManager.RemoveState(state);
                                /*
                                 * Gra sama wykmini, �e gra si� sko�czy�a.
                                 */
                                //gameStatesManager.AddState(new GameFinishedState(true));
                                break;
                            }
                    }
                }
            }
            else
            {
                /*
                 * Je�li gra si� zacz�a
                 */
                if (GameVariables.Instance.TimeLeft >= 0 ||
                    GameVariables.Instance.FirstPlayer.Goals <= GameVariables.Instance.GoalsLimit &&
                    GameVariables.Instance.SecondPlayer.Goals <= GameVariables.Instance.GoalsLimit)
                {
                    GameState[] states = gameStatesManager.GetStates();
                    foreach (GameState state in states)
                        gameStatesManager.RemoveState(state);
                    gameStatesManager.AddState(new GameplayState(true));
                    gameStatesManager.AddState(new PauseState(true));

                    /*
                     * Je�li jednak kto� osi�gna� limit bramek, czyli gra si� sko�czy�a to usuwamy PauseState
                     * i pokazujemy ko�cowy ekran gry.
                     * */
                    if (GameVariables.Instance.FirstPlayer.Goals == GameVariables.Instance.GoalsLimit ||
                        GameVariables.Instance.SecondPlayer.Goals == GameVariables.Instance.GoalsLimit)
                    {
                        states = gameStatesManager.GetStates();
                        foreach (GameState state in states)
                            if (state is PauseState)
                            {
                                gameStatesManager.RemoveState(state);
                                /*
                                 * Nie trzeba dodawa� GameFinishedState, bo gra sama doda ten stan !!
                                 */
                                //gameStatesManager.AddState(new GameFinishedState(true));
                                break;
                            }
                    }
                }
            }
            cleaning:
            /*
             * I teraz czy�cimy IsolatedStotrage
             */
            CleanIsolatedStorage();
        }

        #region Key for saving State
        private readonly string keyMusic = "music";
        private readonly string keySounds = "sounds";
        private readonly string keyMoves = "moves";
        private readonly string keyCurrent = "current";
        private readonly string keyFirst = "first";
        private readonly string keySecond = "second";
        private readonly string keyDiffLevel = "difficultyLevel";
        private readonly string keyTotalTime = "totalTime";
        private readonly string keyTimeLeft = "timeLeft";
        private readonly string keyTypeOfField = "field";
        private readonly string keyIsLimitedByGoals = "goalsLimited";
        private readonly string keyGoalLimit = "goalsLimit";
        private readonly string keySelectedCountry = "selectedCountry";
        private readonly string keyWorldCupStarted = "worldCup";
        private readonly string keyWorldCupMatchIsActive = "worldCupActive";
        #endregion
        /// <summary>
        /// Zapisanie stanu gry z <code>GameVariables</code> do <code>PhoneApplicationService</code>.
        /// </summary>
        private void SaveToStateObject()
        {
            PhoneApplicationService.Current.State[keyMusic] =
                GameVariables.Instance.MusicOn;
            PhoneApplicationService.Current.State[keySounds] =
                GameVariables.Instance.SoundsOn;
            PhoneApplicationService.Current.State[keyMoves] =
                GameVariables.Instance.PerformedMoves;
            PhoneApplicationService.Current.State[keyFirst] =
                GameVariables.Instance.FirstPlayer;
            PhoneApplicationService.Current.State[keySecond] =
                GameVariables.Instance.SecondPlayer;
            PhoneApplicationService.Current.State[keyCurrent] =
                GameVariables.Instance.CurrentPlayer;
            PhoneApplicationService.Current.State[keyDiffLevel] =
                GameVariables.Instance.DiffLevel;
            PhoneApplicationService.Current.State[keyTotalTime] =
                GameVariables.Instance.TotalTime;
            PhoneApplicationService.Current.State[keyTimeLeft] =
                GameVariables.Instance.TimeLeft;
            PhoneApplicationService.Current.State[keyTypeOfField] =
                GameVariables.Instance.TypeOfField;
            PhoneApplicationService.Current.State[keyIsLimitedByGoals] =
                GameVariables.Instance.IsLimitedByGoals;
            PhoneApplicationService.Current.State[keyGoalLimit] =
                GameVariables.Instance.GoalsLimit;
            PhoneApplicationService.Current.State[keySelectedCountry] =
                GameVariables.Instance.SelectedCountry;
            PhoneApplicationService.Current.State[keyWorldCupStarted] =
                GameVariables.Instance.WorldCupStarted;
            PhoneApplicationService.Current.State[keyWorldCupMatchIsActive] =
                GameVariables.Instance.ActiveWorldCupMatch;
        }
        /// <summary>
        /// Uzupe�nienie <code>GameVariables</code> z <code>PhoneApplicationService</code>.
        /// </summary>
        private void LoadFromStateObject()
        {
            if (PhoneApplicationService.Current.State.ContainsKey(keyMusic))
            {
                GameVariables.Instance.MusicOn = (bool)PhoneApplicationService.Current.State[keyMusic];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keySounds))
            {
                GameVariables.Instance.SoundsOn = (bool)PhoneApplicationService.Current.State[keySounds];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyMoves))
            {
                GameVariables.Instance.PerformedMoves = (List<GameMove>)PhoneApplicationService.Current.State[keyMoves];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyCurrent))
            {
                GameVariables.Instance.CurrentPlayer = (Team)PhoneApplicationService.Current.State[keyCurrent];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyFirst))
            {
                GameVariables.Instance.FirstPlayer = (Team)PhoneApplicationService.Current.State[keyFirst];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keySecond))
            {
                GameVariables.Instance.SecondPlayer = (Team)PhoneApplicationService.Current.State[keySecond];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyDiffLevel))
            {
                GameVariables.Instance.DiffLevel = (DifficultyLevel)PhoneApplicationService.Current.State[keyDiffLevel];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyTotalTime))
            {
                GameVariables.Instance.TotalTime = (int)PhoneApplicationService.Current.State[keyTotalTime];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyTimeLeft))
            {
                GameVariables.Instance.TimeLeft = (int)PhoneApplicationService.Current.State[keyTimeLeft];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyTypeOfField))
            {
                GameVariables.Instance.TypeOfField = (PlayField)PhoneApplicationService.Current.State[keyTypeOfField];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyIsLimitedByGoals))
            {
                GameVariables.Instance.IsLimitedByGoals = (bool)PhoneApplicationService.Current.State[keyIsLimitedByGoals];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyGoalLimit))
            {
                GameVariables.Instance.GoalsLimit = (int)PhoneApplicationService.Current.State[keyGoalLimit];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keySelectedCountry))
            {
                GameVariables.Instance.SelectedCountry = (Country)PhoneApplicationService.Current.State[keySelectedCountry];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyWorldCupStarted))
            {
                GameVariables.Instance.WorldCupStarted = (bool)PhoneApplicationService.Current.State[keyWorldCupStarted];
            }
            if (PhoneApplicationService.Current.State.ContainsKey(keyWorldCupMatchIsActive))
            {
                GameVariables.Instance.ActiveWorldCupMatch = (bool)PhoneApplicationService.Current.State[keyWorldCupMatchIsActive];
            }
        }

        /// <summary>
        /// Zapisanie do pliku <code>GameVariables</code>.
        /// </summary>
        private void SaveToIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //using (IsolatedStorageFileStream fileStream
                //    = isolatedStorageFile.CreateFile(storageFile))
                using (IsolatedStorageFileStream fileStream
                 = isolatedStorageFile.OpenFile(storageFile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        GameVariables.Instance.Serialize(streamWriter);
                    }
                }
            }
        }
        /// <summary>
        /// Wczytanie z pliku danych do <code>GameVariables</code>.
        /// </summary>
        private void LoadFromIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(storageFile))
                {
                    using (IsolatedStorageFileStream fileStream
                        = isolatedStorageFile.OpenFile(storageFile, FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            GameVariables.Instance.Deserialize(streamReader);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Usuni�cie zapisanych danych z <code>GameVariables</code>.
        /// </summary>
        private void CleanIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(storageFile))
                    isolatedStorageFile.DeleteFile(storageFile);
            }
            TableGoal.GameVarOK = true;
        }
        /// <summary>
        /// Zwraca <code>GameplayState</code> ze wszystkich stan�w w danym momencie.
        /// </summary>
        /// <returns>Stan <code>GameplayState</code>, lub <code>null</code>.</returns>
        private GameplayState GetGameplayState()
        {
            var states = gameStatesManager.GetStates();
            foreach (var state in states)
            {
                if (state is GameplayState)
                {
                    return state as GameplayState;
                }
            }
            return null;
        }

        #endregion

    }
}
