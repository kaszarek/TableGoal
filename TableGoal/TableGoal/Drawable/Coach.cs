using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;

namespace TableGoal
{
    /// <summary>
    /// Kody piptalk
    /// </summary>
    public enum CoachPipTalkCodes 
    {
        GreetingsHello,
        GreetingsGoodDay,
        GreetingsGreetings,
        GreetingsHi,
        GreetingsHiThere,
        GreetingsHowdy,
        GreetingsGoEasyOnMe,
        GreetingsBringItOn,
        GreetingsPLSiemka,
        GreetingsPLHej,
        GreetingsPLWitam,
        GreetingsPLJakLeci,
        PraiseNice,
        PraiseNiceMove,
        PraiseWelldone,
        PriseWellPlayed,
        PraiseYouAreOnFire,
        PraiseCannotBe,
        PraiseGreatMove,
        PriseSpeechless,
        PraiseShocked,
        CheersTakeThat,
        CheersTongueOut,
        CheersSoooLucky,
        CheersSleepingZzz,
        CheersSmile,
        CheersGetReady,
        CheersGo,
        CheersTime3_2_1,
        CheersDefence,
        CheersEasyPeasy,
        CheersEpicMove,
        CheersImpressive,
        SympathyBringItOn,
        SympathyFeelTheBurn,
        SympathyFuck,
        SympathyItAintOver,
        SympathyNoWay,
        SymphatyOops,
        SympathySorry,
        SympathyYourTurn,
        SympathyYourMove,
        SympathyWhyWHyWHY,
        SpecificComeOn,
        SpecificLegal,
        SpecificPLAleToByloDobre,
        SpecificPLNoChybaTy,
        SpecificPLNieNIeNIE,
        SpecificPLTakaSytuacja,
        SpecificPLTerazTy,
        SpecificPLNoiCoTeraz,
        SpeficicPLChybaWygrywam,
        SpecificPLChybaPrzegrasz,
        SpecificPLMamCie,
        SpecificPLToKoniec,
        SpecificPLOnie,
        SpecificPLOtak,
        SpecificPLZarazZaraz,
        SpecificPLEjTakNieWolno,
        SpecificPLIKontratak,
        SpecificPLZmianaStrategii,
        SpecificTimeIsRunning,
        SpecificTimeIsTicking,
        SpecificWishUndo,
        SpecificLowBattery,
        TipSendMessage,
        TipTapMe,
        TipUpdateNeeded,
        TipWannaTalk,
        NoBaloon,
        CustomMessage,
        Unknown
    }

    /// <summary>
    /// Graficzna reprezentacja trenera podczas gry. Wyœwietla trash talk / pip talk
    /// </summary>
    public class Coach : UIElement
    {
        /// <summary>
        /// Czcionka 
        /// </summary>
        public SpriteFont pipTalkFont;
        /// <summary>
        /// Prostok¹t w którym wykrywane jest tapniêcie trenera
        /// </summary>
        Rectangle touchRect;
        /// <summary>
        /// Margines do <code>touchRect</code>
        /// </summary>
        int marigin;
        /// <summary>
        /// Czy to jest figurka pierwszego gracza - po lewej stronie
        /// </summary>
        bool isFirstPlayer;
        /// <summary>
        /// Kod tego co ma wyœwietlaæ ikona trenera
        /// </summary>
        CoachPipTalkCodes currentPipTalk;

        public CoachPipTalkCodes CurrentPipTalk
        {
            get { return currentPipTalk; }
            set { currentPipTalk = value; }
        }
        /// <summary>
        /// Tekstura z balonikami w których wyœwietla siê trash talk
        /// </summary>
        public Texture2D pipTalkTexture;
        /// <summary>
        /// Wspó³rzêdne baloon trash talk
        /// </summary>
        Vector2 pipTalkBaloonPositionVect;

