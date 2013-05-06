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
    public class HeatMap
    {
        public Texture2D texture;

        Effect heatmap;
        RenderTargetState rts;

        public void Load(GraphicsDevice graf, ContentManager cont)
        {
            heatmap = cont.Load<Effect>("Shaders\\Heatmap");
            rts = new RenderTargetState(graf, Game1.TextureWidth, Game1.TextureHeight, Game1.TextureWidth, Game1.TextureHeight);
        }

        public void Generate(SpriteBatch sp)
        {
            rts.BeginRenderToTexture();
            heatmap.Parameters["esun"].SetValue(Game1.ESun);
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            heatmap.Begin();
            heatmap.CurrentTechnique.Passes[0].Begin();
            sp.Draw(Game1.mercator, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            heatmap.CurrentTechnique.Passes[0].End();
            heatmap.End();
            texture = rts.EndRenderGetTexture();
        }
    }
}
