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
    public class InitialVelocity
    {
        public Texture2D texture;

        Effect velocity;
        RenderTargetState rts;
        Vector2[] sampleOffsets = new Vector2[8];
        GraphicsDevice graphics;
 
        public void Load(GraphicsDevice graf, ContentManager cont)
        {
            float dx = 1.0f / Game1.TextureWidth;
            float dy = 1.0f / Game1.TextureHeight;

            sampleOffsets[0] = new Vector2(-dx, -dy);
            sampleOffsets[1] = new Vector2(0, -dy);
            sampleOffsets[2] = new Vector2(dx, -dy);
            sampleOffsets[3] = new Vector2(-dx, 0);
            sampleOffsets[4] = new Vector2(dx, 0);
            sampleOffsets[5] = new Vector2(-dx, dy);
            sampleOffsets[6] = new Vector2(0, dy);
            sampleOffsets[7] = new Vector2(dx, dy);

            velocity = cont.Load<Effect>("Shaders\\InitialVelocity");
            velocity.Parameters["adense"].SetValue(Game1.AtmosphericDensity);
            velocity.Parameters["xpix"].SetValue(1.0f / Game1.TextureWidth);
            velocity.Parameters["ypix"].SetValue(1.0f / Game1.TextureHeight);
            velocity.Parameters["tpr"].SetValue(Game1.PlanetSurfaceVelocity);
            velocity.Parameters["SampleOffsets"].SetValue(sampleOffsets);
            graphics = graf;

        }

        public void Generate(SpriteBatch sp, Texture2D pressure, Texture2D friction)
        {
            velocity.Parameters["FrictionMap"].SetValue(friction);
            velocity.Parameters["HeightMap"].SetValue(pressure);
            rts = new RenderTargetState(graphics, Game1.TextureWidth, Game1.TextureHeight, Game1.TextureWidth, Game1.TextureHeight);
            rts.BeginRenderToTexture();
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            velocity.Begin();
            velocity.CurrentTechnique.Passes[0].Begin();
            sp.Draw(pressure, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            velocity.CurrentTechnique.Passes[0].End();
            velocity.End();
            texture = rts.EndRenderGetTexture();
        }
        
    }
}
