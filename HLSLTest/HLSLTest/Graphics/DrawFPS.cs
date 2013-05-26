using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{

    public class DebugMessage : DrawableGameComponent
    {
        private double _fps;
        private double _updateInterval = 1.0;
        private double _timeSinceLastUpdate = 0.0;
        private double _framecount = 0.0;
        private double _elapsed;
        private SpriteFont _font;
        private SpriteBatch _spriteBatch;
        private Vector2 _position = Vector2.Zero;
        //private Game1 game;

        public DebugMessage(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            // TODO: Construct any child components here
            _spriteBatch = spriteBatch;
            _font = this.Game.Content.Load<SpriteFont>("Fonts\\menuFont");
        }

        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
            this.LoadContent();
        }

        protected override void LoadContent()//(string filePath)
        {
            base.LoadContent();

            _font = this.Game.Content.Load<SpriteFont>("Fonts\\menuFont");
        }

        public override void Draw(GameTime gameTime)
        {
            _elapsed = gameTime.ElapsedGameTime.TotalSeconds;
            _framecount++;
            _timeSinceLastUpdate += _elapsed;
            if (_timeSinceLastUpdate > _updateInterval) {
                _fps = _framecount / _timeSinceLastUpdate;
                _timeSinceLastUpdate -= _updateInterval;
                _framecount = 0;

                // FPSをコンソールに出力
                System.Diagnostics.Debug.WriteLine("FPS: " + _fps.ToString("00.000") + " RT: " + gameTime.ElapsedGameTime.TotalSeconds);
            }

            base.Draw(gameTime);

            // FPSを画面に出力
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, "FPS: " + _fps.ToString("00.000"), _position, Color.White);
            _spriteBatch.End();
        }
    }
}
