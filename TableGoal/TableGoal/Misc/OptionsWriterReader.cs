using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Diagnostics;

namespace TableGoal
{
    static class OptionsWriterReader
    {
        private readonly static string settings = "PaperSoccerSettings.txt";
        
        public class Options
        {
            public bool Music { get; set; }
            public bool Sound { get; set; }
            public bool DefaultStyle { get; set; }
        }

        public static Options opts = new Options()
        {
            Music = false,
            Sound = false,
            DefaultStyle = true
        };

        public static void SaveSettingsToIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = isolatedStorageFile.CreateFile(settings))
                {
                    using (StreamWriter textWriter = new StreamWriter(fileStream))
                    {
                        textWriter.WriteLine(opts.Music);
                        textWriter.WriteLine(opts.Sound);
                        textWriter.WriteLine(opts.DefaultStyle);
                    }
                }
            }
        }

        public static void LoadSetttingsFromIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(settings))
                {
                    using (IsolatedStorageFileStream fileStream = isolatedStorageFile.OpenFile(settings, FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            string a = streamReader.ReadToEnd();
                            streamReader.BaseStream.Position = 0;
                            try
                            {
                                opts.Music = bool.Parse(streamReader.ReadLine());
                                opts.Sound = bool.Parse(streamReader.ReadLine());
                                opts.DefaultStyle = bool.Parse(streamReader.ReadLine());
                            }
                            catch (Exception ex)
                            {
                                opts.Music = false;
                                opts.Sound = false;
                                opts.DefaultStyle = true;
#if DEBUG
                                Debug.WriteLine(" =========  EXCEPTION when sending email  =============");
                                Debug.WriteLine(ex.Message);
#endif
                            }
                        }
                    }
                }
                else
                {
                    opts.Music = true;
                    opts.Sound = true;
                    opts.DefaultStyle = true;
                }
            }
        }

    }
}
