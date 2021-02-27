using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Phone.Tasks;
using System.Diagnostics;

namespace TableGoal
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MainMenuState : GameState
    {
        Menu menu;
        UIPicture mail;
        UIPicture rateAndReview;
        UIPicture stats;
        float menuCooldown = 0.15f;
        readonly float MENUCOOLDOWN = 0.15f;
        bool clickAnimationOngoing = false;
        /// <summary>
        /// Czy osoba chce pozbyæ siê reklam.
        /// </summary>
        Rectangle _removeAdsRect;
        /// <summary>
        /// Czy osoba kliknê³a w prostok¹t
        /// </summary>
        bool _removeAdsClicked = false;
        SpriteFont _buyFont;
        bool _isTrial;

        public MainMenuState()
        {
            menu = new Menu("Backgrounds/Background", new Rectangle(200, 20, 400, 440));
            menu.DestinationRectangle = new Rectangle(0, 0, 800, 480);
            menu.AddButton("MenusElements/PlayBtn", ButtonType.NewGame);
            menu.AddButton("MenusElements/OptionsBtn", ButtonType.Options);
            menu.AddButton("MenusElements/HowToPlayBtn", ButtonType.HowToPlay);
            menu.AddButton("MenusElements/QuitBtn", ButtonType.Exit);
            mail = new UIPicture("MenusElements/envelope", new Rectangle(701, 425, 85, 45));
            mail.Color = Color.Black;
            menu.AddElement(mail);
            rateAndReview = new UIPicture("MenusElements/rate", new Rectangle(0, 388, 120, 92));
            rateAndReview.Color = Color.Black;
            menu.AddElement(rateAndReview);
            stats = new UIPicture("MenusElements/profile", new Rectangle(690, 10, 100, 100));
            stats.Color = Color.Black;
            menu.AddElement(stats);
            _removeAdsRect = new Rectangle(0, 0, 150, 110);
            _isTrial = ApplicationLicence.IsTrialMode;
            /*
             * Skoro tu jesteœmy to na pewno nie trwa mecz.
             */
            GameVariables.Instance.ActiveWorldCupMatch = false;
            TableGoal.Players.Clear();
        }

        public override void LoadContent()
        {
            OptionsWriterReader.LoadSetttingsFromIsolatedStorage();
            GameVariables.Instance.MusicOn = OptionsWriterReader.opts.Music;
            GameVariables.Instance.SoundsOn = OptionsWriterReader.opts.Sound;
            _buyFont = GameManager.Game.Content.Load<SpriteFont>("Fonts/TRIAL_font");
            menu.LoadTexture(GameManager.Game.Content);
            if (!AudioManager.Instance.PlaybackAlreadyStarted)
                AudioManager.PlayMusic("crowd");
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (menu.PressedButton != ButtonType.None)
            {
                ButtonClicked(gameTime);
                if (clickAnimationOngoing)
                    return;
            }

            if (menu.PressedButton == ButtonType.NewGame)
            {
                GameManager.AddState(new NewGameMenu());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.Options)
            {
                GameManager.AddState(new OptionMenuState());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.BuyFullVersion)
            {
                MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
                marketplaceDetailTask.ContentIdentifier = "abc8ed4a-64ff-4a10-a965-75b34d52806d";
                try
                {
                    marketplaceDetailTask.Show();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine(" =========  MainMenu: EXCEPTION when calling DetailTask  =============");
                    Debug.WriteLine(ex.Message);
#endif
                }
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.HowToPlay)
            {
                GameManager.AddState(new HowToPlayState());
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
                menu.PressedButton = ButtonType.None;
            }
            if (menu.PressedButton == ButtonType.Exit || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                GameManager.Game.Exit();
            }
            if (mail.Pressed)
            {
                AudioManager.PlaySound("selected");
                EmailComposeTask email = new EmailComposeTask();
                if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "pl")
                {
                    email.Subject = "Paper Soccer - opinie i sugestie";
                    email.To = "madbrainstudio@hotmail.com";
                    email.Body = "Dziêki za œci¹gniêcie gry Paper Soccer - pi³karzyki.\nJeœli masz jakieœ sugestie lub opinie chêtnie je przeczytam.";
                }
                else
                {
                    email.Subject = "Paper Soccer - opinion and suggestion.";
                    email.To = "madbrainstudio@hotmail.com";
                    email.Body = "Thanks for downloading Paper Soccer game.\nIf you have any suggestions or opinions concerning the game, please send me an email.";
                }
                try
                {
                    email.Show();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine(" =========  EXCEPTION when sending email  =============");
                    Debug.WriteLine(ex.Message);
#endif
                }
                mail.Pressed = false;
            }
            if (rateAndReview.Pressed)
            {
                AudioManager.PlaySound("selected");
                MarketplaceReviewTask review = new MarketplaceReviewTask();
                try
                {
                    review.Show();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine(" =========  EXCEPTION when calling ReviewTask  =============");
                    Debug.WriteLine(ex.Message);
#endif
                }
                rateAndReview.Pressed = false;
            }
            if (_removeAdsClicked)
            {
                _removeAdsClicked = false;
                
                if (!_isTrial)
                {
                    return;
                }

                if (!Guide.IsVisible)
                {
                    string head = "Better without ads";
                    string msg = "Pay for the app and remove advertisements.";
                    string confirm = "Let's do it";
                    string deny = "No, thanks";
                    if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "pl")
                    {
                        head = "Lepiej bez reklam.";
                        msg = "PrzejdŸ do sklepu, zakup apkê i gotowe - zero reklam.";
                        confirm = "Do dzie³a";
                        deny = "Nie, dziêki";
                    }

                    Guide.BeginShowMessageBox(head,
                                              msg,
                                              new string[] { confirm, deny },
                                              0,
                                              MessageBoxIcon.Alert,
                                              new AsyncCallback(OnMessageBoxClosed),
                                              null);
                }
            }
            if (stats.Pressed)
            {
                stats.Pressed = false;
                GameManager.AddState(new PlyerProfileState());
                AudioManager.PlaySound("selected");
                this.ScreenState = global::TableGoal.ScreenState.Hidden;
            }
        }

        /// <summary>
        /// Callback function for MessageBox
        /// </summary>
        /// <param name="ar">Encapsulated result.</param>
        private void OnMessageBoxClosed(IAsyncResult ar)
        {
            int? buttonIndex = Guide.EndShowMessageBox(ar);
            switch (buttonIndex)
            {
                case 0:
                    MarketplaceDetailTask marketDetails = new MarketplaceDetailTask();
                    marketDetails.ContentIdentifier = "2a52b226-048c-453d-bdff-43ce4c18e6b9";
                    marketDetails.Show();
                    break;
                case 1:
                    break;
                default:
                    break;
            }
        }

        public void ButtonClicked(GameTime gameTime)
        {
            menuCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (menuCooldown <= 0.0f)
            {
                menuCooldown = MENUCOOLDOWN;
                clickAnimationOngoing = false;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GameManager.SpriteBatch;
            spriteBatch.Begin();
            menu.Draw(spriteBatch);
            if (_isTrial)
            {
                if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "pl")
                {
                    spriteBatch.DrawString(_buyFont, "  Bez\nreklam", new Vector2(10, 20), Color.DarkBlue);
                }
                else
                {
                    spriteBatch.DrawString(_buyFont, "Remove\n    ads", new Vector2(10, 20), Color.DarkBlue);
                }
            }
            spriteBatch.End();
        }

        public override void HandleInput(GameTime gameTime, Input input)
        {
            if (clickAnimationOngoing)
                return;
            if (input.Gestures.Count > 0)
            {
                if (input.Gestures[0].GestureType == GestureType.Tap)
                {
                    if (_isTrial)
                    {
                        if (_removeAdsRect.Contains((int)input.Gestures[0].Position.X, (int)input.Gestures[0].Position.Y))
                        {
                            AudioManager.PlaySound("selected");
                            _removeAdsClicked = true;
                        }
                    }
                    menu.WasPressed(input.Gestures[0].Position);
                    if (menu.PressedButton != ButtonType.None)
                    {
                        clickAnimationOngoing = true;
                        AudioManager.PlaySound("selected");
                    }
                }
            }
        }
    }
}
