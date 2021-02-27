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
    public class CountryTeam
    {
        static Random LuckDrawer;
        Country country;

        public Country Country
        {
            get { return country; }
            set { country = value; }
        }
        int offensiveStats;
        /// <summary>
        /// Statystyki offensywne (0, 100)
        /// </summary>
        public int OffensiveStats
        {
            get { return offensiveStats; }
            set { offensiveStats = value; }
        }
        int defensiveStats;
        /// <summary>
        /// Statystyki defensywne (0, 100)
        /// </summary>
        public int DefensiveStats
        {
            get { return defensiveStats; }
            set { defensiveStats = value; }
        }
        int matchesPlayed;
        /// <summary>
        /// Iloœæ granych meczy
        /// </summary>
        public int MatchesPlayed
        {
            get { return matchesPlayed; }
            set { matchesPlayed = value; }
        }
        int wonMatches;
        /// <summary>
        /// Iloœæ wygranych meczy
        /// </summary>
        public int WonMatches
        {
            get { return wonMatches; }
            set { wonMatches = value; }
        }
        int lostMatches;
        /// <summary>
        /// Iloœæ przegranych meczy
        /// </summary>
        public int LostMatches
        {
            get { return lostMatches; }
            set { lostMatches = value; }
        }
        int drawMatches;
        /// <summary>
        /// Iloœæ zremisowanych meczy
        /// </summary>
        public int DrawMatches
        {
            get { return drawMatches; }
            set { drawMatches = value; }
        }
        int collectedPoints;
        /// <summary>
        /// Zwraca iloœæ punktów.
        /// </summary>
        public int CollectedPoints
        {
            get { return collectedPoints; }
            set { collectedPoints = value; }
        }
        /// <summary>
        /// Wygrana meczu
        /// </summary>
        public void WonTheMatch()
        {
            matchesPlayed++;
            wonMatches++;
            collectedPoints += 3;
        }
        /// <summary>
        /// Mecz przegrany
        /// </summary>
        public void LostTheMatch()
        {
            matchesPlayed++;
            lostMatches++;
        }
        /// <summary>
        /// Rremis
        /// </summary>
        public void DrawTheMatch()
        {
            matchesPlayed++;
            drawMatches++;
            collectedPoints += 1;
        }
        /// <summary>
        /// Oblicza iloœæ zdobytych punktów
        /// </summary>
        public void CalculatePoints()
        {
            collectedPoints = 3 * wonMatches + drawMatches;
        }

        public CountryTeam()
        {
            CalculatePoints();
        }

        /// <summary>
        /// Tworzy zespó³ o okreœlonych statystykach.
        /// </summary>
        /// <param name="c">Kraj</param>
        /// <param name="offStats">Statystyki ataku z zakresu od 0 do 100</param>
        /// <param name="defStats">Statystyki obrony z zakresu od 0 do 100</param>
        public CountryTeam(Country c, int offStats, int defStats)
        {
            LuckDrawer = new Random(DateTime.Now.Millisecond);
            this.country = c;
            if (offStats > 100)
                offStats = 100;
            if (offStats < 0)
                offStats = 0;
            if (defStats > 100)
                defStats = 100;
            if (defStats < 0)
                defStats = 0;
            this.offensiveStats = offStats;
            this.defensiveStats = defStats;
            if (offensiveStats + defensiveStats < 150)
                offensiveStats += LuckDrawer.Next(25);
            offensiveStats += LuckDrawer.Next(10);
            defensiveStats += LuckDrawer.Next(10);
        }
        /// <summary>
        /// Tworzy zespó³. Statystyki pobierane s¹ automatycznie
        /// </summary>
        /// <param name="c">Kraj</param>
        public CountryTeam(Country c)
        {
            if (c == global::TableGoal.Country.UNKNOWN)
                return;
            int offStats = Countries.offensiveStats[c];
            int defStats = Countries.defensiveStats[c];
            LuckDrawer = new Random(DateTime.Now.Millisecond);
            this.country = c;
            if (offStats > 100)
                offStats = 100;
            if (offStats < 0)
                offStats = 0;
            if (defStats > 100)
                defStats = 100;
            if (defStats < 0)
                defStats = 0;
            this.offensiveStats = offStats;
            this.defensiveStats = defStats;
            if (offensiveStats + defensiveStats < 150)
                offensiveStats += LuckDrawer.Next(25);
            offensiveStats += LuckDrawer.Next(10);
            defensiveStats += LuckDrawer.Next(10);
        }

        /// <summary>
        /// Bazuj¹c na statystykach zespo³ów symuluje mecz pomiêdzy nimi.
        /// </summary>
        /// <param name="opponent">Przecinik</param>
        public void PlayMatch(CountryTeam opponent)
        {
            int result = (offensiveStats + defensiveStats - opponent.offensiveStats - opponent.defensiveStats) / 2;
            if (result >= -1 && result <= 1)
            {
                DrawTheMatch();
                opponent.DrawTheMatch();
            }
            if (result > 1)
            {
                WonTheMatch();
                opponent.LostTheMatch();
            }
            if (result < -1)
            {
                LostTheMatch();
                opponent.WonTheMatch();
            }
        }

        public override string ToString()
        {
            return String.Format("{0} M={1} W={2} L={3} D={4} Pts={5}",
                                 this.country.ToString(),
                                 this.matchesPlayed,
                                 this.wonMatches,
                                 this.lostMatches,
                                 this.drawMatches,
                                 this.collectedPoints);
        }
    }
}
