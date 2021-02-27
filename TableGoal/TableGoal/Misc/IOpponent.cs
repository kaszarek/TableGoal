using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableGoal
{
    /*
     * 
     *  TO DO !!!
     *  For better optimization and to make code more cleaner.
     * 
     */

    public interface IOpponent
    {
        void MoveMade(string move);

        void MakeMove();

        bool isThinking();

        void CancelMove();

        void CheckDifficultyLevel();
        
        void NotifyAboutMoves();

        void UnregisterEvents();

    }
}
