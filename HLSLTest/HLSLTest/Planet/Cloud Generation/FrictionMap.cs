using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace HLSLTest
{
    public class FrictionMap
    {
        public Texture2D texture;

        Effect friction;
        RenderTargetState rts;

        public void Load(GraphicsDevice graf, ContentManager cont)
        {
            friction = cont.Load<Effect>("Shaders\\Friction");
            rts = new RenderTargetState(graf, Game1.TextureWidth, Game1.TextureHeight, Game1.TextureWidth, Game1.TextureHeight);
        }

        public void Generate(SpriteBatch sp)
        {
            rts.BeginRenderToTexture();
            friction.Parameters["cland"].SetValue(Game1.CLand);
            friction.Parameters["cwater"].SetValue(Game1.CWater);
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            friction.Begin();
            friction.CurrentTechnique.Passes[0].Begin();
            sp.Draw(Game1.mercator, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            friction.CurrentTechnique.Passes[0].End();
            friction.End();
            texture = rts.EndRenderGetTexture();
        }
    }
}
