using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace TableGoal
{
    /// <summary>
    /// Graficzna reprezentacja uciekaj¹cego czasu - znikaj¹ca strza³ka w prawym lub lewym dolnym roku
    /// </summary>
    class RunningTimeIndicator : UIElement
    {
        int thinkingTimeLimit;
        int thinkingTimer;
        int waitingTimeLimit;
        /// <summary>
        /// Czas czekania na ruch przeciwnika [s]
        /// </summary>
        public int WaitingTimeLimit
        {
            get { return waitingTimeLimit; }
            set
            {
                if (waitingTimeLimit * 1000 <= thinkingTimeLimit)
                { 
                    throw new Exception("waitingTimeLimit has to be greater than thinkingTimeLimit!"); 
                }
                waitingTimeLimit = value * 1000;
            }
        }
        int waitingTimer;
        float decreasingPart;
        bool firstPlayerHaveMove;
        Rectangle secondPlayerRect;
        Color secondPlayerColor;
        /// <summary>
        /// Obiekt w mutexie s³u¿¹cy do lockowania
        /// </summary>
        readonly object padlock;
        bool isWaiting;
        bool userWarned;
        /// <summary>
        /// Czas do koñca tury w którym wysy³ana zostaje notifikacja o up³ywie czasu. [ms]
        /// </summary>
        int WARNING_TIME_NOTIFICATIONS = 5999;

        public bool IsWaiting
        {
            get { return isWaiting; }
            set
            {
                if (isWaiting)
                {
                    return;
                }
                isWaiting = value;
                isThinking = !isWaiting;
                ResetVariables();
#if DEBUG
                Debug.WriteLine(String.Format("\t-> Waiting now."));
#endif
            }
        }
        bool isThinking;

        public bool IsThinking
        {
            get {
                bool rBool = false;
                lock (padlock)
                {
                    rBool = isThinking;
                }
                return rBool; 
            }
            set
            {
                lock (padlock)
                {
                    if (isThinking)
                    {
                        return;
                    }
                    isThinking = value;
                    isWaiting = !isThinking;
                    ResetVariables();
#if DEBUG
                Debug.WriteLine(String.Format("\t-> Thinking now."));
#endif
                }
            }
        }

        /// <summary>
        /// Czy pierwszy gracz teraz wykonuje ruch
        /// </summary>
        public bool FirstPlayerHaveMove
        {
            get { return firstPlayerHaveMove; }
            set { firstPlayerHaveMove = value; }
        }
        bool isActive;

        /// <summary>
        /// Czy aktywna jest animacja odliczania czasu
        /// </summary>
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }


        /// <summary>
        /// Delegat dla <code>ThinkingTimeIsUp</code>
        /// </summary>
        public delegate void ThinkigTimeIsUpEventHandler();
        /// <summary>
        /// Wydarzenie któro jest uruchamiane gdy czas na podjêcie decyzji dobiegnie koñca.
        /// </summary>
        public event ThinkigTimeIsUpEventHandler ThinkingTimeIsUp;

        /// <summary>
        /// Delagat dla <code>WaitingTimeIsUp</code>
        /// </summary>
        public delegate void WaitingTimeIsUpEventHandler();
        /// <summary>
        /// Wydarzenie któro jest uruchamiane gdy czas czekania na ruch przeciwnika dobiegnie koñca.
        /// </summary>
        public event WaitingTimeIsUpEventHandler WaitingTimeIsUp;

        /// <summary>
        /// Delegat dla <code>WarningTimeStarted</code>
        /// </summary>
        public delegate void WarningTimeStartedEventHandler(int warningTime);
        /// <summary>
        /// Wydarzenie któro jest uruchamiane gdy pozosta³o 5 sekund do koñca czasu myœlenia.
        /// </summary>
        public event WarningTimeStartedEventHandler WarningTimeStarted;


        /// <summary>
        /// Tworzy obiekt który wymbolizuje up³ywaj¹cy czas na turê.
        /// </summary>
        /// <param name="firstPlayerDestinationRect">Prostok¹t w który zostanie wrysowana tekstura pierwszego gracza</param>
        /// <param name="firstPlayerColor">Kolor obiektu pierwszego gracza</param>
        /// <param name="secondPlayerDestinationRect">Prostok¹t w który zostanie wrysowana tekstura drugiego gracza</param>
        /// <param name="secondPlayerColor">Kolor obiektu drugiego gracza</param>
        /// <param name="thinkingTime">Czas na turê [s]. Po up³ywie czasu wysy³any jest event <code>TimeIsUp</code> o ile zosta³ podpiêty. Równie¿ automatycznie powiadamiany jest po³¹czony indykator <code>connectedIndicator</code></param>
        /// <param name="firstPlayerHaveMove">Czy pierwszy gracz ma teraz ruch</param>
        /// <param name="isActive">Czy ma byæ wyœwietlana animacja uciekaj¹cego czasu</param>
        public RunningTimeIndicator(Rectangle firstPlayerDestinationRect, Color firstPlayerColor, Rectangle secondPlayerDestinationRect, Color secondPlayerColor, int thinkingTime, bool isActive)
        {
            this.TextureName = "arrowGo";
            this.Color = firstPlayerColor;
            this.DestinationRectangle = firstPlayerDestinationRect;
            this.secondPlayerColor = secondPlayerColor;
            this.secondPlayerRect = secondPlayerDestinationRect;
            this.LayerDepth = 0.5f;
            thinkingTimer = 0;
            thinkingTimeLimit = thinkingTime * 1000;
            decreasingPart = 0;
            this.firstPlayerHaveMove = false;
            this.isActive = isActive;
            waitingTimeLimit = thinkingTimeLimit;
            isWaiting = false;
            isThinking = true;
            padlock = new object();
            userWarned = false;
        }

        /// <summary>
        /// Tworzy obiekt który wymbolizuje up³ywaj¹cy czas na turê.
        /// </summary>
        /// <param name="firstPlayerDestinationRect">Prostok¹t w który zostanie wrysowana tekstura pierwszego gracza</param>
        /// <param name="firstPlayerColor">Kolor obiektu pierwszego gracza</param>
        /// <param name="secondPlayerDestinationRect">Prostok¹t w który zostanie wrysowana tekstura drugiego gracza</param>
        /// <param name="secondPlayerColor">Kolor obiektu drugiego gracza</param>
        /// <param name="thinkingTime">Czas na turê [s]. Po up³ywie czasu wysy³any jest event <code>TimeIsUp</code> o ile zosta³ podpiêty. Równie¿ automatycznie powiadamiany jest po³¹czony indykator <code>connectedIndicator</code></param>
        /// <param name="waitingTime">Czas który czeka siê na ruch przeciwnika [s]</param>
        /// <param name="firstPlayerHaveMove">Czy pierwszy gracz ma teraz ruch</param>
        /// <param name="isActive">Czy ma byæ wyœwietlana animacja uciekaj¹cego czasu</param>
        /// <exception cref="Exception">Wyrzuca generalny wyj¹tek jeœli czas czekania jest mniejszy od czasu na myœlenie.</exception>
        public RunningTimeIndicator(Rectangle firstPlayerDestinationRect, Color firstPlayerColor, Rectangle secondPlayerDestinationRect, Color secondPlayerColor, int thinkingTime, int waitingTime, bool isActive)
        {
            if (waitingTime <= thinkingTime)
            {
                throw new Exception("waitingTime has to be greater than thinkingTime!");
            }
            this.TextureName = "arrowGo";
            this.Color = firstPlayerColor;
            this.DestinationRectangle = firstPlayerDestinationRect;
            this.secondPlayerColor = secondPlayerColor;
            this.secondPlayerRect = secondPlayerDestinationRect;
            this.LayerDepth = 0.5f;
            thinkingTimer = 0;
            thinkingTimeLimit = thinkingTime * 1000;
            waitingTimer = 0;
            waitingTimeLimit = waitingTime * 1000;
            decreasingPart = 0;
            this.firstPlayerHaveMove = false;
            this.isActive = isActive;
            isWaiting = false;
            isThinking = true;
            padlock = new object();
            userWarned = false;
        }

        /// <summary>
        /// Resetuje <code>decreasingPart</code> i ustawia <code>thinkingTimer</code> i <code>waitingTime</code> na zero.
        /// </summary>
        public void ResetVariables()
        {
            decreasingPart = 0;
            thinkingTimer = 0;
            waitingTimer = 0;
            userWarned = false;
#if DEBUG
            //Debug.WriteLine(String.Format("Reset VARIABLE | decreasingPart = {0} | thinkingTimer = {1} | waitingTimer = {2}", decreasingPart, thinkingTimer, waitingTimer));
#endif
        }

        /// <summary>
        /// Ustawia <code>isActive</code> na false, co uniemo¿liwia uruchamianie kodu z funkcji <code>Update</code>
        /// </summary>
        public void PauseElapsing()
        {
            isActive = false;
        }

        /// <summary>
        /// Ustawia <code>isActive</code> na true, co pozwala na wykonywanie kodu z funkcji <code>Update</code>
        /// </summary>
        public void ResumeElapsing()
        {
            isActive = true;
        }

        /// <summary>
        /// Przekazuje ruch od jednego gracza do drugiego.
        /// </summary>
        public void TurnOver()
        {
            if (isThinking)
            {
                if (ThinkingTimeIsUp != null)
                {
#if DEBUG
                    Debug.WriteLine(String.Format("Thinking time is over"));
#endif
                    ThinkingTimeIsUp();
                    IsWaiting = true;
                }
            }
            else if (isWaiting)
            {
                if (WaitingTimeIsUp != null)
                {
#if DEBUG
                    Debug.WriteLine(String.Format("Waiting time is over"));
#endif
                    WaitingTimeIsUp();
                    IsThinking = true;
                }
            }
        }
        
        public override void Update(GameTime gametime)
        {
            if (!isActive)
            {
                return;
            }
            
            if (isThinking)
            {
                thinkingTimer += gametime.ElapsedGameTime.Milliseconds;
                decreasingPart = (float)thinkingTimer / thinkingTimeLimit * DestinationRectangle.Width;
                if (!userWarned && (thinkingTimeLimit - thinkingTimer) < WARNING_TIME_NOTIFICATIONS)
                {
                    userWarned = true;
                    WarningTimeStarted(WARNING_TIME_NOTIFICATIONS);
                }
                if (thinkingTimer >= thinkingTimeLimit)
                {
                    TurnOver();
                }
            }
            else if (isWaiting)
            {
                waitingTimer += gametime.ElapsedGameTime.Milliseconds;
                decreasingPart = (float)waitingTimer / waitingTimeLimit * DestinationRectangle.Width;
                if (waitingTimer >= waitingTimeLimit)
                {
                    TurnOver();
                }
            }
            base.Update(gametime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (firstPlayerHaveMove)
            {
                spriteBatch.Draw(ObjectTexture,
                                DestinationRectangle,
                                null,
                                Color,
                                0,
                                new Vector2(decreasingPart, 0),
                                SpriteEffects.None,
                                LayerDepth);
                
            }
            else
            {
                spriteBatch.Draw(ObjectTexture,
                                secondPlayerRect,
                                null,
                                secondPlayerColor,
                                0,
                                new Vector2(-decreasingPart, 0),
                                SpriteEffects.FlipHorizontally,
                                LayerDepth);
            }
        }

        public override void LoadTexture(ContentManager contentManager)
        {
            this.ObjectTexture = contentManager.Load<Texture2D>(TextureName);
        }

        /// <summary>
        /// Resetuje dane i ustawia wskaŸnik na myœlenie
        /// </summary>
        public void ResetForThinking()
        {
            Debug.WriteLine("Reset For Thinking");
            isThinking = true;
            isWaiting = false;
            ResetVariables(); 
        }

        /// <summary>
        /// Resetuje dane i ustawia wskaŸnik na czekanie
        /// </summary>
        public void ResetForWaiting()
        {
            Debug.WriteLine("Reset For Waiting");
            isThinking = false;
            isWaiting = true;
            ResetVariables();  
        }
    }
}
