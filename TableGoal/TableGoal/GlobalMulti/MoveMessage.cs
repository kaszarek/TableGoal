using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework;

namespace TableGoal
{
    /// <summary>
    /// Paper Soccer gameplay message class. Objects of this class represent actions of the user
    /// and are used to serialize/deserialize JSON exchanged between users in the room
    /// </summary>
    public class MoveMessage
    {
        public String sender;
        public String move;
        public String type;
        public Color color;

        public static MoveMessage buildMessage(byte[] update)
        {
            JObject jsonObj = JObject.Parse(System.Text.Encoding.UTF8.GetString(update, 0, update.Length));
            MoveMessage msg = new MoveMessage();
            msg.sender = jsonObj["sender"].ToString();
            msg.type = jsonObj["type"].ToString();
            if (msg.type == "new")
            {
                string color = jsonObj["color"].ToString();
                if (color != "")
                {
                    msg.color = ExportColor(color);
                }
                else
                {
                    msg.color = Color.Black;
                }
            }
            if (msg.type == "move")
            {
                msg.move = jsonObj["piece"].ToString();
            }
            return msg;
        }

        public static byte[] buildMessageBytes(String move)
        {
            JObject moveObj = new JObject();
            moveObj.Add("sender", GlobalMultiplayerContext.UniqueLocalPlayerName);
            moveObj.Add("piece", move);
            moveObj.Add("type", "move");
            return System.Text.Encoding.UTF8.GetBytes(moveObj.ToString());
        }

        public static byte[] buildLostTurnMessageBytes()
        {
            JObject moveObj = new JObject();
            moveObj.Add("sender", GlobalMultiplayerContext.UniqueLocalPlayerName);
            moveObj.Add("type", "lostTurn");
            return System.Text.Encoding.UTF8.GetBytes(moveObj.ToString());
        }

        public static byte[] buildNewGameMessageBytes(string color)
        {
            JObject moveObj = new JObject();
            moveObj.Add("sender", GlobalMultiplayerContext.UniqueLocalPlayerName);
            moveObj.Add("type", "new");
            moveObj.Add("color", color);
            return System.Text.Encoding.UTF8.GetBytes(moveObj.ToString());
        }

        public static byte[] buildChallengeRejectedMessageBytes()
        {
            JObject moveObj = new JObject();
            moveObj.Add("sender", GlobalMultiplayerContext.UniqueLocalPlayerName);
            moveObj.Add("type", "challengeRejected");
            return System.Text.Encoding.UTF8.GetBytes(moveObj.ToString());
        }

        public static byte[] buildChallengeAcceptedMessageBytes()
        {
            JObject moveObj = new JObject();
            moveObj.Add("sender", GlobalMultiplayerContext.UniqueLocalPlayerName);
            moveObj.Add("type", "challengeAccepted");
            return System.Text.Encoding.UTF8.GetBytes(moveObj.ToString());
        }

        /// <summary>
        /// Exports <code>Color</code> from string (RGB format).
        /// </summary>
        /// <param name="encapsulatedColor">Color in RGB format.</param>
        /// <returns><code>Color</code> extracted from the string.</returns>
        private static Color ExportColor(string encapsulatedColor)
        {
            string sep = ":RGBA";
            string[] RGB = encapsulatedColor.Split(sep.ToCharArray());
            int r = int.Parse(RGB[2]);
            int g = int.Parse(RGB[4]);
            int b = int.Parse(RGB[6]);
            return new Color(r, g, b);
        }

    }
}
