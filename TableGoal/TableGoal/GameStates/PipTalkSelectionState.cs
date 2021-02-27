using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;
using Microsoft.Phone.Tasks;

namespace TableGoal
{
    /// <summary>
    /// Stan gry gdzie wybiera siê flagi.
    /// </summary>
    public class PipTalkSelectionState : GameState
    {
        Menu menu;
        PipTalkBaloon[] baloons;
        int ySpeed;
        int yDisplacement;
        bool shouldBounceBack;
        Coach _coach;
        public delegate void UpdatePipTalkCallback(int pipTalkIndex, CoachPipTalkCodes newCode);
        UpdatePipTalkCallback _updatePipTalk;
        int _selCode = 0;
        CoachPipTalkCodes _newCode;
        Rectangle screen;
        int mariginBetweenBaloons = 70;
        int upBorder = 50;
        int initialStartingPoint = 25;
        int downBorder = 420;
        SpriteFont _font;
        Rectangle _addMoreHitBox;


        public PipTalkSelectionState(CoachPipTalkCodes[] takenCodes, Coach coach, int selectedPipTalk, UpdatePipTalkCallback updatePipTalk)
        {
            _selCode = selectedPipTalk;
            _coach = coach;
            _updatePipTalk = updatePipTalk;
            this.EnabledGestures = GestureType.FreeDrag | GestureType.Tap | GestureType.Flick;
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            _newCode = CoachPipTalkCodes.Unknown;
            if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "pl")
            {
                baloons = new PipTalkBaloon[64];
                FillOutPL();
                MarkedTaken(takenCodes);
            }
            else
            {
                baloons = new PipTalkBaloon[44];
                FillOutBaloons(0);
                MarkedTaken(takenCodes);
            }
            screen = new Rectangle(0, 0, 800, 480);

            ySpeed = 0;
            shouldBounceBack = false;
            _addMoreHitBox = new Rectangle(0, 400, 200, 80);
        }

