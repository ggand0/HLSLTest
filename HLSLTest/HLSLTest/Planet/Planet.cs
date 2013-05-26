using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

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
	public enum PlanetRenderType
	{
		MultiColored,
		MultiTextured,
		OneTexMultiColored
	}
	public abstract class Planet : Object
	{
		//public static Game1 game;
		//public static Level level;

		//protected Model model, sphere;
		protected Model sphere;
		//protected int Scale;

		public int Seed { get; private set; }
		public PlanetType Type { get; protected set; }
		public Texture2D PermTex { get; protected set; }
		public Texture2D Mercator { get; protected set; }
		public Texture2D Palette { get; protected set; }
		public Texture2D normalmap { get; protected set; }
		public Effect terrain { get; protected set; }
		public Effect draw { get; protected set; }
		/// <summary>
		/// このプロジェクトでは使う予定は無いが、チュートリアルで使うかもしれないので一応。
		/// </summary>
		public RenderTargetState rts { get; protected set; }
		/// <summary>
		/// 小さいほど凹凸が激しい
		/// </summary>
		public int Nz { get; protected set; }

		public int TextureWidth = 512;
		public int TextureHeight = 512;
		public int SubType { get; protected set; }


		protected GraphicsDevice graphicsDevice;
		public Effect atmosphere { get; protected set; }
		float p_radius = 200;
		float a_radius = 220;//205;
		float c_radius = 200.5f;
		public float roll;
		public float pitch;

		protected PlanetRenderType renderType;
		public Texture2D BlendMap { get; protected set; }
		protected Texture2D baseTexture, gTexture, bTexture;
		protected Texture2D baseNormalTexture;
		protected bool rotate;
		protected float rotationSpeed = MathHelper.ToRadians(1);
		protected bool revolution;
		protected float revolutionSpeed = MathHelper.ToRadians(1);
		protected static Vector3 DEF_POSITION = new Vector3(-300, 0, -200);
		protected static Vector3 DEF_STAR_POSITION = Vector3.Zero;
		//public Vector3 Position { get; protected set; }
		public Vector3 StarPosition { get; protected set; }
		//public bool IsActive { get; set; }
		public float revolutionAngle { get; set; }
		private BasicEffect basicEffect;
		private Effect simpleEffect;


		protected virtual void LoadContent(ContentManager content)
		{
			base.Load("Models\\sphere2");

			sphere = content.Load<Model>("Models\\sphere");
			atmosphere = content.Load<Effect>("Planets//SkyFromSpace");
			//Model = content.Load<Model>("Models\\sphere2");
			//GenerateTags();
			SetModelEffect(draw, false);
			BuildPerm(graphicsDevice);
		}

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

			// 色の決定
			SubType = r.Next(8);

			/*using (Stream stream = File.OpenWrite("permtex.png")) {
				PermTex.SaveAsPng(stream, PermTex.Width, PermTex.Height);
				stream.Position = 0;
			}*/
		}
		private int safex(int x)
		{
			if (x >= TextureWidth)
				return x - TextureWidth;
			if (x < 0)
				return TextureWidth + x;
			return x;
		}
		private int safey(int y)
		{
			if (y >= TextureHeight)
				return TextureHeight - 1;
			if (y < 0)
				return 0;
			return y;
		}
		public void Generate(GraphicsDevice graphics)
		{
			//rts = new RenderTargetState(graphics, TextureWidth, TextureHeight, TextureWidth, TextureHeight);// このクラス消して直接setRenderTargetした方が絶対わかりやすいよな...
			//rts.BeginRenderToTexture();
			RenderTarget2D RenderTarget = new RenderTarget2D(graphics, TextureWidth, TextureHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
			graphics.SetRenderTarget(RenderTarget);

			DepthStencilState ds = new DepthStencilState();
			ds.DepthBufferEnable = false;
			graphics.DepthStencilState = ds;
			graphics.RasterizerState = RasterizerState.CullNone;

			terrain.Parameters["ColorMap"].SetValue(PermTex);
			foreach (ModelMesh mesh in Model.Meshes) {
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
			Color[] Map = new Color[TextureHeight * TextureWidth];
			Mercator.GetData<Color>(Map);

			//normalmap = new Texture2D(graphics, 512, 512, 1, TextureUsage.None, SurfaceFormat.Color);
			normalmap = new Texture2D(graphics, TextureWidth, TextureHeight, false, SurfaceFormat.Color);
			Color[] pixels = new Color[TextureHeight * TextureWidth];
			Color c3;

			#region Fix the crenalations at the top and bottom of the heightmap
			// Fix the heightmap at the poles.
			int max_x = (TextureHeight - 1) * TextureWidth;
			for (int x = 0; x < TextureWidth; x++) {
				Map[x + max_x - TextureWidth] = Map[x + max_x - (TextureWidth * 2)];
				Map[x + max_x] = Map[x + max_x - TextureWidth];
				Map[x + TextureWidth] = Map[x + (TextureWidth * 2)];
				Map[x] = Map[x + TextureWidth];

			}
			//max_x -= TextureWidth;
			for (int x = 0; x < TextureWidth; x += 16) {
				for (int y = 0; y < 32; y++) {
					int i = y / 2;
					for (int j = i + 1; j <= 16; j++) {
						Map[x + j + (y * TextureWidth)] = Map[x + i + (y * TextureWidth)];
						int mx = x + j + max_x - (y * TextureWidth);
						if (mx < TextureWidth * TextureHeight)
							Map[mx] = Map[x + i + max_x - (y * TextureWidth)];
					}
				}
			}
			Mercator.SetData<Color>(Map);
			#endregion



			// added : make blend maps
			// とりあえずテクスチャ2パターンで。→3パターンにした
			if (renderType == PlanetRenderType.MultiTextured) {
				BlendMap = new Texture2D(graphics, TextureWidth, TextureHeight, false, SurfaceFormat.Color);
				Color[] blend = new Color[TextureHeight * TextureWidth];
				float maxHeight = 1;
				for (int y = 0; y < TextureHeight; y++) {
					for (int x = 0; x < TextureWidth; x++) {
						// get height of that pixel
						// Get color value (0 - 255)
						float amt = Map[y * TextureWidth + x].R;
						// Scale to (0 - 1)
						amt /= 255.0f;
						// Multiply by max height to get final height
						float height = amt * maxHeight;// 本当はamt * maxHeightだが、区別出来ればいいだけなので必要なし

						// determine Base texture or not
						if (height <= 0.1f) blend[y * TextureWidth + x] = Color.Red;
						else if (height <= 0.5f) blend[y * TextureWidth + x] = Color.Green;
						else blend[y * TextureWidth + x] = Color.Blue;

					}
				}
				BlendMap.SetData<Color>(blend);
			} else if (renderType == PlanetRenderType.OneTexMultiColored) {
				BlendMap = new Texture2D(graphics, TextureWidth, TextureHeight, false, SurfaceFormat.Color);
				Color[] blend = new Color[TextureHeight * TextureWidth];
				float maxHeight = 1;
				for (int y = 0; y < TextureHeight; y++) {
					for (int x = 0; x < TextureWidth; x++) {
						// get height of that pixel
						// Get color value (0 - 255)
						float amt = Map[y * TextureWidth + x].R;
						// Scale to (0 - 1)
						amt /= 255.0f;
						// Multiply by max height to get final height
						float height = amt * maxHeight;// 本当はamt * maxHeightだが、区別出来ればいいだけなので必要なし

						// determine Base texture or not
						if (height <= 0.1f) blend[y * TextureWidth + x] = Color.Red;// 海の部分だけテクスチャにする
						else blend[y * TextureWidth + x] = Color.Green;
					}
				}
				BlendMap.SetData<Color>(blend);
			}

			if (renderType != PlanetRenderType.MultiColored)
				using (Stream stream = File.OpenWrite("blendMap.png")) {
					BlendMap.SaveAsPng(stream, BlendMap.Width, BlendMap.Height);
					stream.Position = 0;
				}


			for (int y = 0; y < TextureHeight; y++) {
				int offset = y * TextureWidth;
				for (int x = 0; x < TextureWidth; x++) {
					float h0 = (float)Map[x + (TextureWidth * y)].R;
					float h1 = (float)Map[x + (TextureWidth * safey(y + 1))].R;
					float h2 = (float)Map[safex(x + 1) + (TextureWidth * y)].R;

					float Nx = h0 - h2;
					float Ny = h0 - h1;

					Vector3 Normal = new Vector3((float)Nx, (float)Ny, (float)Nz);
					Normal.Normalize();
					Normal /= 2;

					byte cr = (byte)(128 + (255 * Normal.X));
					byte cg = (byte)(128 + (255 * Normal.Y));
					byte cb = (byte)(128 + (255 * Normal.Z));
					c3 = new Color(cr, cg, cb);
					pixels[x + (y * TextureWidth)] = c3;
				}
			}

			normalmap.SetData(pixels);
		}
		protected virtual void SetAtmosphereEffectParametersDetail()
		{
			atmosphere.Parameters["fKrESun"].SetValue(0.00025f * 10);
			atmosphere.Parameters["fKmESun"].SetValue(0.0015f * 10);
			atmosphere.Parameters["v3InvWavelength"].SetValue(new Vector3(1.0f / (float)Math.Pow(0.650f, 4), 1.0f / (float)Math.Pow(0.570f, 4), 1.0f / (float)Math.Pow(0.475f, 4)));
		}
		protected virtual void SetAtmosphereEffectParameters(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			float scale = 1.0f / (a_radius - p_radius);

			atmosphere.Parameters["fOuterRadius"].SetValue(a_radius);
			atmosphere.Parameters["fInnerRadius"].SetValue(p_radius);
			atmosphere.Parameters["fOuterRadius2"].SetValue(a_radius * a_radius);
			atmosphere.Parameters["fInnerRadius2"].SetValue(p_radius * p_radius);
			atmosphere.Parameters["fKr4PI"].SetValue(0.0025f * 4 * MathHelper.Pi);
			atmosphere.Parameters["fKm4PI"].SetValue(0.0015f * 4 * MathHelper.Pi);
			atmosphere.Parameters["fScale"].SetValue(scale);
			atmosphere.Parameters["fScaleDepth"].SetValue(0.25f);
			atmosphere.Parameters["fScaleOverScaleDepth"].SetValue(scale / 0.25f);
			atmosphere.Parameters["fSamples"].SetValue(2.0f);
			atmosphere.Parameters["nSamples"].SetValue(2);


			// ここを惑星ごとに変えるべし
			SetAtmosphereEffectParametersDetail();

			Matrix World = Matrix.CreateScale(a_radius) * Matrix.CreateRotationX(pitch)
			* Matrix.CreateTranslation(Position);
			World.Translation = Position;
			//Vector3 vl = -level.LightPosition;
			Vector3 vl = -level.LightPosition;
			vl.Normalize();

			atmosphere.Parameters["World"].SetValue(World);
			World = Matrix.CreateScale(a_radius) * Matrix.CreateRotationX(pitch);
			atmosphere.Parameters["DefWorld"].SetValue(World);

			atmosphere.Parameters["View"].SetValue(View);
			atmosphere.Parameters["Projection"].SetValue(Projection);
			atmosphere.Parameters["v3CameraPos"].SetValue(CameraPosition);

			atmosphere.Parameters["v3LightDir"].SetValue(vl);
			atmosphere.Parameters["v3LightPos"].SetValue(level.LightPosition);
			atmosphere.Parameters["fCameraHeight"].SetValue(CameraPosition.Length());
			atmosphere.Parameters["fCameraHeight2"].SetValue(CameraPosition.LengthSquared());
		}


		public override void SetModelEffect(Effect effect, bool CopyEffect)
		{
			foreach (ModelMesh mesh in Model.Meshes)
				foreach (ModelMeshPart part in mesh.MeshParts) {
					Effect toSet = effect;
					// Copy the effect if necessary
					if (CopyEffect)
						toSet = effect.Clone();
					MeshTag tag = ((MeshTag)part.Tag);

					if (DrawingDepthNormalPass) {
						SetEffectParameter(toSet, "BumpMap", normalmap);// hennkou
					}
					SetEffectParameter(toSet, "BasicTexture", Mercator);// hennkou
					SetEffectParameter(toSet, "TextureEnabled", true);

					// Set our remaining parameters to the effect
					SetEffectParameter(toSet, "DiffuseColor", tag.Color);
					SetEffectParameter(toSet, "SpecularPower", tag.SpecularPower);
					part.Effect = toSet;
				}
		}
		public override void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{

			/*//Matrix wvp = World * View * Projection;
			Matrix World = Matrix.CreateScale(p_radius) * Matrix.CreateRotationY(roll) * Matrix.CreateRotationX(pitch);
				//* Matrix.CreateTranslation(Position);
			Matrix wvp = World * View * Projection;
			Vector3 light = -level.LightPosition;
			light.Normalize();

			draw.Parameters["wvp"].SetValue(wvp);
			// wvp以外は毎フレーム設定する必要ないよな
			draw.Parameters["Palette"].SetValue(Palette);
			draw.Parameters["ColorMap"].SetValue(Mercator);
			draw.Parameters["BumpMap"].SetValue(normalmap);
			draw.Parameters["world"].SetValue(World);
			draw.Parameters["subtype"].SetValue(SubType / 8.0f);*/
			Matrix World = Matrix.CreateScale(p_radius) * Matrix.CreateRotationY(roll) * Matrix.CreateRotationX(pitch)
				* Matrix.CreateTranslation(Position);
			_world = World;
			Matrix wvp = World * View * Projection;
			//Vector3 light = -level.LightPosition;
			Vector3 light = level.LightPosition;
			light.Normalize();
			draw.Parameters["LightDirection"].SetValue(light);
			draw.Parameters["wvp"].SetValue(wvp);
			draw.Parameters["world"].SetValue(World);
			draw.Parameters["subtype"].SetValue(SubType / 8.0f);


			draw.Parameters["Palette"].SetValue(Palette);
			draw.Parameters["ColorMap"].SetValue(Mercator);
			draw.Parameters["BumpMap"].SetValue(normalmap);

			if (renderType == PlanetRenderType.MultiColored) {
				draw.Parameters["renderType"].SetValue(0);

			} else if (renderType == PlanetRenderType.MultiTextured) {
				draw.Parameters["renderType"].SetValue(1);

				draw.Parameters["BaseTexture"].SetValue(baseTexture);
				draw.Parameters["GTexture"].SetValue(gTexture);
				draw.Parameters["BTexture"].SetValue(bTexture);
				draw.Parameters["WeightMap"].SetValue(BlendMap);
			} else if (renderType == PlanetRenderType.OneTexMultiColored) {
				draw.Parameters["renderType"].SetValue(2);

				draw.Parameters["BaseTexture"].SetValue(baseTexture);
				draw.Parameters["BaseNormalTexture"].SetValue(baseNormalTexture);
				draw.Parameters["WeightMap"].SetValue(BlendMap);
			}

			if (DrawingPrePass) {
				base.Draw(View, Projection, CameraPosition);
			} else {
				for (int pass = 0; pass < draw.CurrentTechnique.Passes.Count; pass++) {
					for (int msh = 0; msh < Model.Meshes.Count; msh++) {
						ModelMesh mesh = Model.Meshes[msh];
						for (int prt = 0; prt < mesh.MeshParts.Count; prt++) {
							/*if (DrawingPreShadowPass) {
								Effect effect = mesh.Effect;

								SetEffectParameter(effect, "World", World);
								SetEffectParameter(effect, "View", View);
								SetEffectParameter(effect, "Projection", Projection);
								SetEffectParameter(effect, "CameraPosition", CameraPosition);
								mesh.MeshParts[prt].Effect = effect;
							} else {
								mesh.MeshParts[prt].Effect = draw;
							}*/
							mesh.MeshParts[prt].Effect = draw;
						}

						mesh.Draw();
					}
				}/**/
			}

			// atmosphere scattering setteings
			SetAtmosphereEffectParameters(View, Projection, CameraPosition);


			// Draw
			if (!DrawingPrePass) {
				graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
				/*DepthStencilState ds = new DepthStencilState();
				ds.DepthBufferEnable = false;
				graphics.DepthStencilState = ds;*/
				foreach (ModelMesh mesh in sphere.Meshes) {
					for (int i = 0; i < mesh.MeshParts.Count; i++) {
						// Set this MeshParts effect (currentEffect) to the desired effect (from .fx file)   
						mesh.MeshParts[i].Effect = atmosphere;
					}
					mesh.Draw();
				}
			}
			//graphics.RenderState.CullMode = CullMode.None;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;

		}
		public override  void Update(GameTime gameTime)
		{
			if (rotate) {
				roll += rotationSpeed;
			}
			if (revolution) {
				revolutionSpeed = 0.2f;
				revolutionAngle += MathHelper.ToRadians(revolutionSpeed);
				Vector3 velocity = new Vector3((float)Math.Cos(revolutionAngle), 0, (float)Math.Sin(revolutionAngle));
				//Vector3 tmp = StarPosition + velocity * 3000;

				float radius = (Position - StarPosition).Length();
				Vector3 tmp = StarPosition + velocity * radius;

				Position = tmp;
			}
		}
		#region Constructors
		public Planet(GraphicsDevice graphics, ContentManager content)
			: this(DEF_STAR_POSITION, graphics, content)
		{
		}
		public Planet(Vector3 starPosition, GraphicsDevice graphics, ContentManager content)
			: this(DEF_POSITION, starPosition, graphics, content)
		{
		}
		public Planet(Vector3 position, Vector3 starPosition, GraphicsDevice graphics, ContentManager content)
			: base(position) 
		{
			this.graphicsDevice = graphics;
			this.StarPosition = starPosition;
			Position = position;
			
			
			LoadContent(content);
			// shadwo pre-draw用
			basicEffect = new BasicEffect(graphics);
			simpleEffect = content.Load<Effect>("Lights\\SimpleEffect");
			SetEffectParameter(simpleEffect, "BasicTexture", Mercator);

			IsActive = true;
		}
		#endregion
	}
}
