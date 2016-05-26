﻿/* ©2016 Hathor Gaia 
 * http://HathorsLove.com
 * 
 * Licensed Under GNU GPL 3:
 * http://www.gnu.org/licenses/gpl-3.0.html
 */
 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Ruge.ViewportAdapters;
using MonoGame.Ruge.CardEngine;
using MonoGame.Ruge.DragonDrop;

namespace OpenSolitaire {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class OpenSolitaireGame : Game {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BoxingViewportAdapter viewport;

        const int WindowWidth = 1035;
        const int WindowHeight = 600;

        const int spacer = 10;
        const int cardWidth = 125;
        const int cardHeight = 156;
        
        Texture2D cardSlot, cardBack, refreshMe, newGame;
        Rectangle newGameRect;
        Color newGameColor;
        
        CardTable table;

        DragonDrop<IDragonDropItem> dragonDrop;
        
        private MouseState prevMouseState;

        public OpenSolitaireGame() {

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set the screen resolution
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;

            this.Window.Title = "Open Solitaire";
            this.Window.AllowUserResizing = true;

            IsMouseVisible = true;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {

            viewport = new BoxingViewportAdapter(Window, GraphicsDevice, WindowWidth, WindowHeight);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            cardSlot = Content.Load<Texture2D>("card_slot");
            cardBack = Content.Load<Texture2D>("card_back_green");
            refreshMe = Content.Load<Texture2D>("refresh");
            newGame = Content.Load<Texture2D>("new_game");

            dragonDrop = new DragonDrop<IDragonDropItem>(this, spriteBatch, viewport);

            // table creates a fresh table.deck
            table = new CardTable(dragonDrop, cardBack, cardSlot, 0, 30);

            table.InitializeTable();

            // load up the card assets for the new deck
            foreach (Card card in table.drawPile.cards) {

                string location = card.suit.ToString() + card.rank.ToString();
                card.SetTexture(Content.Load<Texture2D>(location));

                dragonDrop.Add(card);

            }

            table.SetTable();
            
            Components.Add(dragonDrop);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            MouseState mouseState = Mouse.GetState();
            Point point = viewport.PointToScreen(mouseState.X, mouseState.Y);

            if (table.isSetup && !table.isAnimating) { 

                newGameRect = new Rectangle(310, 20, newGame.Width, newGame.Height);
                newGameColor = Color.White;


                if (newGameRect.Contains(point)) {

                    newGameColor = Color.Aqua;

                    if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released) {

                        table.Clear();


                        // load up the card assets for the new deck
                        foreach (Card card in table.drawPile.cards) {

                            string location = card.suit.ToString() + card.rank.ToString();
                            card.SetTexture(Content.Load<Texture2D>(location));

                            dragonDrop.Add(card);

                        }

                        table.SetTable();

                    }
                }

            }

            prevMouseState = mouseState;


            table.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {

            GraphicsDevice.Clear(Color.SandyBrown);

            spriteBatch.Begin(transformMatrix: viewport.GetScaleMatrix(), samplerState: SamplerState.LinearWrap);
            
            foreach (Slot slot in table.slots) slot.Draw(gameTime);
            
            //all this does is figure out where to center the refresh icon in relation to the draw slot
            Rectangle refreshRect = new Rectangle((int)table.slots[0].Position.X + cardWidth / 2 - refreshMe.Width / 2,
                (int)table.slots[0].Position.Y + cardHeight / 2 - refreshMe.Height / 2, refreshMe.Width, refreshMe.Height);

            spriteBatch.Draw(refreshMe, refreshRect, Color.White);

            spriteBatch.Draw(newGame, newGameRect, newGameColor);

            if (table.isSetup) {

                foreach (Slot slot in table.slots) {

                    foreach (Card card in slot.stack.cards_Zsort) card.Draw(gameTime);

                }


                foreach (Card card in table.drawPile.cards_Zsort) card.Draw(gameTime);
                foreach (Card card in table.discardPile.cards_Zsort) card.Draw(gameTime);


                // fix the Z-ordering
                foreach (var item in dragonDrop.Items) {

                    var type = item.GetType();

                    if (type == typeof(Card)) {

                        Card card = (Card)item;

                        if (card.IsSelected || card.returnToOrigin) {

                            card.Draw(gameTime);

                            while (card.Child != null) {
                                card = card.Child;
                                card.Draw(gameTime);
                            }

                        }

                    }

                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}