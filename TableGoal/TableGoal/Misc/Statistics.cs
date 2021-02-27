using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.IO.IsolatedStorage;

namespace TableGoal
{
    /// <summary>
    /// Klasa przetrzymuj¹ce tylko rozegrane, wygrane, przegrane, zremisowane i przerwane mecze.
    /// </summary>
    public class DiffLevelStats
    {
        /// <summary>
        /// Konstruktor - tworzy obiekt z wyzerowanymi statystykami.
        /// </summary>
        public DiffLevelStats()
        {
            RozegraneMecze = 0;
            WygraneMecze = 0;
            PrzegraneMecze = 0;
            Remisy = 0;
            Przerwane = 0;
        }
        /// <summary>
        /// Iloœæ rozegranych meczy.
        /// </summary>
        public int RozegraneMecze { set; get; }
        /// <summary>
        /// Iloœæ wygranych meczy.
        /// </summary>
        public int WygraneMecze { set; get; }
        /// <summary>
        /// Iloœæ przegranych meczy.
        /// </summary>
        public int PrzegraneMecze { set; get; }
        /// <summary>
        /// Iloœæ zremisowanych meczy.
        /// </summary>
        public int Remisy { set; get; }
        /// <summary>
        /// Iloœæ przerwanych meczy.
        /// </summary>
        public int Przerwane { set; get; }
        /// <summary>
        /// Wygrany kolejny mecz.
        /// </summary>
        public void Wygrana()
        {
            WygraneMecze++;
            RozegraneMecze++;
        }
        /// <summary>
        /// Przegrany kolejny mecz.
        /// </summary>
        public void Przegrana()
        {
            PrzegraneMecze++;
            RozegraneMecze++;
        }
        /// <summary>
        /// Zremisowany kolejny mecz.
        /// </summary>
        public void Remis()
        {
            Remisy++;
            RozegraneMecze++;
        }
        /// <summary>
        /// Przerwany kolejny mecz.
        /// </summary>
        public void Przerwany()
        {
            Przerwane++;
            RozegraneMecze++;
        }
        /// <summary>
        /// Czyœci wszystkie informacje - zeruje liczniki.
        /// </summary>
        public void Clear()
        {
            RozegraneMecze = 0;
            WygraneMecze = 0;
            PrzegraneMecze = 0;
            Remisy = 0;
            Przerwane = 0; 
        }
    }

    public sealed class Statistics
    {
        /// <summary>
        /// Nazwa pliku z danymi.
        /// </summary>
        static readonly string filename = "Statistics.dat";
        
        public Statistics()
        {
            m_stracone_bramki = 0;
            m_zdobyte_bramki = 0;
            m_najwiecej_odbic = 0;
            m_EASYstats = new DiffLevelStats();
            m_MEDIUMstats = new DiffLevelStats();
            m_HARDstats = new DiffLevelStats();
            m_MULTIPLAYERstats = new DiffLevelStats();
            m_calkowita_dlugosc_rozgrywek = 0;
            m_ilosc_rozegranych_meczy = 0;
            m_srednia_glugosc_meczu = 0;
        }
        
        public static Statistics Instance
        {
            get
            {
                return Nested._gameVars;
            }
            internal set
            { Nested._gameVars = value; }
        }

        private class Nested
        {
            static Nested() { }
            internal static Statistics _gameVars = new Statistics();
        }


        #region daneDoStatystyk
        int m_stracone_bramki;
        int m_zdobyte_bramki;
        int m_najwiecej_odbic;
        DiffLevelStats m_EASYstats;
        DiffLevelStats m_MEDIUMstats;
        DiffLevelStats m_HARDstats;
        DiffLevelStats m_MULTIPLAYERstats;
        int m_calkowita_dlugosc_rozgrywek;
        int m_ilosc_rozegranych_meczy;
        int m_srednia_glugosc_meczu;
        int m_WC_Attended;
        int m_WC_First;
        int m_WC_Second;
        int m_WC_Third;
        #endregion

