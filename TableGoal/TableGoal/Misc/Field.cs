using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TableGoal
{
    public enum PlayField
    {
        classic,
        large
    }

    public class Field
    {
        public string pathToBackground { get; set; }
        public int fieldWallSize { get; set; }
        public Rectangle[] goals { get; set; }
        public Vector2[] winningPoints { get; set; }
        public Vector2[] borders { get; set; }
        public Vector2[] cornerPoints { get; set; }
        public Vector2[] stillSafePoints { get; set; }
        public Vector2[] partiallySafePoints { get; set; }

        public Field()
        {
            fieldWallSize = 1;
            goals = new Rectangle[2];
            winningPoints = new Vector2[6];
            borders = new Vector2[4];
            cornerPoints = new Vector2[4];
            stillSafePoints = new Vector2[2];
            partiallySafePoints = new Vector2[4];
        }

        public static string GetPathToBackground()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    return field10x8.pathToBackground;
                case PlayField.large:
                    return field22x14.pathToBackground;
            }
            return String.Empty; 
        }

        public int GetWallSize()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    return field10x8.fieldWallSize;
                case PlayField.large:
                    return field22x14.fieldWallSize;
            }
            return 1;
        }

        public Rectangle[] GetGoals()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    return field10x8.goals;
                case PlayField.large:
                    return field22x14.goals;
            }
            return null;
        }

        public Vector2[] GetWinningPoints()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    return field10x8.winningPoints;
                case PlayField.large:
                    return field22x14.winningPoints;
            }
            return null;
        }

        public Vector2[] GetBorders()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    return field10x8.borders;
                case PlayField.large:
                    return field22x14.borders;
            }
            return null;
        }

        public Vector2[] GetCornerPoints()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    return field10x8.cornerPoints;
                case PlayField.large:
                    return field22x14.cornerPoints;
            }
            return null;
        }

        public Vector2[] GetStillSafePoints()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    return field10x8.stillSafePoints;
                case PlayField.large:
                    return field22x14.stillSafePoints;
            }
            return null;
        }

        public Vector2[] GetPartiallySafePoints()
        {
            switch (GameVariables.Instance.TypeOfField)
            {
                case PlayField.classic:
                    return field10x8.partiallySafePoints;
                case PlayField.large:
                    return field22x14.partiallySafePoints;
            }
            return null;
        }

        public static void CreateFields()
        {
            field10x8.pathToBackground = "Backgrounds/field10x8";
            field10x8.goals[0] = new Rectangle(67, 188, 55, 105);
            field10x8.goals[1] = new Rectangle(679, 188, 55, 105);
            field10x8.winningPoints[0] = new Vector2(70, 185); // bramka po lewej = 1st player GÓRNY -> CEL AI
            field10x8.winningPoints[1] = new Vector2(70, 240); // bramka po lewej = 1st player ŒRODEK -> CEL AI
            field10x8.winningPoints[2] = new Vector2(70, 295); // bramka po lewej = 1st player DOLNY -> CEL AI
            field10x8.winningPoints[3] = new Vector2(730, 185); // bramka po prawej = 2nd player GÓRNY
            field10x8.winningPoints[4] = new Vector2(730, 240); // bramka po prawej = 2nd player ŒRODEK
            field10x8.winningPoints[5] = new Vector2(730, 295); // bramka po prawej = 2nd player DOLNY
            field10x8.borders[0] = new Vector2(0, 20); // górna granica
            field10x8.borders[1] = new Vector2(0, 460); // dolna granica
            field10x8.borders[2] = new Vector2(125, 0); // lewa granica
            field10x8.borders[3] = new Vector2(675, 0); // prawa granica
            field10x8.cornerPoints[0] = new Vector2(125, 20); // lewy górny naro¿nik boiska
            field10x8.cornerPoints[1] = new Vector2(125, 460); // lewy dolny naro¿nik boiska
            field10x8.cornerPoints[2] = new Vector2(675, 20); // prawy górny naro¿nik boiska
            field10x8.cornerPoints[3] = new Vector2(675, 460); // prawy dolny naro¿nik boiska
            field10x8.stillSafePoints[0] = new Vector2(125, 240); // œrodek bramki po lewej stronie = 1st player
            field10x8.stillSafePoints[1] = new Vector2(675, 240); // œrodek bramki po prawej stronie = 2nd player
            field10x8.partiallySafePoints[0] = new Vector2(125, 185); // górny naro¿nik bramki po lewej = 1st player -> CEL AI
            field10x8.partiallySafePoints[1] = new Vector2(125, 295); // dolny naro¿nik bramki po lewej = 1st player -> CEL AI
            field10x8.partiallySafePoints[2] = new Vector2(675, 185); // górny naro¿nik bramki po prawej = 2nd player
            field10x8.partiallySafePoints[3] = new Vector2(675, 295); // dolny naro¿nik bramki po prawej = 2nd player


            field22x14.pathToBackground = "Backgrounds/field22x14";
            field22x14.goals[0] = new Rectangle(18, 210, 32, 61);
            field22x14.goals[1] = new Rectangle(751, 210, 32, 61);
            field22x14.winningPoints[0] = new Vector2(16, 208);
            field22x14.winningPoints[1] = new Vector2(16, 240);
            field22x14.winningPoints[2] = new Vector2(16, 272);
            field22x14.winningPoints[3] = new Vector2(784, 208);
            field22x14.winningPoints[4] = new Vector2(784, 240);
            field22x14.winningPoints[5] = new Vector2(784, 272);
            field22x14.borders[0] = new Vector2(0, 16);
            field22x14.borders[1] = new Vector2(0, 464);
            field22x14.borders[2] = new Vector2(48, 0);
            field22x14.borders[3] = new Vector2(752, 0);
            field22x14.cornerPoints[0] = new Vector2(48, 16);
            field22x14.cornerPoints[1] = new Vector2(48, 464);
            field22x14.cornerPoints[2] = new Vector2(752, 16);
            field22x14.cornerPoints[3] = new Vector2(752, 464);
            field22x14.stillSafePoints[0] = new Vector2(48, 240);
            field22x14.stillSafePoints[1] = new Vector2(752, 240);
            field22x14.partiallySafePoints[0] = new Vector2(48, 208);
            field22x14.partiallySafePoints[1] = new Vector2(48, 272);
            field22x14.partiallySafePoints[2] = new Vector2(752, 208);
            field22x14.partiallySafePoints[3] = new Vector2(752, 272);
        }

        static public Field field22x14 = new Field
        {
            fieldWallSize = 32,
        };

        static public Field field10x8 = new Field
        {
            fieldWallSize = 55,
        };
        
    }
}
