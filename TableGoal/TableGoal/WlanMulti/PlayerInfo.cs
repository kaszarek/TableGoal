using System;
using System.Net;
using System.Windows;
using Microsoft.Xna.Framework;

namespace TableGoal
{
    /// <summary>
    /// The information about each player.
    /// </summary>
    public class PlayerInfo
    {
        public PlayerInfo(string playerName, IPEndPoint endPoint)
        {
            PlayerEndPoint = endPoint;
            PlayerName = playerName;
        }

        public PlayerInfo(string playerName, IPEndPoint endPoint, Color color)
        {
            PlayerEndPoint = endPoint;
            PlayerName = playerName;
            PlayerColor = color;
        }

        public PlayerInfo(string playerName, Color color)
        {
            PlayerEndPoint = null;
            PlayerName = playerName;
            PlayerColor = color;
        }

        public PlayerInfo(string playerName)
        {
            PlayerEndPoint = null;
            PlayerName = playerName;
        }

        public Color PlayerColor { get; set; }
        public IPEndPoint PlayerEndPoint { get; set; }
        public string PlayerName { get; set; }
        public PlayField Field { get; set; }
        public bool IsGoalLimited { get; set; }
        public int GameLimit { get; set; }
    }
}