        public Vector2 PipTalkBaloonPositionVect
        {
            get { return pipTalkBaloonPositionVect; }
            set { pipTalkBaloonPositionVect = value; }
        }
        /// <summary>
        /// Pozycja tekstu
        /// </summary>
        Vector2 pipTalkPositionVect;

        public Vector2 PipTalkPositionVect
        {
            get { return pipTalkPositionVect; }
            set { pipTalkPositionVect = value; }
        }
        /// <summary>
        /// Rozmiar tekstu do balona
        /// </summary>
        Vector2 stringMeasure;
        /// <summary>
        /// Fragment baloon z lewej strony
        /// </summary>
        Rectangle pipTalkBaloonSourceRectL;
        /// <summary>
        /// Fragment baloon z prawej strony
        /// </summary>
        Rectangle pipTalkBaloonSourceRectR;
        /// <summary>
        /// Tekst do baloona
        /// </summary>
        string pipTalkText;
        /// <summary>
        /// Nazwa tekstury z baloonem
        /// </summary>
        string pipTalkTextureName;
        /// <summary>
        /// Teksty, które wyœwietla trener
        /// </summary>
        public Dictionary<CoachPipTalkCodes, String> pipTalkStrings;
        /// <summary>
        /// D³ugoœæ tekstów które wyœwietla trener
        /// </summary>
        public Dictionary<CoachPipTalkCodes, Vector2> pipTalkLenghts;
        /// <summary>
        /// Timeout na wyœwietlanie wiadomoœci przez figurkê trenera
        /// </summary>
        readonly int MESSAGE_TIMEOUT = 5000;
        /// <summary>
        /// Naliczany czas podczas wyœwietlania napisu
        /// </summary>
        int messageTimelife;
        /// <summary>
        /// Margines z ewej strony
        /// </summary>
        readonly int minimumLeftSide = 34;
        /// <summary>
        /// Czas wyœwietlania wiadomoœci o koñcz¹cm siê czasie
        /// </summary>
        int runningWarningTime;
        /// <summary>
        /// Cyfra w odliczanym czasie do koñca rundy.
        /// </summary>
        int runningTimeDigit;

        Queue<String> customMessagesQueue;
        Queue<CoachPipTalkCodes> pipTalkQueue;
        /// <summary>
        /// Kod <code>CoachPipTalkCodes</code> oznaczaj¹cy w jakim ostatnio stanie by³ gracz, czy by³ aktywny i mia³ kod <code>Go</code>, czy czeka³ i mia³ kod <code>NoBaloon</code>.
        /// </summary>
        CoachPipTalkCodes turnIndication;

        /// <summary>
        /// Trener reprezentuj¹cy ruch gracza.
        /// </summary>
        /// <param name="destinationRect">Prostok¹t w który zostaje wrysowany trener</param>
        /// <param name="sourceRect">Prostok¹t definiuj¹cy czêœæ tekstury która zawiera grafikê trenera</param>
        /// <param name="color">Kolor trenera, którym zostanie wype³niony</param>
        /// <param name="isFirstPlayer">Czy to jest trener pierwszego czy drugiego gracza</param>
        public Coach(Rectangle destinationRect, Rectangle sourceRect ,Color color, bool isFirstPlayer)
        {
            this.isFirstPlayer = isFirstPlayer;
            TextureName = "coach";
            pipTalkTextureName = "pipTalk";
            DestinationRectangle = destinationRect;
            this.Color = color;
            SourceRectangle = sourceRect;
            marigin = 30;
            messageTimelife = 0;
            AddPipTalkToDictionary();
            touchRect = new Rectangle(destinationRect.X - marigin, destinationRect.Y - marigin, destinationRect.X + destinationRect.Width + 2 * marigin, destinationRect.Y + destinationRect.Height + marigin);
            if (isFirstPlayer)
            {
                //pipTalkPositionVect = new Vector2(15, 295);
                pipTalkPositionVect = new Vector2(destinationRect.X + 5, destinationRect.Y);
                //pipTalkBaloonPositionVect = new Vector2(7, 295);
                pipTalkBaloonPositionVect = new Vector2(destinationRect.X - 3, destinationRect.Y);
            }
            else
            {
                pipTalkPositionVect = new Vector2(750, 100);
                pipTalkBaloonPositionVect = new Vector2(758, 100); 
            }
            currentPipTalk = CoachPipTalkCodes.CheersGo;
            pipTalkBaloonSourceRectL = new Rectangle(0, 0, minimumLeftSide, 46);
            pipTalkBaloonSourceRectR = new Rectangle(584, 0, 16, 46);
            customMessagesQueue = new Queue<string>();
            pipTalkQueue = new Queue<CoachPipTalkCodes>();
            turnIndication = CoachPipTalkCodes.CheersGo;
            Active = false;
        }