        /// <summary>
        /// Oznacza ju¿ wybrane powiedzonka na nieaktywne.
        /// Wa¿ne aby tablica balonów by³a ju¿ wype³niona.
        /// </summary>
        /// <param name="taken"></param>
        private void MarkedTaken(CoachPipTalkCodes[] taken)
        {
            if (baloons == null)
            {
                throw new Exception("Tablica baloon jest równa null!! Nie zosta³a zainicjalizowana");
            }
            if (baloons.Length == 0)
            {
                throw new Exception("Tablica baloon jest pusta!!");
            }
            for (int i = 0; i < baloons.Length - 1; i++)
            {
                for (int j = 0; j < taken.Length; j++)
                {
                    if (baloons[i].PipTalkCode == taken[j])
                    {
                        baloons[i].Active = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Wype³nia tablicê balonów angielskimi powiedzonkami.
        /// </summary>
        /// <param name="startingIndex">Indeks od którego powinno zacz¹æ wype³niaæ. Standardowo 0, chyba, ¿e ju¿ coœ jest w tablicy.</param>
        private void FillOutBaloons(int startingIndex)
        {
            int tmpIndex = startingIndex + 0;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsGoEasyOnMe].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsGoEasyOnMe], CoachPipTalkCodes.GreetingsGoEasyOnMe);
            tmpIndex = startingIndex + 1;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsGoodDay].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsGoodDay], CoachPipTalkCodes.GreetingsGoodDay);
            tmpIndex = startingIndex + 2;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsGreetings].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsGreetings], CoachPipTalkCodes.GreetingsGreetings);
            tmpIndex = startingIndex + 3;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsHello].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsHello], CoachPipTalkCodes.GreetingsHello);
            tmpIndex = startingIndex + 4;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsHi].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsHi], CoachPipTalkCodes.GreetingsHi);
            tmpIndex = startingIndex + 5;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsHiThere].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsHiThere], CoachPipTalkCodes.GreetingsHiThere);
            tmpIndex = startingIndex + 6;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsHowdy].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsHowdy], CoachPipTalkCodes.GreetingsHowdy);
            tmpIndex = startingIndex + 7;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsBringItOn].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsBringItOn], CoachPipTalkCodes.GreetingsBringItOn);


            tmpIndex = startingIndex + 8;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PraiseCannotBe].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PraiseCannotBe], CoachPipTalkCodes.PraiseCannotBe);
            tmpIndex = startingIndex + 9;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PraiseNice].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PraiseNice], CoachPipTalkCodes.PraiseNice);
            tmpIndex = startingIndex + 10;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PraiseGreatMove].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PraiseGreatMove], CoachPipTalkCodes.PraiseGreatMove);
            tmpIndex = startingIndex + 11;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PraiseNiceMove].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PraiseNiceMove], CoachPipTalkCodes.PraiseNiceMove);
            tmpIndex = startingIndex + 12;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PraiseShocked].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PraiseShocked], CoachPipTalkCodes.PraiseShocked);
            tmpIndex = startingIndex + 13;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PraiseWelldone].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PraiseWelldone], CoachPipTalkCodes.PraiseWelldone);
            tmpIndex = startingIndex + 14;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PraiseYouAreOnFire].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PraiseYouAreOnFire], CoachPipTalkCodes.PraiseYouAreOnFire);
            tmpIndex = startingIndex + 15;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PriseSpeechless].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PriseSpeechless], CoachPipTalkCodes.PriseSpeechless);
            tmpIndex = startingIndex + 16;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.PriseWellPlayed].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.PriseWellPlayed], CoachPipTalkCodes.PriseWellPlayed);


            tmpIndex = startingIndex + 17;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersDefence].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersDefence], CoachPipTalkCodes.CheersDefence);
            tmpIndex = startingIndex + 18;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersEasyPeasy].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersEasyPeasy], CoachPipTalkCodes.CheersEasyPeasy);
            tmpIndex = startingIndex + 19;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersEpicMove].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersEpicMove], CoachPipTalkCodes.CheersEpicMove);
            tmpIndex = startingIndex + 20;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersGetReady].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersGetReady], CoachPipTalkCodes.CheersGetReady);
            tmpIndex = startingIndex + 21;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersImpressive].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersImpressive], CoachPipTalkCodes.CheersImpressive);
            tmpIndex = startingIndex + 22;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersSleepingZzz].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersSleepingZzz], CoachPipTalkCodes.CheersSleepingZzz);
            tmpIndex = startingIndex + 23;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersSmile].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersSmile], CoachPipTalkCodes.CheersSmile);
            tmpIndex = startingIndex + 24;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersSoooLucky].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersSoooLucky], CoachPipTalkCodes.CheersSoooLucky);
            tmpIndex = startingIndex + 25;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersTakeThat].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersTakeThat], CoachPipTalkCodes.CheersTakeThat);
            tmpIndex = startingIndex + 26;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.CheersTongueOut].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.CheersTongueOut], CoachPipTalkCodes.CheersTongueOut);


            tmpIndex = startingIndex + 27;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificComeOn].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificComeOn], CoachPipTalkCodes.SpecificComeOn);
            tmpIndex = startingIndex + 28;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificLegal].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificLegal], CoachPipTalkCodes.SpecificLegal);
            tmpIndex = startingIndex + 29;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificLowBattery].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificLowBattery], CoachPipTalkCodes.SpecificLowBattery);
            tmpIndex = startingIndex + 30;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificTimeIsRunning].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificTimeIsRunning], CoachPipTalkCodes.SpecificTimeIsRunning);
            tmpIndex = startingIndex + 31;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificTimeIsTicking].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificTimeIsTicking], CoachPipTalkCodes.SpecificTimeIsTicking);
            tmpIndex = startingIndex + 32;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificWishUndo].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificWishUndo], CoachPipTalkCodes.SpecificWishUndo);


            tmpIndex = startingIndex + 33;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathyBringItOn].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathyBringItOn], CoachPipTalkCodes.SympathyBringItOn);
            tmpIndex = startingIndex + 34;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathyFeelTheBurn].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathyFeelTheBurn], CoachPipTalkCodes.SympathyFeelTheBurn);
            tmpIndex = startingIndex + 35;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathyFuck].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathyFuck], CoachPipTalkCodes.SympathyFuck);
            tmpIndex = startingIndex + 36;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathyItAintOver].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathyItAintOver], CoachPipTalkCodes.SympathyItAintOver);
            tmpIndex = startingIndex + 37;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathyNoWay].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathyNoWay], CoachPipTalkCodes.SympathyNoWay);
            tmpIndex = startingIndex + 38;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathySorry].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathySorry], CoachPipTalkCodes.SympathySorry);
            tmpIndex = startingIndex + 39;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathyWhyWHyWHY].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathyWhyWHyWHY], CoachPipTalkCodes.SympathyWhyWHyWHY);
            tmpIndex = startingIndex + 40;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathyYourMove].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathyYourMove], CoachPipTalkCodes.SympathyYourMove);
            tmpIndex = startingIndex + 41;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SympathyYourTurn].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SympathyYourTurn], CoachPipTalkCodes.SympathyYourTurn);
            tmpIndex = startingIndex + 42;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SymphatyOops].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SymphatyOops], CoachPipTalkCodes.SymphatyOops);
            tmpIndex = startingIndex + 43;
            baloons[tmpIndex] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.TipWannaTalk].X / 2, initialStartingPoint + tmpIndex * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.TipWannaTalk], CoachPipTalkCodes.TipWannaTalk); 
        }

        /// <summary>
        /// Wype³nia tablicê balonów najpierw polskimi odzywkami i póŸniej dope³nia j¹ angielskimi.
        /// </summary>
        void FillOutPL()
        {
            baloons[0] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsPLHej].X / 2, initialStartingPoint + 0 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsPLHej], CoachPipTalkCodes.GreetingsPLHej);
            baloons[1] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsPLSiemka].X / 2, initialStartingPoint + 1 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsPLSiemka], CoachPipTalkCodes.GreetingsPLSiemka);
            baloons[2] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsPLJakLeci].X / 2, initialStartingPoint + 2 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsPLJakLeci], CoachPipTalkCodes.GreetingsPLJakLeci);
            baloons[3] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.GreetingsPLWitam].X / 2, initialStartingPoint + 3 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.GreetingsPLWitam], CoachPipTalkCodes.GreetingsPLWitam);
            baloons[4] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLAleToByloDobre].X / 2, initialStartingPoint + 4 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLAleToByloDobre], CoachPipTalkCodes.SpecificPLAleToByloDobre);
            baloons[5] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpeficicPLChybaWygrywam].X / 2, initialStartingPoint + 5 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpeficicPLChybaWygrywam], CoachPipTalkCodes.SpeficicPLChybaWygrywam);
            baloons[6] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLChybaPrzegrasz].X / 2, initialStartingPoint + 6 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLChybaPrzegrasz], CoachPipTalkCodes.SpecificPLChybaPrzegrasz);
            baloons[7] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLEjTakNieWolno].X / 2, initialStartingPoint + 7 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLEjTakNieWolno], CoachPipTalkCodes.SpecificPLEjTakNieWolno);
            baloons[8] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLIKontratak].X / 2, initialStartingPoint + 8 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLIKontratak], CoachPipTalkCodes.SpecificPLIKontratak);
            baloons[9] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLMamCie].X / 2, initialStartingPoint + 9 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLMamCie], CoachPipTalkCodes.SpecificPLMamCie);
            baloons[10] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLNieNIeNIE].X / 2, initialStartingPoint + 10 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLNieNIeNIE], CoachPipTalkCodes.SpecificPLNieNIeNIE);
            baloons[11] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLNoChybaTy].X / 2, initialStartingPoint + 11 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLNoChybaTy], CoachPipTalkCodes.SpecificPLNoChybaTy);
            baloons[12] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLNoiCoTeraz].X / 2, initialStartingPoint + 12 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLNoiCoTeraz], CoachPipTalkCodes.SpecificPLNoiCoTeraz);
            baloons[13] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLOnie].X / 2, initialStartingPoint + 13 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLOnie], CoachPipTalkCodes.SpecificPLOnie);
            baloons[14] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLOtak].X / 2, initialStartingPoint + 14 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLOtak], CoachPipTalkCodes.SpecificPLOtak);
            baloons[15] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLTakaSytuacja].X / 2, initialStartingPoint + 15 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLTakaSytuacja], CoachPipTalkCodes.SpecificPLTakaSytuacja);
            baloons[16] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLTerazTy].X / 2, initialStartingPoint + 16 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLTerazTy], CoachPipTalkCodes.SpecificPLTerazTy);
            baloons[17] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLToKoniec].X / 2, initialStartingPoint + 17 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLToKoniec], CoachPipTalkCodes.SpecificPLToKoniec);
            baloons[18] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLZarazZaraz].X / 2, initialStartingPoint + 18 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLZarazZaraz], CoachPipTalkCodes.SpecificPLZarazZaraz);
            baloons[19] = new PipTalkBaloon(new Vector2(400 - _coach.pipTalkLenghts[CoachPipTalkCodes.SpecificPLZmianaStrategii].X / 2, initialStartingPoint + 19 * mariginBetweenBaloons), Color.Black, _coach.pipTalkStrings[CoachPipTalkCodes.SpecificPLZmianaStrategii], CoachPipTalkCodes.SpecificPLZmianaStrategii);
            FillOutBaloons(20);
        }

        public override void LoadContent()
        {
            menu.LoadTexture(GameManager.Game.Content);
            for (int i = 0; i < baloons.Length; i ++)
            {
                baloons[i].LoadTexture(GameManager.Game.Content);
            }
            _font = GameManager.Game.Content.Load<SpriteFont>("Fonts/TRIAL_font");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
            }
            if (_newCode != CoachPipTalkCodes.Unknown)
            {
                _updatePipTalk(_selCode, _newCode);
                AudioManager.PlaySound("selected");
                GameManager.RemoveState(this);
                return;
            }

            for (int index = 0; index < baloons.Length; index++)
            {
                baloons[index].AdjustPosition(0, yDisplacement + ySpeed);
                if (baloons[index].R_hitbox.Intersects(screen))
                {
                    baloons[index].Visible = true;
                }
                else
                {
                    baloons[index].Visible = false;
                }
            }
            yDisplacement = 0;
            
            /*
             * Jeœli po³o¿enie pierwszego balonika jest wiêksze od 50 pixeli.
             */
            if (baloons[0].R_hitbox.Top > upBorder)
            {
                shouldBounceBack = true;
                ySpeed = (int)(ySpeed * -0.2);
                if (ySpeed >= 0)
                    ySpeed = -28;
            }
                /*
                 * Jeœli po³o¿enie ostatniego balonika - dowlna krawêdŸ - jest mniejsze od 400).
                 */
            else if (baloons[baloons.Length - 1].R_hitbox.Bottom < downBorder)
            {
                shouldBounceBack = true;
                ySpeed = (int)(ySpeed * -0.3);
                if (ySpeed <= 0)
                    ySpeed = 28;
            }
            /*
             * Jeœli ¿adne z poni¿szych to dzia³amy parametrem "tarcia" na prêdkoœæ.
             */
            else
            {
                if (shouldBounceBack)
                {
                    if (Math.Abs(ySpeed) > 23)
                    {
                        ySpeed = (int)(ySpeed * 0.4);
                    }
                    else if (Math.Abs(ySpeed) > 15)
                    {
                        ySpeed = (int)(ySpeed * 0.4);
                    }
                    else if (Math.Abs(ySpeed) > 6)
                    {
                        ySpeed = (int)(ySpeed * 0.4);
                    }
                }
                ySpeed = (int)(ySpeed * 0.96);
                if (Math.Abs(ySpeed) < 2)
                {
                    ySpeed = 0;
                    shouldBounceBack = false;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            for (int j = 0; j < baloons.Length; j++)
            {
                baloons[j].Draw(spriteBatch);
            }
            spriteBatch.DrawString(_font, "+ suggest one", new Vector2(5, 430), Color.Black);
            base.Draw(gameTime);
            spriteBatch.End();
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            //yDisplacement = 0;
            if (input.Gestures.Count > 0)
            {
                shouldBounceBack = false;
                ySpeed = 0;
                if (input.Gestures[0].GestureType == GestureType.FreeDrag)
                {
                    yDisplacement = (int)input.Gestures[0].Delta.Y;
                    yDisplacement = (int)(2.5 * yDisplacement);
                }
                if (input.Gestures[0].GestureType == GestureType.Flick)
                {
                    /*
                     * Bierzemy delte x z Flickniecia i dzielimy przez 20 - to daje nam predkosc
                     */
                    ySpeed = (int)input.Gestures[0].Delta.Y / 20;
                    if (baloons[0].R_hitbox.Top >= upBorder)
                    {
                        if (ySpeed > 0)
                            ySpeed = -1;
                    }
                    if (baloons[baloons.Length -1].R_hitbox.Bottom <= downBorder)
                    {
                        if (ySpeed < 0)
                            ySpeed = 1;
                    }
                }
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    if (_addMoreHitBox.Contains((int)input.Gestures[0].Position.X, (int)input.Gestures[0].Position.Y))                    
                    {
                        AudioManager.PlaySound("selected");
                        EmailComposeTask email = new EmailComposeTask();
                        if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "pl")
                        {
                            email.Subject = "Paper Soccer - sugestia wiadomoœci.";
                            email.To = "madbrainstudio@hotmail.com";
                            email.Body = "Twoja propozycja nowej wiadomoœci to:\n- PROSZÊ WPISZ TUTAJ";
                        }
                        else
                        {
                            email.Subject = "Paper Soccer - trash talk suggestion.";
                            email.To = "madbrainstudio@hotmail.com";
                            email.Body = "New trash talk you suggest to add is:\n- ENTER HERE PLEASE";
                        }
                        try
                        {
                            email.Show();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    /*
                     * Jeœli tapniemy i jest jakas prêdkoœæ to zatrzymujemy.
                     */
                    if (ySpeed != 0)
                    {
                        ySpeed = 0;
                        return;
                    }
                    for (int i = 0; i < baloons.Length; i++)
                    {
                        if (baloons[i].WasPressed(input.Gestures[0].Position))
                        {
                            _newCode = baloons[i].PipTalkCode;
                        }
                    }
                }
            }
            base.HandleInput(gameTime, input);
        }
    }
}
