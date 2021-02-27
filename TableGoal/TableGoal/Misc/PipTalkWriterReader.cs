using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Diagnostics;

namespace TableGoal
{
    static class PipTalkWriterReader
    {
        private readonly static string pipTalkData = "PaperSoccerPipTalkInfo.dat";
        
        internal class PipTalkInfo
        {
            public CoachPipTalkCodes First { get; set; }
            public CoachPipTalkCodes Second { get; set; }
            public CoachPipTalkCodes Third { get; set; }
            public CoachPipTalkCodes Fourth { get; set; }
        }

        public static PipTalkInfo ptInfo = new PipTalkInfo()
        {
            Fourth = CoachPipTalkCodes.GreetingsGreetings,
            Third = CoachPipTalkCodes.GreetingsHiThere,
            Second = CoachPipTalkCodes.GreetingsPLSiemka,
            First = CoachPipTalkCodes.GreetingsGoodDay
        };

        public static void SaveToIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = isolatedStorageFile.CreateFile(pipTalkData))
                {
                    using (StreamWriter textWriter = new StreamWriter(fileStream))
                    {
                        textWriter.WriteLine(ptInfo.First);
                        textWriter.WriteLine(ptInfo.Second);
                        textWriter.WriteLine(ptInfo.Third);
                        textWriter.WriteLine(ptInfo.Fourth);
                    }
                }
            }
        }

        public static void LoadFromIsolatedStorage()
        {
            bool fileExist = false;
            string codeInString = string.Empty;
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(pipTalkData))
                {
                    fileExist = true;
                    using (IsolatedStorageFileStream fileStream = isolatedStorageFile.OpenFile(pipTalkData, FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            // odczytanie pierwszej liniki
                            codeInString = streamReader.ReadLine();
                            try
                            {
                                ptInfo.First = (CoachPipTalkCodes)Enum.Parse(typeof(CoachPipTalkCodes), codeInString, true);
                            }
                            catch (Exception)
                            {
#if DEBUG
                                Debug.WriteLine(String.Format("Cannot convert string '{0}' into a CoachPipTalkCodes.", codeInString));
#endif
                                ptInfo.First = CoachPipTalkCodes.GreetingsHello;
                            }

                            // odczytanie drugiej liniki
                            codeInString = streamReader.ReadLine();
                            try
                            {
                                ptInfo.Second = (CoachPipTalkCodes)Enum.Parse(typeof(CoachPipTalkCodes), codeInString, true);
                            }
                            catch (Exception)
                            {
#if DEBUG
                                Debug.WriteLine(String.Format("Cannot convert string '{0}' into a CoachPipTalkCodes.", codeInString));
#endif
                                ptInfo.Second = CoachPipTalkCodes.PraiseGreatMove;
                            }

                            // odczytanie trzeciej liniki
                            codeInString = streamReader.ReadLine();
                            try
                            {
                                ptInfo.Third = (CoachPipTalkCodes)Enum.Parse(typeof(CoachPipTalkCodes), codeInString, true);
                            }
                            catch (Exception)
                            {
#if DEBUG
                                Debug.WriteLine(String.Format("Cannot convert string '{0}' into a CoachPipTalkCodes.", codeInString));
#endif
                                ptInfo.Third = CoachPipTalkCodes.SpecificComeOn;
                            }

                            // odczytanie czwartej liniki
                            codeInString = streamReader.ReadLine();
                            try
                            {
                                ptInfo.Fourth = (CoachPipTalkCodes)Enum.Parse(typeof(CoachPipTalkCodes), codeInString, true);
                            }
                            catch (Exception)
                            {
#if DEBUG
                                Debug.WriteLine(String.Format("Cannot convert string '{0}' into a CoachPipTalkCodes.", codeInString));
#endif
                                ptInfo.Fourth = CoachPipTalkCodes.SympathyItAintOver;
                            }
                        }
                    }
                }
            }
            if (!fileExist)
            {
                ptInfo.First = CoachPipTalkCodes.GreetingsHello;
                ptInfo.Second = CoachPipTalkCodes.PraiseGreatMove;
                ptInfo.Third = CoachPipTalkCodes.SpecificComeOn;
                ptInfo.Fourth = CoachPipTalkCodes.SympathyItAintOver;
            }
        }

        public static void CleanIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(pipTalkData))
                    isolatedStorageFile.DeleteFile(pipTalkData);
            }
        }
    }
}
