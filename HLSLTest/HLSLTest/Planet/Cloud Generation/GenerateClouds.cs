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
using System.Threading;

namespace HLSLTest
{
    public class GenerateClouds
    {
        public bool CloudsReady = false;

        Effect cloudRender;

        FrictionMap Friction;
        HeatMap Heatmap;
        PressureMap Pressure;
        InitialVelocity initialVelocity;
        public Flow flow;

        SpriteBatch sp;
        GraphicsDevice graphics;

        public Texture2D clouds;
        public Texture2D CloudBump;

        public GenerateClouds()
        {
            Friction = new FrictionMap();
            Heatmap = new HeatMap();
            Pressure = new PressureMap();
            initialVelocity = new InitialVelocity();
            flow = new Flow();
        }

        public void Load(GraphicsDevice graf, ContentManager cont)
        {
            cloudRender = cont.Load<Effect>("Shaders\\Clouds");
            sp = new SpriteBatch(graf);
            Friction.Load(graf, cont);
            Heatmap.Load(graf, cont);
            initialVelocity.Load(graf, cont);
            flow.Load(graf, cont);

            graphics = graf;

        }
       

        public void Generate()
        {
            CloudsReady = false;
            Friction.Generate(sp);
            Heatmap.Generate(sp);
            Pressure.Generate(graphics);
            initialVelocity.Generate(sp, Pressure.texture, Friction.texture);
            flow.pass = false;
            for (int i = 0; i < 100; i++)
            {
                flow.Generate(sp, Game1.mercator, initialVelocity.texture, Friction.texture, Pressure.texture);
            }
            clouds = flow.texture;
            CloudBump = BumpMap(flow.texture, 32);
                        
            CloudsReady = true;
        }

        public void DrawClouds(Matrix View, Matrix World, Matrix Projection, int type)
        {
            switch (type)
            {
                case 0:
                    cloudRender.Parameters["CloudColour"].SetValue(new Vector4(1,1,1,1));
                    break;
                case 1:
                    cloudRender.Parameters["CloudColour"].SetValue(new Vector4(0.59375f,0.99f,0.59375f,1));
                    break;
                case 2:
                    cloudRender.Parameters["CloudColour"].SetValue(new Vector4(1,0.4f,0.4f,1));
                    break;
                case 3:
                    cloudRender.Parameters["CloudColour"].SetValue(new Vector4(1,1,1,1));
                    break;
                case 4:
                    cloudRender.Parameters["CloudColour"].SetValue(new Vector4(0.6875f,0.766525f,0.857f,1));
                    break;
                case 5:
                    cloudRender.Parameters["CloudColour"].SetValue(new Vector4(0.2f,0.8f,0.2f,1));
                    break;
                case 6:
                    cloudRender.Parameters["CloudColour"].SetValue(new Vector4(1,0.9f,0.8f,1));
                    break;
                case 7:
                    cloudRender.Parameters["CloudColour"].SetValue(new Vector4(0.86f,0.44f,0.58f,1));
                    break;



            }

            Matrix wvp = World * View * Projection;
            cloudRender.Parameters["wvp"].SetValue(wvp);
            cloudRender.Parameters["world"].SetValue(World);
            cloudRender.Parameters["CloudMap"].SetValue(flow.texture);
            cloudRender.Parameters["CloudBumpMap"].SetValue(CloudBump);
            cloudRender.CurrentTechnique = cloudRender.Techniques["Bumped"];


            for (int pass = 0; pass < cloudRender.CurrentTechnique.Passes.Count; pass++)
            {
                for (int msh = 0; msh < Game1.sphere.Meshes.Count; msh++)
                {
                    ModelMesh mesh = Game1.sphere.Meshes[msh];

                    for (int prt = 0; prt < mesh.MeshParts.Count; prt++)
                        mesh.MeshParts[prt].Effect = cloudRender;
                    mesh.Draw();
                }
            }
        }
        private Texture2D BumpMap(Texture2D src, float Nz)
        {
            Texture2D normalmap;
            Color[] Map = new Color[512 * 512];
            src.GetData<Color>(Map);

            normalmap = new Texture2D(graphics, 512, 512, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] pixels = new Color[512 * 512];
            Color c3;


            for (int y = 0; y < 512; y++)
            {
                int offset = y * 512;
                for (int x = 0; x < 512; x++)
                {

                    float h0 = (float)Map[x + (512 * y)].B;
                    float h1 = (float)Map[x + (512 * safey(y + 1))].B;
                    float h2 = (float)Map[safex(x + 1) + (512 * y)].B;

                    float Nx = h0 - h2;
                    float Ny = h0 - h1;

                    Vector3 Normal = new Vector3((float)Nx, (float)Ny, (float)Nz);
                    Normal.Normalize();
                    Normal /= 2;

                    byte cr = (byte)(128 + (255 * Normal.X));
                    byte cg = (byte)(128 + (255 * Normal.Y));
                    byte cb = (byte)(128 + (255 * Normal.Z));
                    c3 = new Color(cr, cg, cb);
                    pixels[x + (y * 512)] = c3;
                }
            }

            normalmap.SetData<Color>(pixels);
            return normalmap;
        }
        int safex(int x)
        {
            if (x >= 512)
                return x - 512;
            if (x < 0)
                return 512 + x;
            return x;
        }

        int safey(int y)
        {
            if (y >= 512)
                return y - 512;
            if (y < 0)
                return 512 + y;
            return y;
        }
    }
}
