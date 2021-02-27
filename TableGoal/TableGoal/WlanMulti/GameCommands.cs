using System;
using System.Net;
using System.Windows;

namespace TableGoal
{
    public class GameCommands
    {
        /// <summary>
        /// This class holds all the commands a that can be passed between clients of the multicast. 
        /// The goal is to demonstrate the creation of a small protocol for the purpose of communicating ]
        /// and understanding each message. 
        /// </summary>
        public const string CommandDelimeter = "|";
        #region Game/player related
        public const string Join = "J";
        public const string Leave = "L";
        public const string Challenge = "C";
        public const string AcceptChallenge = "AC";
        public const string RejectChallenge = "RC";
        public const string Play = "P";
        public const string Ready = "G";
        public const string NewGame = "N";
        public const string LeaveGame = "LG";
        public const string LostTurn = "LM";
        public const string TrashTalkMessage = "IM";
        #endregion
        #region game properties related
        public const string InfoRequest = "IR";
        public const string InfoDetails = "ID";
        public const string ColorRequest = "CR";
        public const string ColorDetails = "CD";
        #endregion

        public const string JoinFormat = Join + CommandDelimeter + "{0}";
        public const string LeaveFormat = Leave + CommandDelimeter + "{0}";
        public const string LeaveGameFormat = LeaveGame + CommandDelimeter + "{0}";
        public const string ChallengeFormat = Challenge + CommandDelimeter + "{0}";
        public const string AcceptChallengeFormat = AcceptChallenge + CommandDelimeter + "{0}";
        public const string NewGameFormat = NewGame + CommandDelimeter + "{0}";
        public const string RejectChallengeFormat = RejectChallenge + CommandDelimeter + "{0}";
        public const string PlayFormat = Play + CommandDelimeter + "{0}" + CommandDelimeter + "{1}";
        public const string ExtendedPlayFormat = Play + CommandDelimeter + "{0}" + CommandDelimeter + "{1}" + CommandDelimeter + "{2}";
        public const string ReadyFormat = Ready + CommandDelimeter + "{0}";
        public const string LostTurnFormat = LostTurn + CommandDelimeter + "{0}";
        public const string TrashTalkMessageFormat = TrashTalkMessage + CommandDelimeter + "{0}";
        public const string InfoRequestFormat = InfoRequest + CommandDelimeter + "{0}";
        public const string InfoDetailsFormat = InfoDetails + CommandDelimeter + "{0}" + CommandDelimeter + "{1}" + CommandDelimeter + "{2}" + CommandDelimeter + "{3}" + CommandDelimeter + "{4}";
        public const string ColorRequestFormat = ColorRequest + CommandDelimeter + "{0}";
        public const string ColorDetailsFormat = ColorDetails + CommandDelimeter + "{0}" + CommandDelimeter + "{1}";
    }
}
