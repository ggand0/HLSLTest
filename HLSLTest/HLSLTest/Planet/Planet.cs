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

		public int Seed;
		public PlanetType Type;
		public Texture2D PermTex;
		public Texture2D Mercator;
		public Texture2D Palette;
		public Effect terrain;
		public Effect draw;
		public RenderTargetState rts;

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
			using (Stream stream = File.OpenWrite("planet_map.png")) {
				Mercator.SaveAsPng(stream, Mercator.Width, Mercator.Height);
				stream.Position = 0;
			}
			//rts.DestroyBuffers();
			//rts = null;
		}
	}
}