        /// <summary>
        /// Iloœc straconych bramek przez gracza.
        /// </summary>
        public int StraconeBramki
        {
            get { return m_stracone_bramki; }
            set { m_stracone_bramki = value; }
        }
        /// <summary>
        /// Iloœæ zdobytyh bramek przez gracza.
        /// </summary>
        public int ZdobyteBramki
        {
            get { return m_zdobyte_bramki; }
            set { m_zdobyte_bramki = value; }
        }
        /// <summary>
        /// Najd³u¿sze odbicie wykonane przez gracza.
        /// </summary>
        public int Najwiecejodbic
        {
            get { return m_najwiecej_odbic; }
            set { m_najwiecej_odbic = value; }
        }
        /// <summary>
        /// Statystyki rozegranych, wygranych, przegranych, zremisowanych i przerwanych meczy na poziomie trudnoœci EASY.
        /// </summary>
        public DiffLevelStats EASYstats
        {
            get { return m_EASYstats; }
            set { m_EASYstats = value; }
        }
        /// <summary>
        /// Statystyki rozegranych, wygranych, przegranych, zremisowanych i przerwanych meczy na poziomie trudnoœci MEDIUM.
        /// </summary>
        public DiffLevelStats MEDIUMstats
        {
            get { return m_MEDIUMstats; }
            set { m_MEDIUMstats = value; }
        }
        /// <summary>
        /// Statystyki rozegranych, wygranych, przegranych, zremisowanych i przerwanych meczy na poziomie trudnoœci HARD.
        /// </summary>
        public DiffLevelStats HARDstats
        {
            get { return m_HARDstats; }
            set { m_HARDstats = value; }
        }
        /// <summary>
        /// Statystyki rozegranych, wygranych, przegranych, zremisowanych i przerwanych meczy w trybie MULTI.
        /// </summary>
        public DiffLevelStats MULTIPLAYERstats
        {
            get { return m_MULTIPLAYERstats; }
            set { m_MULTIPLAYERstats = value; }
        }
        /// <summary>
        /// Ca³kowicy czas spêdzony w meczach. Ustala lub zwraca czas w sekundach.
        /// </summary>
        public int CzasSpedzonyWmeczach
        {
            get { return m_calkowita_dlugosc_rozgrywek; }
            set { m_calkowita_dlugosc_rozgrywek = value; }
        }
        /// <summary>
        /// Iloœc rozegranych meczy. Ustala lub zwraca iloœæ rozegranych meczy.
        /// </summary>
        public int IloscRozegranychMeczy
        {
            get { return m_ilosc_rozegranych_meczy; }
            set { m_ilosc_rozegranych_meczy = value; }
        }
        /// <summary>
        /// Œredni czas rozegranych meczy.
        /// </summary>
        public int SredniCzasMeczu
        {
            get { return m_srednia_glugosc_meczu; }
            set { m_srednia_glugosc_meczu = value; }
        }
        /// <summary>
        /// Uczestnictwa w World Cup.
        /// </summary>
        public int WC_Attended
        {
            get { return m_WC_Attended; }
            set { m_WC_Attended = value; }
        }
        /// <summary>
        /// Iloœc pierwszych miejsc w World Cup.
        /// </summary>
        public int WC_First
        {
            get { return m_WC_First; }
            set { m_WC_First = value; }
        }
        /// <summary>
        /// Iloœæ drugich miejsc w World Cup.
        /// </summary>
        public int WC_Second
        {
            get { return m_WC_Second; }
            set { m_WC_Second = value; }
        }
        /// <summary>
        /// Iloœæ trzecich miejsc w World Cup.
        /// </summary>
        public int WC_Third
        {
            get { return m_WC_Third; }
            set { m_WC_Third = value; }
        }