        /// <summary>
        /// Tworzy s³ownik kodów i stringów pipTalk <code>pipTalkString</code>.
        /// </summary>
        private void AddPipTalkToDictionary()
        {
            pipTalkStrings = new Dictionary<CoachPipTalkCodes, string>();
            pipTalkStrings.Add(CoachPipTalkCodes.CheersGetReady, "Get ready!");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersGo, "GO");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersSleepingZzz, "ZzzZZzzZZ...");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersSmile, " :-) ");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersSoooLucky, "Sooo lucky!");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersTakeThat, "Take THAT!");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersTime3_2_1, "3");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersTongueOut, " :-P ");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersDefence, "Defence DEFENCE!");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersEasyPeasy, "Easy Peasy");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersEpicMove, "Epic move!");
            pipTalkStrings.Add(CoachPipTalkCodes.CheersImpressive, "Impressive!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsBringItOn, "Bring it on!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsGoEasyOnMe, "Go easy on me");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsGoodDay, "Good day!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsGreetings, "Greetings!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsHello, "Hello!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsHi, "Hi!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsHiThere, "Hi there!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsHowdy, "Howdy!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsPLSiemka, "Siemka!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsPLHej, "Hej!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsPLWitam, "Witam!");
            pipTalkStrings.Add(CoachPipTalkCodes.GreetingsPLJakLeci, "Jak leci?");
            pipTalkStrings.Add(CoachPipTalkCodes.NoBaloon, "NOBALOON");
            pipTalkStrings.Add(CoachPipTalkCodes.PraiseCannotBe, "Cannot be!");
            pipTalkStrings.Add(CoachPipTalkCodes.PraiseGreatMove, "Great move!");
            pipTalkStrings.Add(CoachPipTalkCodes.PraiseNice, "Nice!");
            pipTalkStrings.Add(CoachPipTalkCodes.PraiseNiceMove, "Nice move!");
            pipTalkStrings.Add(CoachPipTalkCodes.PraiseWelldone, "Well done!");
            pipTalkStrings.Add(CoachPipTalkCodes.PriseWellPlayed, "Well Played!");            
            pipTalkStrings.Add(CoachPipTalkCodes.PriseSpeechless, "I am speechless.");
            pipTalkStrings.Add(CoachPipTalkCodes.PraiseShocked, " :-O ");
            pipTalkStrings.Add(CoachPipTalkCodes.PraiseYouAreOnFire, "You're on fire!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificComeOn, "Come on!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificLegal, "Was that even legal!?");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificLowBattery, "Low on battery?");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificTimeIsRunning, "Time is running!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificTimeIsTicking, "Time is ticking!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLAleToByloDobre, "Ale to bylo dobre!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLTakaSytuacja, "Taka Sytuacja!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLNoChybaTy, "No chyba TY!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLNieNIeNIE, "nie, Nie, NIE!!");            
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLTerazTy, "Teraz Ty!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLNoiCoTeraz, "No i co teraz?");
            pipTalkStrings.Add(CoachPipTalkCodes.SpeficicPLChybaWygrywam, "Chyba to wygram!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLChybaPrzegrasz, "Chyba to przegrasz!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLMamCie, "Mam Cie!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLToKoniec, "To koniec!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLOnie, "o NIE!!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLOtak, "o TAK!!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLEjTakNieWolno, "Ej, tak nie wolno!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLZarazZaraz, "Zaraz, zaraz... co jest?");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLIKontratak, "A teraz konratak!!");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificPLZmianaStrategii, "... zmiana strategii");
            pipTalkStrings.Add(CoachPipTalkCodes.SpecificWishUndo, "I wish I had UNDO button!");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathyBringItOn, "Bring it on!");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathyFeelTheBurn, "Feel the burn!");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathyFuck, ".!#%$@");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathyItAintOver, "It ain't over yet...");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathyNoWay, "No way!");
            pipTalkStrings.Add(CoachPipTalkCodes.SymphatyOops, "Oops");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathySorry, "Sorry");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathyYourTurn, "Your turn!");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathyYourMove, "Your move!");
            pipTalkStrings.Add(CoachPipTalkCodes.SympathyWhyWHyWHY, "why, Why, WHY!!");
            pipTalkStrings.Add(CoachPipTalkCodes.TipSendMessage, "Send message");
            pipTalkStrings.Add(CoachPipTalkCodes.TipTapMe, "Tap me");
            pipTalkStrings.Add(CoachPipTalkCodes.TipWannaTalk, "Wanna talk?");
            pipTalkStrings.Add(CoachPipTalkCodes.TipUpdateNeeded, "Please update Paper Soccer");
            pipTalkStrings.Add(CoachPipTalkCodes.Unknown, "?!");
        }

        /// <summary>
        /// Tworzy s³ownik kodów <code>CoachPiPTalkCodes</code> i <code>Vector2</code> który odzwierciedla d³ugoœæ stringów
        /// </summary>
        private void MeasurePipTalks()
        {
            pipTalkLenghts = new Dictionary<CoachPipTalkCodes, Vector2>();
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersGetReady, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersGetReady]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersGo, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersGo]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersSleepingZzz, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersSleepingZzz]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersSmile, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersSmile]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersSoooLucky, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersSoooLucky]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersTakeThat, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersTakeThat]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersTime3_2_1, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersTime3_2_1]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersTongueOut, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersTongueOut]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersDefence, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersDefence]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersEasyPeasy, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersEasyPeasy]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersEpicMove, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersEpicMove]));
            pipTalkLenghts.Add(CoachPipTalkCodes.CheersImpressive, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.CheersImpressive]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsBringItOn, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsBringItOn]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsGoEasyOnMe, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsGoEasyOnMe]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsGoodDay, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsGoodDay]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsGreetings, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsGreetings]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsHello, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsHello]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsHi, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsHi]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsHiThere, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsHiThere]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsHowdy, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsHowdy]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsPLSiemka, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsPLSiemka]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsPLHej, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsPLHej]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsPLWitam, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsPLWitam]));
            pipTalkLenghts.Add(CoachPipTalkCodes.GreetingsPLJakLeci, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.GreetingsPLJakLeci]));
            pipTalkLenghts.Add(CoachPipTalkCodes.NoBaloon, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.NoBaloon]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PraiseCannotBe, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PraiseCannotBe]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PraiseGreatMove, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PraiseGreatMove]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PraiseNice, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PraiseNice]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PraiseNiceMove, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PraiseNiceMove]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PriseSpeechless, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PriseSpeechless]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PraiseShocked, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PraiseShocked]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PraiseWelldone, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PraiseWelldone]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PriseWellPlayed, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PriseWellPlayed]));
            pipTalkLenghts.Add(CoachPipTalkCodes.PraiseYouAreOnFire, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.PraiseYouAreOnFire]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificComeOn, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificComeOn]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificLegal, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificLegal]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificLowBattery, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificLowBattery]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLAleToByloDobre, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLAleToByloDobre]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificWishUndo, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificWishUndo]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLTakaSytuacja, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLTakaSytuacja]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLNoChybaTy, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLNoChybaTy]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLNieNIeNIE, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLNieNIeNIE]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLTerazTy, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLTerazTy]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLNoiCoTeraz, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLNoiCoTeraz]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpeficicPLChybaWygrywam, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpeficicPLChybaWygrywam]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLChybaPrzegrasz, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLChybaPrzegrasz]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLMamCie, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLMamCie]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLToKoniec, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLToKoniec]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLOnie, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLOnie]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLOtak, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLOtak]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLZarazZaraz, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLZarazZaraz]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLEjTakNieWolno, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLEjTakNieWolno]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLIKontratak, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLIKontratak]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificPLZmianaStrategii, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificPLZmianaStrategii]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificTimeIsRunning, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificTimeIsRunning]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SpecificTimeIsTicking, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SpecificTimeIsTicking]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathyBringItOn, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathyBringItOn]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathyFeelTheBurn, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathyFeelTheBurn]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathyFuck, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathyFuck]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathyItAintOver, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathyItAintOver]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathyNoWay, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathyNoWay]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SymphatyOops, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SymphatyOops]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathySorry, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathySorry]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathyYourMove, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathyYourMove]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathyYourTurn, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathyYourTurn]));
            pipTalkLenghts.Add(CoachPipTalkCodes.SympathyWhyWHyWHY, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.SympathyWhyWHyWHY]));
            pipTalkLenghts.Add(CoachPipTalkCodes.TipSendMessage, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.TipSendMessage]));
            pipTalkLenghts.Add(CoachPipTalkCodes.TipTapMe, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.TipTapMe]));
            pipTalkLenghts.Add(CoachPipTalkCodes.TipWannaTalk, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.TipWannaTalk]));
            pipTalkLenghts.Add(CoachPipTalkCodes.TipUpdateNeeded, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.TipUpdateNeeded]));          
            pipTalkLenghts.Add(CoachPipTalkCodes.Unknown, pipTalkFont.MeasureString(pipTalkStrings[CoachPipTalkCodes.Unknown]));
        }

        /// <summary>
        /// Update'uje dane bior¹c pod uwagê <code>currentPipTalk</code>.
        /// </summary>
        private void UpdateData()
        {
            if (currentPipTalk == CoachPipTalkCodes.CheersGo || currentPipTalk == CoachPipTalkCodes.NoBaloon)
            {
                turnIndication = currentPipTalk;
            }
            if (currentPipTalk != CoachPipTalkCodes.CustomMessage)
            {
                pipTalkText = pipTalkStrings[currentPipTalk];
                stringMeasure = pipTalkLenghts[currentPipTalk];
            }
            else
            {
                if (customMessagesQueue.Count > 0)
                {
                    pipTalkText = customMessagesQueue.Dequeue();
                    stringMeasure = pipTalkFont.MeasureString(pipTalkText);
                }
                else
                {
                    currentPipTalk = turnIndication;
                    UpdateData();
                }
            }
            pipTalkBaloonSourceRectL = new Rectangle(0, 0, minimumLeftSide + (int)stringMeasure.X - 32 , 46);
            if (!isFirstPlayer)
            {
                pipTalkPositionVect = new Vector2(750 - stringMeasure.X + 32, 100);
                pipTalkBaloonPositionVect = new Vector2(758 - stringMeasure.X + 32, 100);
            }
            messageTimelife = 0;
        }

        /// <summary>
        /// W³¹czenie pokazania odliczanego czasu przy grze w multi
        /// </summary>
        /// <param name="warningTime">Czas od którego zaczyna siê odliczanie czasu przez trenera [ms]</param>
        public void ShowWarningCountdown(int warningTime)
        {
            ShowCoachMessage(CoachPipTalkCodes.CheersTime3_2_1);
            runningWarningTime = warningTime;
        }

        /// <summary>
        /// W³¹czenie wyœwietlania konkretnej wiadomoœci przez trenera.
        /// </summary>
        /// <param name="pipTalkCode">Kod wiadomoœci do wyœwietlenia przez trenera</param>
        public void ShowCoachMessage(CoachPipTalkCodes pipTalkCode)
        {
            if (pipTalkCode == CoachPipTalkCodes.CheersGo)
            {
                currentPipTalk = pipTalkCode;
                messageTimelife += MESSAGE_TIMEOUT;
                UpdateData();
            }
            else if (pipTalkCode == CoachPipTalkCodes.NoBaloon)
            {
                turnIndication = CoachPipTalkCodes.NoBaloon;
                if (pipTalkQueue.Count == 0) // jeœli nie ma nic w kolejce
                {
                    if (currentPipTalk == CoachPipTalkCodes.CheersGo || currentPipTalk == CoachPipTalkCodes.CheersTime3_2_1) // a teraz jest wyœwietlane GO
                    {
                        currentPipTalk = pipTalkCode;
                        messageTimelife += MESSAGE_TIMEOUT;
                        UpdateData();
                    }
                }
                else // jeœli jest coœ w kolejce
                {
                    if (currentPipTalk == CoachPipTalkCodes.CheersGo || currentPipTalk == CoachPipTalkCodes.CheersTime3_2_1) // a teraz jest wyœwietlane GO
                    {
                        messageTimelife += MESSAGE_TIMEOUT;
                    }
                }
            }
            else
            {
                pipTalkQueue.Enqueue(pipTalkCode);
                //if (currentPipTalk == turnIndication)
                //{
                    messageTimelife += MESSAGE_TIMEOUT;
                //}
            }
        }

        public void ShowStringMessage(String message)
        {
            customMessagesQueue.Enqueue(message);
            pipTalkQueue.Enqueue(CoachPipTalkCodes.CustomMessage);
            if (currentPipTalk == CoachPipTalkCodes.NoBaloon)
            {
                messageTimelife += MESSAGE_TIMEOUT;                
            }
        }

        /// <summary>
        /// To jest funkacja obs³uguj¹ca wydarzenie -> otrzymanie zdarzenia od przeciwnika z trash talk.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Enkapsulowany string z kodem wiadomoœci.</param>
        public void ShowCoachMessage(object sender, StringEventArs e)
        {
            try
            {
                CoachPipTalkCodes messageCode = (CoachPipTalkCodes)Enum.Parse(typeof(CoachPipTalkCodes), e.InternalString, true);
                ShowCoachMessage(messageCode);
            }
            catch (Exception)
            {
                ShowCoachMessage(CoachPipTalkCodes.TipUpdateNeeded);
#if DEBUG
            Debug.WriteLine(String.Format("Cannot convert string '{0}' into a CoachPipTalkCodes.", e.InternalString));
#endif
            }
        }

        public void ShowCoachMessage(string msgCode)
        {
            try
            {
                CoachPipTalkCodes messageCode = (CoachPipTalkCodes)Enum.Parse(typeof(CoachPipTalkCodes), msgCode, true);
                ShowCoachMessage(messageCode);
            }
            catch (Exception)
            {
                ShowCoachMessage(CoachPipTalkCodes.TipUpdateNeeded);
#if DEBUG
                Debug.WriteLine(String.Format("Cannot convert string '{0}' into a CoachPipTalkCodes.", msgCode));
#endif
            }
        }

        /// <summary>
        /// Wysy³a <code>CoachPipTalkCodes</code> do przeciwnika w trypie gry WiFi, wyœwietla t¹ sam¹ wiadomoœæ lokalnie. Wywo³uje <code>ShowCoachMessage</code>
        /// </summary>
        /// <param name="pipTalkCode">Kod wiadomoœci do wyœwietlenia.</param>
        public void SendCoachMessage(CoachPipTalkCodes pipTalkCode)
        {
            TableGoal.GamePlay.SendTrashTalkMessage(pipTalkCode.ToString());
            GlobalMultiplayerContext.warpClient.SendChat(pipTalkCode.ToString());
            ShowCoachMessage(pipTalkCode);
        }

        public void CleanMessages()
        {
            pipTalkQueue.Clear();
            UpdateData();
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            ObjectTexture = contentManager.Load<Texture2D>(TextureName);
            pipTalkTexture = contentManager.Load<Texture2D>(pipTalkTextureName);
            pipTalkFont = contentManager.Load<SpriteFont>("Fonts/TRIAL_font");
            MeasurePipTalks();
            UpdateData();
        }
        
        public override void Update(GameTime gametime)
        {
            if (currentPipTalk == CoachPipTalkCodes.CheersTime3_2_1)
            {
                runningWarningTime -= gametime.ElapsedGameTime.Milliseconds;
                runningTimeDigit = runningWarningTime / 1000;
                if (runningWarningTime <= 500)
                {
                    runningTimeDigit = 0;
                    messageTimelife += MESSAGE_TIMEOUT;
                    currentPipTalk = CoachPipTalkCodes.NoBaloon;
                    turnIndication = CoachPipTalkCodes.NoBaloon;
                }
            }
            else
            {
                messageTimelife += gametime.ElapsedGameTime.Milliseconds;
            }

            if (messageTimelife >= MESSAGE_TIMEOUT)
            {
                if (pipTalkQueue.Count > 0)
                {
                    messageTimelife = 0;
                    if (currentPipTalk == CoachPipTalkCodes.CheersTime3_2_1)
                    {
                        return;
                    }

                    currentPipTalk = pipTalkQueue.Dequeue();
                    UpdateData();
                }
                else
                {
                    currentPipTalk = turnIndication;
                    UpdateData();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ObjectTexture, DestinationRectangle, SourceRectangle, this.Color);
            if (currentPipTalk != CoachPipTalkCodes.NoBaloon)
            {
                if (isFirstPlayer)
                {
                    spriteBatch.Draw(pipTalkTexture, pipTalkBaloonPositionVect, pipTalkBaloonSourceRectL, Color.Gray);
                    spriteBatch.Draw(pipTalkTexture, new Vector2(pipTalkBaloonPositionVect.X + pipTalkBaloonSourceRectL.Width, pipTalkBaloonPositionVect.Y), pipTalkBaloonSourceRectR, Color.Gray);
                }
                else
                {
                    spriteBatch.Draw(pipTalkTexture,
                                     pipTalkBaloonPositionVect,
                                     pipTalkBaloonSourceRectL,
                                     Color.Gray,
                                     0,
                                     new Vector2(0, 0),
                                     1,
                                     SpriteEffects.FlipHorizontally,
                                     0);

                    spriteBatch.Draw(pipTalkTexture,
                                     new Vector2(pipTalkBaloonPositionVect.X - pipTalkBaloonSourceRectR.Width, pipTalkBaloonPositionVect.Y),
                                     pipTalkBaloonSourceRectR,
                                     Color.Gray,
                                     0,
                                     new Vector2(0, 0),
                                     1,
                                     SpriteEffects.FlipHorizontally,
                                     0);
                }

                if (currentPipTalk == CoachPipTalkCodes.CheersTime3_2_1)
                {
                    spriteBatch.DrawString(pipTalkFont, runningTimeDigit.ToString(), pipTalkPositionVect, Color.Red);
                }
                else
                {
                    spriteBatch.DrawString(pipTalkFont, pipTalkText, pipTalkPositionVect, Color.Gray);
                }
            }
        }

        public override void HandleInput(Vector2 tapPoint)
        {
            if (touchRect.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
            {
                Active = !Active;
            }
        }

        public bool WasPressed(Vector2 tapPoint)
        {
            if (Visible && Active)
            {
                if (touchRect.Contains(new Point((int)tapPoint.X, (int)tapPoint.Y)))
                {
                    AudioManager.PlaySound("selected");
                    return true;
                }
            }
            return false;
        }
    }
}