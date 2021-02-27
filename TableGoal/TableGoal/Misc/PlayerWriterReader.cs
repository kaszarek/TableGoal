using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;

namespace TableGoal
{
    static class PlayerWriterReader
    {
        private readonly static string playerData = "PaperSoccerPlayerInfo.dat";
        private static Random _playerNumberDrawer;
        
        internal class PlayerInfo
        {
            public string Name { get; set; }
            public string IdRandomPrefix { get; set; }
        }

        public static PlayerInfo plInfo = new PlayerInfo()
        {
            Name = "Player"
        };

        public static void SaveToIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = isolatedStorageFile.CreateFile(playerData))
                {
                    using (StreamWriter textWriter = new StreamWriter(fileStream))
                    {
                        textWriter.WriteLine(plInfo.Name);
                        textWriter.WriteLine(plInfo.IdRandomPrefix);
                    }
                }
            }
        }

        public static void LoadFromIsolatedStorage()
        {
            bool fileExist = false;
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(playerData))
                {
                    fileExist = true;
                    using (IsolatedStorageFileStream fileStream = isolatedStorageFile.OpenFile(playerData, FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            plInfo.Name = streamReader.ReadLine();
                            plInfo.IdRandomPrefix = streamReader.ReadLine();
                        }
                    }
                }
            }
            if (!fileExist)
            {
                if (plInfo.Name.Length <= 6)
                {
                    _playerNumberDrawer = new Random(DateTime.Now.Millisecond);
                    int playerNumber = _playerNumberDrawer.Next(10000, 99999);
                    plInfo.Name += playerNumber.ToString();
                }
            }
            if (String.IsNullOrEmpty(plInfo.IdRandomPrefix))
            {
                _playerNumberDrawer = new Random(DateTime.Now.Millisecond);
                int randomId = _playerNumberDrawer.Next(10000, 99999);
                plInfo.IdRandomPrefix = DateTime.Now.DayOfYear.ToString() + randomId.ToString();
                SaveToIsolatedStorage();
            }
        }

        public static void CleanIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(playerData))
                    isolatedStorageFile.DeleteFile(playerData);
            }
        }
    }
}