        #region Funkcje inkrementuj¹ce
        /// <summary>
        /// Strzeli³em kolejna bramkê.
        /// </summary>
        public void StrzelonyGol()
        {
            m_zdobyte_bramki++;
        }
        /// <summary>
        /// Straci³em kolejna bramkê.
        /// </summary>
        public void StraconaBramka()
        {
            m_stracone_bramki++;
        }
        /// <summary>
        /// Odbi³em sie kilka razy pod rz¹d.
        /// </summary>
        /// <param name="iloscOdbic">Iloœc odbiæ.</param>
        public void WielokrotneOdbicie(int iloscOdbic)
        {
            if (iloscOdbic > m_najwiecej_odbic)
            {
                m_najwiecej_odbic = iloscOdbic;
            }
        }
        /// <summary>
        /// Rozegra³em w³asnie (niepe³ny) mecz o okreœlonej d³ugoœci.
        /// </summary>
        /// <param name="czas">Czas spêdzony w meczu.</param>
        public void CzasMeczu(int czas)
        {
            m_calkowita_dlugosc_rozgrywek += czas;
            PoliczSredniCzasMeczu();
        }
        /// <summary>
        /// Rozpoczynam kolejne spotkanie.
        /// </summary>
        public void ZaczynamKolejnyMecz()
        {
            m_ilosc_rozegranych_meczy++;
            PoliczSredniCzasMeczu();
        }
        /// <summary>
        /// Liczy œrednia d³ugoœæ rozegranych meczów.
        /// </summary>
        public void PoliczSredniCzasMeczu()
        {
            if (m_ilosc_rozegranych_meczy == 0)
                return;
            m_srednia_glugosc_meczu = m_calkowita_dlugosc_rozgrywek / m_ilosc_rozegranych_meczy;
        }
        /// <summary>
        /// Wygrany kolejny mecz.
        /// </summary>
        public void Wygrana()
        {
            if (GameVariables.Instance.IsWiFiGame())
            {
                m_MULTIPLAYERstats.Wygrana();
                return;
            }
            switch (GameVariables.Instance.DiffLevel)
            {
                case DifficultyLevel.EASY:
                    m_EASYstats.Wygrana();
                    break;
                case DifficultyLevel.MEDIUM:
                    m_MEDIUMstats.Wygrana();
                    break;
                case DifficultyLevel.HARD:
                    m_HARDstats.Wygrana();
                    break;
            }
        }
        /// <summary>
        /// Przegrany kolejny mecz.
        /// </summary>
        public void Przegrana()
        {
            if (GameVariables.Instance.IsWiFiGame())
            {
                m_MULTIPLAYERstats.Przegrana();
                return;
            }
            switch (GameVariables.Instance.DiffLevel)
            {
                case DifficultyLevel.EASY:
                    m_EASYstats.Przegrana();
                    break;
                case DifficultyLevel.MEDIUM:
                    m_MEDIUMstats.Przegrana();
                    break;
                case DifficultyLevel.HARD:
                    m_HARDstats.Przegrana();
                    break;
            }
        }
        /// <summary>
        /// Zremisowany kolejny mecz.
        /// </summary>
        public void Remis()
        {
            if (GameVariables.Instance.IsWiFiGame())
            {
                m_MULTIPLAYERstats.Remis();
                return;
            }
            switch (GameVariables.Instance.DiffLevel)
            {
                case DifficultyLevel.EASY:
                    m_EASYstats.Remis();
                    break;
                case DifficultyLevel.MEDIUM:
                    m_MEDIUMstats.Remis();
                    break;
                case DifficultyLevel.HARD:
                    m_HARDstats.Remis();
                    break;
            }
        }
        /// <summary>
        /// Przerwany kolejny mecz.
        /// </summary>
        public void Przerwany()
        {
            if (GameVariables.Instance.IsWiFiGame())
            {
                m_MULTIPLAYERstats.Przerwany();
                return;
            }
            switch (GameVariables.Instance.DiffLevel)
            {
                case DifficultyLevel.EASY:
                    m_EASYstats.Przerwany();
                    break;
                case DifficultyLevel.MEDIUM:
                    m_MEDIUMstats.Przerwany();
                    break;
                case DifficultyLevel.HARD:
                    m_HARDstats.Przerwany();
                    break;
            }
        }
        /// <summary>
        /// W trybie multi - wygrany mecz.
        /// </summary>
        public void MULTI_wygrany()
        {
            m_MULTIPLAYERstats.Wygrana();
        }
        /// <summary>
        /// W trybie multi - przegrany mecz.
        /// </summary>
        public void MULTI_przegrany()
        {
            m_MULTIPLAYERstats.Przegrana();
        }
        /// <summary>
        /// W trybie multi - zremisowany mecz.
        /// </summary>
        public void MULTI_remis()
        {
            m_MULTIPLAYERstats.Remis();
        }
        /// <summary>
        /// W trybie multi - zerwane(opuszczone) mecze.
        /// </summary>
        public void MULTI_przerwany()
        {
            m_MULTIPLAYERstats.Przerwany();
        }

