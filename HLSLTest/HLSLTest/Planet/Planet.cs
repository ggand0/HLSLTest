using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public enum PlanetType
	{
		Ice,
		Water,
		Rock,
		Gas,
		Molten,
	};
	public abstract class Planet
	{
		protected Model model;
		protected int Scale;

		public int Seed { get; private set; }
		public PlanetType Type { get; protected set; }
		public Texture2D PermTex { get; protected set; }
		public Texture2D Mercator { get; protected set; }
		public Texture2D Palette { get; protected set; }
		public Texture2D normalmap { get; protected set; }
		public Effect terrain { get; protected set; }
		public Effect draw { get; protected set; }
		public RenderTargetState rts { get; protected set; }
		public int Nz { get; protected set; }

		/// <summary>
		/// generate a random array of numbers, based on the planets seed,
		/// and store this in a texture that we pass into our shader.
		/// </summary>
		public void BuildPerm(GraphicsDevice graphicsDevice)
		{
			float[] map = new float[256];
			Random r = new Random(Seed);
			Vector4[] perm = new Vector4[256 * 256];
			int index = 0;
			for (int y = 0; y < 256; y++) {
				for (int i = 0; i < 256; i++) {
					Vector4 a = new Vector4();
					a.X = (float)((r.NextDouble()));
					a.Y = (float)((r.NextDouble()));
					a.Z = (float)((r.NextDouble()));
					a.W = 1;
					a.Normalize();
					perm[index] = a;
					index++;
				}
			}
			for (int i = 0; i < 256; i++) {
				map[i] = ((float)i) / 256.0f;
			}
			for (int i = 0; i < 256; i++) {
				int k = r.Next(255);
				float l = map[k];
				map[k] = map[i];
				map[i] = l;
			}
			for (int i = 0; i < 256; i++) {
				perm[i].W = map[i];
			}
			//PermTex = new Texture2D(graf, 256, 256, 1, TextureUsage.None, SurfaceFormat.Vector4);
			PermTex = new Texture2D(graphicsDevice, 256, 256, false, SurfaceFormat.Vector4);
			//PermTex = new Texture2D(graphicsDevice, 256, 256, false, SurfaceFormat.Single);
			PermTex.SetData(perm);
			using (Stream stream = File.OpenWrite("permtex.png")) {
				PermTex.SaveAsPng(stream, PermTex.Width, PermTex.Height);
				stream.Position = 0;
			}
		}
		public void Draw(Matrix View, Matrix World, Matrix Projection)
		{
			Matrix wvp = World * View * Projection;
			draw.Parameters["wvp"].SetValue(wvp);
			draw.Parameters["Palette"].SetValue(Palette);
			draw.Parameters["ColorMap"].SetValue(Mercator);
			draw.Parameters["BumpMap"].SetValue(normalmap);
			draw.Parameters["world"].SetValue(World);
			for (int pass = 0; pass < draw.CurrentTechnique.Passes.Count; pass++) {
				for (int msh = 0; msh < model.Meshes.Count; msh++) {
					ModelMesh mesh = model.Meshes[msh];
					for (int prt = 0; prt < mesh.MeshParts.Count; prt++)
						mesh.MeshParts[prt].Effect = draw;

					mesh.Draw();
				}
			}

		}


		private int safex(int x)
		{
			if (x >= 512)
				return x - 512;
			if (x < 0)
				return 512 + x;
			return x;
		}

		private int safey(int y)
		{
			if (y >= 512)
				return y - 512;
			if (y < 0)
				return 512 + y;
			return y;
		}
		public void Generate(GraphicsDevice graphics)
		{
			//rts = new RenderTargetState(graphics, 512, 512, 512, 512);// このクラス消して直接setRenderTargetした方が絶対わかりやすいよな...
			//rts.BeginRenderToTexture();
			RenderTarget2D RenderTarget = new RenderTarget2D(graphics, 512, 512, false, SurfaceFormat.Color, DepthFormat.Depth24);
			graphics.SetRenderTarget(RenderTarget);

			DepthStencilState ds = new DepthStencilState();
			ds.DepthBufferEnable = false;
			graphics.DepthStencilState = ds;
			graphics.RasterizerState = RasterizerState.CullNone;

			terrain.Parameters["ColorMap"].SetValue(PermTex);
			foreach (ModelMesh mesh in model.Meshes) {
				for (int i = 0; i < mesh.MeshParts.Count; i++) {
					// Set this MeshParts effect (currentEffect) to the
					// desired effect (from .fx file)  
					mesh.MeshParts[i].Effect = terrain;
				}
				mesh.Draw();
			}
			graphics.SetRenderTarget(null);
			Mercator = RenderTarget;
			//Mercator = (Texture2D)graphics.GetRenderTargets()[0].RenderTarget;
			//Mercator = rts.EndRenderGetTexture();


			// debug
			/*using (Stream stream = File.OpenWrite("planet_map.png")) {
				Mercator.SaveAsPng(stream, Mercator.Width, Mercator.Height);
				stream.Position = 0;
			}*/
			//rts.DestroyBuffers();
			//rts = null;

			// generate normals
			Color[] Map = new Color[512 * 512];
			Mercator.GetData<Color>(Map);

			//normalmap = new Texture2D(graphics, 512, 512, 1, TextureUsage.None, SurfaceFormat.Color);
			normalmap = new Texture2D(graphics, 512, 512, false, SurfaceFormat.Color);
			Color[] pixels = new Color[512 * 512];
			Color c3;
			for (int y = 0; y < 512; y++) {
				int offset = y * 512;
				for (int x = 0; x < 512; x++) {
					float h0 = (float)Map[x + (512 * y)].R;
					float h1 = (float)Map[x + (512 * safey(y + 1))].R;
					float h2 = (float)Map[safex(x + 1) + (512 * y)].R;

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

			normalmap.SetData(pixels);
		}
	}
}
