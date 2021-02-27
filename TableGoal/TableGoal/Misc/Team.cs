using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using System.IO;

namespace TableGoal
{
    public enum Controlling
    {
        GESTURES,
        BUTTONS,
        NOTSET
    }

    public enum TeamCoach
    {
        HUMAN,
        CPU,
        REMOTEOPPONENT
    }

    public class Team
    {
        public Color ShirtsColor { get; set; }
        public int Goals { get; set; }
        public bool HaveMoveNow { get; set; }
        public Controlling Controler { get; set; }
        public TeamCoach Coach { get; set; }
        /// <summary>
        /// Domyœlny konstruktor. Ustala kolor na <code>Color.Gold</code>, gole na 0, kontroler na <code>NOSET</code> i trenera na <code>HUMAN</code>.
        /// </summary>
        public Team ()
        {
            ShirtsColor = Color.Gold;
            Goals = 0;
            HaveMoveNow = false;
            Controler = Controlling.NOTSET;
            Coach = TeamCoach.HUMAN;
        }

        public Team(Color shirtColor)
        {
            ShirtsColor = shirtColor;
        }
        /// <summary>
        /// Tworzy kopiê zespo³u na podtawie podanego.
        /// </summary>
        /// <param name="team">Zespó³, który ma zostaæ sklonowany.</param>
        public Team(Team team)
        {
            ShirtsColor = team.ShirtsColor;
            Goals = team.Goals;
            HaveMoveNow = team.HaveMoveNow;
            Controler = team.Controler;
            Coach = team.Coach;
        }

        public void Scored()
        {
            Goals++;
        }
    }
}
