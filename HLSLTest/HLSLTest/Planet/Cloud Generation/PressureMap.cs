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
    public class PressureMap
    {
        public Texture2D texture;

        List<Vector2> HighPressureRegions = new List<Vector2>();
        GraphicsDevice graphics;
        Random rand = new Random();

        public void Generate(GraphicsDevice graf)
        {
            
            HighPressureRegions.Clear();
            for (int i = 0; i < Game1.PlanetWeatherIntensity; i++)
            {
                HighPressureRegions.Add(new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()));
            }
            graphics = graf;
            build();
        }

        

        private void build()
        {
            float[] map = new float[Game1.TextureHeight * Game1.TextureWidth];
            int pos = 0;
            float max = 0;
            for (int y = 0; y < Game1.TextureHeight; y++)
            {
                for (int x = 0; x < Game1.TextureWidth; x++)
                {
                    float z = 0;
                    Vector2 ppos = new Vector2((float)x / (float)Game1.TextureWidth, (float)y / (float)Game1.TextureHeight);
                    for (int i = 0; i < Game1.PlanetWeatherIntensity; i++)
                    {
                        float dx = Math.Abs(ppos.X - HighPressureRegions[i].X);
                        if (dx > 0.5)
                            dx = 1 - dx;
                        float dy = Math.Abs(ppos.Y - HighPressureRegions[i].Y);
                        if (dy > 0.5)
                            dy = 1 - dy;
                        float d = (dx * dx) + (dy * dy);
                        if (d < 0.0001f)
                            d = 0.0001f;
                        z += 1.0f / d;
                    }
                    if (z > max) max = z;
                    map[pos] = z;// new Vector4(0, 0, z, 1);
                    pos++;
                }
            }
            pos = 0;

            for (int y = 0; y < Game1.TextureHeight; y++)
            {
                for (int x = 0; x < Game1.TextureWidth; x++)
                {
                    map[pos++] /= max;
                }
            }
            if (texture==null)
                texture = new Texture2D(graphics, Game1.TextureWidth, Game1.TextureHeight, 1, TextureUsage.None, SurfaceFormat.Single);
            texture.SetData<float>(map);
        }
    }
}