        /// <summary>
        /// Rozpoczêcie nowych mistrzostw.
        /// </summary>
        public void WejscieDoWorldCup()
        {
            m_WC_Attended++;
        }
        /// <summary>
        /// Zajête pierwsze miejsce w trybie mistrzostw.
        /// </summary>
        public void WorldCup_PierwszeMiejsce()
        {
            m_WC_First++;
        }
        /// <summary>
        /// Zajête drugie miejsce w trybie mistrzostw.
        /// </summary>
        public void WorldCup_DrugieMiejsce()
        {
            m_WC_Second++;
        }
        /// <summary>
        /// Zajête trzecie miejsce w trybie mistrzostw.
        /// </summary>
        public void WorldCup_TrzecieMiejsce()
        {
            m_WC_Third++;
        }
        /// <summary>
        /// Czyœci zapisane statystyki i zapisany plik.
        /// </summary>
        public void Clear()
        {
            this.m_calkowita_dlugosc_rozgrywek = 0;
            this.m_ilosc_rozegranych_meczy = 0;
            this.m_najwiecej_odbic = 0;
            this.m_srednia_glugosc_meczu = 0;
            this.m_stracone_bramki = 0;
            this.m_WC_Attended = 0;
            this.m_WC_First = 0;
            this.m_WC_Second = 0;
            this.m_WC_Third = 0;
            this.m_zdobyte_bramki = 0;
            this.m_EASYstats.Clear();
            this.m_MEDIUMstats.Clear();
            this.m_HARDstats.Clear();            
            ClearIsolatedStorageRelatedData();
        }

        public void MultiClear()
        {
            this.m_MULTIPLAYERstats.Clear(); 
        }

        #endregion
        /// <summary>
        /// Zapisz statystyki do pliku.
        /// </summary>
        public void Serialize()
        {
            using (IsolatedStorageFile isolatedStorageFile
                   = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //using (IsolatedStorageFileStream fileStream
                //    = isolatedStorageFile.CreateFile(filename))

                using (IsolatedStorageFileStream fileStream
                    = isolatedStorageFile.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Statistics));
                        using (TextWriter tw = streamWriter)
                        {
                            serializer.Serialize(tw, this);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Odczytaj statystyki z pliku.
        /// </summary>
        public void Deserialize()
        {
            using (IsolatedStorageFile isolatedStorageFile
                    = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(filename))
                {
                    using (IsolatedStorageFileStream fileStream
                        = isolatedStorageFile.OpenFile(filename, FileMode.Open, FileAccess.ReadWrite))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            string a = streamReader.ReadToEnd();
                            Debug.WriteLine(a);
                            streamReader.BaseStream.Position = 0;
                            XmlSerializer deserializer = new XmlSerializer(typeof(Statistics));
                            using (TextReader tr = streamReader)
                            {
                                try
                                {
                                    Statistics st = (Statistics)deserializer.Deserialize(tr);
                                    Statistics.Instance = st;
                                }
                                catch (Exception ex)
                                {
#if DEBUG
                                    Debug.WriteLine(String.Format("STATISTICS - Exception:\n", ex.Message));
#endif
                                    TableGoal.StatsOK = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Usuwa zapisany plik ze statystykami.
        /// </summary>
        public void ClearIsolatedStorageRelatedData()
        {
            using (IsolatedStorageFile isolatedStorageFile
                   = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(filename))
                    isolatedStorageFile.DeleteFile(filename);
            }
            TableGoal.StatsOK = true;
        }
    }
}