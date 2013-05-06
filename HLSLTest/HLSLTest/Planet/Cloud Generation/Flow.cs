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
    public class Flow
    {
        public Texture2D texture;
        public Texture2D Velocity;
        public Texture2D Density;
        public Texture2D Vorticity;
        public Texture2D Divergence;
        public Texture2D PressureOffsets;

        Texture2D current;
        Texture2D pass1;
        Texture2D pass2;

        public bool pass = false;
        Effect mover;
        RenderTargetState rts;
        RenderTargetState rts2;
        RenderTargetState rts3;
        RenderTargetState rts4;
        RenderTargetState rts5;

        Vector2[] sampleOffsets = new Vector2[8];

        public void Load(GraphicsDevice graf, ContentManager cont)
        {
            mover = cont.Load<Effect>("Shaders\\FluidFlow");
            rts = new RenderTargetState(graf, Game1.TextureWidth, Game1.TextureHeight, Game1.TextureWidth, Game1.TextureHeight);
            rts2 = new RenderTargetState(graf, Game1.TextureWidth, Game1.TextureHeight, Game1.TextureWidth, Game1.TextureHeight);
            rts3 = new RenderTargetState(graf, Game1.TextureWidth, Game1.TextureHeight, Game1.TextureWidth, Game1.TextureHeight);
            rts4 = new RenderTargetState(graf, Game1.TextureWidth, Game1.TextureHeight, Game1.TextureWidth, Game1.TextureHeight);
            rts5 = new RenderTargetState(graf, Game1.TextureWidth, Game1.TextureHeight, Game1.TextureWidth, Game1.TextureHeight);

            mover.Parameters["timestep"].SetValue(0.2f);
            mover.Parameters["dissipation"].SetValue(1);
            mover.Parameters["rdx"].SetValue(1.0f / Game1.TextureWidth);
            mover.Parameters["halfrdx"].SetValue(0.5f / Game1.TextureWidth);

            Color[] map = new Color[Game1.TextureHeight * Game1.TextureWidth];
            Velocity = new Texture2D(graf, Game1.TextureWidth, Game1.TextureHeight, 1, TextureUsage.None, SurfaceFormat.Color);
            Velocity.SetData<Color>(map);
            Vorticity = new Texture2D(graf, Game1.TextureWidth, Game1.TextureHeight, 1, TextureUsage.None, SurfaceFormat.Color);
            Vorticity.SetData<Color>(map);
            Divergence = new Texture2D(graf, Game1.TextureWidth, Game1.TextureHeight, 1, TextureUsage.None, SurfaceFormat.Color);
            Divergence.SetData<Color>(map);

            for (int i = 0; i < Game1.TextureHeight; i++)
            {
                for (int j = 0; j < Game1.TextureWidth; j++)
                {
                    map[i + (j * Game1.TextureWidth)] = new Color(128, 128, 128, 1);
                }
            }
            PressureOffsets = new Texture2D(graf, Game1.TextureWidth, Game1.TextureHeight, 1, TextureUsage.None, SurfaceFormat.Color);
            PressureOffsets.SetData<Color>(map);
            Density = new Texture2D(graf, Game1.TextureWidth, Game1.TextureHeight, 1, TextureUsage.None, SurfaceFormat.Color);
            Density.SetData<Color>(map);
        }

        public void Generate(SpriteBatch sp, Texture2D mercator, Texture2D velocity, Texture2D friction, Texture2D pressure)
        {
            if (!pass)
            {
                mover.Parameters["VelocityMap"].SetValue(velocity);
                Velocity = velocity;

            }
            else
            {
                mover.Parameters["VelocityMap"].SetValue(Velocity);
            }
            rts.BeginRenderToTexture();
            mover.CurrentTechnique = mover.Techniques["AdvectVelocity"];
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            mover.Begin();
            mover.CurrentTechnique.Passes[0].Begin();
            sp.Draw(Velocity, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            mover.CurrentTechnique.Passes[0].End();
            mover.End();
            Velocity = rts.EndRenderGetTexture();


            rts2.BeginRenderToTexture();
            mover.Parameters["VelocityMap"].SetValue(Velocity);
            mover.Parameters["DensityMap"].SetValue(Density);
            mover.CurrentTechnique = mover.Techniques["AdvectDensity"];
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            mover.Begin();
            mover.CurrentTechnique.Passes[0].Begin();
            sp.Draw(Density, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            mover.CurrentTechnique.Passes[0].End();
            mover.End();
            Density = rts2.EndRenderGetTexture();

            rts3.BeginRenderToTexture();
            mover.Parameters["VelocityMap"].SetValue(Velocity);
            mover.CurrentTechnique = mover.Techniques["Vorticity"];
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            mover.Begin();
            mover.CurrentTechnique.Passes[0].Begin();
            sp.Draw(Velocity, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            mover.CurrentTechnique.Passes[0].End();
            mover.End();
            Vorticity = rts3.EndRenderGetTexture();
            
            rts.BeginRenderToTexture();
            mover.Parameters["VelocityMap"].SetValue(Velocity);
            mover.Parameters["VorticityMap"].SetValue(Vorticity);
            mover.Parameters["dxscale"].SetValue(new Vector2(0.035f * Game1.TextureWidth, 0.035f * Game1.TextureHeight));
            mover.CurrentTechnique = mover.Techniques["ApplyVorticity"];
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            mover.Begin();
            mover.CurrentTechnique.Passes[0].Begin();
            sp.Draw(Velocity, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            mover.CurrentTechnique.Passes[0].End();
            mover.End();
            Velocity = rts.EndRenderGetTexture();

            rts4.BeginRenderToTexture();
            mover.Parameters["VelocityMap"].SetValue(Velocity);
            mover.CurrentTechnique = mover.Techniques["Divergence"];
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            mover.Begin();
            mover.CurrentTechnique.Passes[0].Begin();
            sp.Draw(Velocity, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            mover.CurrentTechnique.Passes[0].End();
            mover.End();
            Divergence = rts4.EndRenderGetTexture();

            for (int i = 0; i < 10; i++)
            {
                rts5.BeginRenderToTexture();
                mover.Parameters["PressureMap"].SetValue(PressureOffsets);
                mover.Parameters["DivergenceMap"].SetValue(Divergence);
                mover.CurrentTechnique = mover.Techniques["Jacobi"];
                sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
                mover.Begin();
                mover.CurrentTechnique.Passes[0].Begin();
                sp.Draw(Velocity, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
                sp.End();
                mover.CurrentTechnique.Passes[0].End();
                mover.End();
                PressureOffsets = rts5.EndRenderGetTexture();
            }
            rts.BeginRenderToTexture();
            mover.Parameters["PressureMap"].SetValue(PressureOffsets);
            mover.Parameters["VelocityMap"].SetValue(Velocity);
            mover.CurrentTechnique = mover.Techniques["Gradient"];
            sp.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            mover.Begin();
            mover.CurrentTechnique.Passes[0].Begin();
            sp.Draw(Velocity, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            sp.End();
            mover.CurrentTechnique.Passes[0].End();
            mover.End();
            Velocity = rts.EndRenderGetTexture();

            pass = true;
            texture = Divergence;
        }
    }
}
