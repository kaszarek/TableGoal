using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableGoal
{
    static class FIFAboard
    {
        static Random rand;
        /// <summary>
        /// Losuje 31 krajów wystepuj¹cych w mistrzostwach œwiata.
        /// </summary>
        /// <returns></returns>
        public static List<Country> DrawCountriesToWC()
        {
            rand = new Random(DateTime.Now.Millisecond);
            List<Country> happy31Counties = new List<Country>();
            int luck = 0;
            int luckLimit = 80;
            do
            {
                foreach (Country c in Countries.pathToFlags.Keys)
                {
                    if (c == WorldCupProgress.Instance.SelectedCountry)
                        continue;
                    if (happy31Counties.Contains(c))
                    {
                        if (luckLimit > 60)
                            luckLimit -= 10;
                        continue;
                    }
                    luck = rand.Next(100);
                    if (luck > luckLimit)
                        happy31Counties.Add(c);
                    if (happy31Counties.Count == 31)
                        break;
                }
                if (luckLimit > 60)
                    luckLimit -= 5;
            }
            while (happy31Counties.Count < 31);

            return happy31Counties;
        }
    }
}
