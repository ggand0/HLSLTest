//#define DEBUG_MODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace HLSLTest
{
	/// <summary>
	/// 基底 Game クラスから派生した、ゲームのメイン クラスです。
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		public Object Target { get; private set; }
		public Object Ground { get; private set; }
		public Object Teapot { get; private set; }
		public ArcBallCamera camera { get; private set; }
		Debug debug;
		PrelightingRenderer renderer;
		public List<Object> Models { get; private set; }
		public SkySphere Sky { get; private set; }
		Water water;
		BillboardSystem trees;
		BillboardSystem clouds;
		Random r = new Random();

		FlameParticleEmitter ps;
		ExplosionParticleEmitter eps;
		DiscoidParticleEmitter discoid;
		ParticleEmitter basicEmitter, beamEmitter;

		BillboardSystem lbs;
		LaserBillboard lb;
		Model s, e;
		Vector3 start = new Vector3(200, 50, 0), end = new Vector3(-50, -50, 0);
		BillboardCross treesCross;

		DiscoidEffect discoidEffect;
		BillboardSystem softParticle;
        GlassEffect glassEffect;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// ゲームの開始前に実行する必要がある初期化を実行できるようにします。
		/// ここで、要求されたサービスを問い合わせて、非グラフィック関連のコンテンツを読み込むことができます。
		/// base.Initialize を呼び出すと、任意のコンポーネントが列挙され、
		/// 初期化もされます。
		/// </summary>
		protected override void Initialize()
		{
			// TODO: ここに初期化ロジックを追加します。
			Object.game = this;
			Object.content = Content;

			Target = new Object(new Vector3(0, 20, 0), "Models\\cube");
			Target.Scale = 20;
			//Target = new Object(new Vector3(0, 0, 0), "Models\\tank");
			//Target.Scale = 0.1f;
			Ground = new Object(new Vector3(0, -50, 0), "Models\\ground");
			Teapot = new Object(new Vector3(-100, 0, 0), "Models\\UtahTeapotDef");
			
			Target.Direction = Vector3.UnitX;
			Ground.Scale = 0.05f;
			Teapot.Scale = 10;
			//Teapot.RotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-180));
			Teapot.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
				* Matrix.CreateRotationX(MathHelper.ToRadians(-90));
			Models = new List<Object>();
			Models.Add(Ground);
			Models.Add(Target);
			Models.Add(Teapot);

			camera = new ArcBallCamera();
			camera.Initialize(this, Target);
			ParticleEmitter.camera = camera;



			// Generate random tree positions
			Random r = new Random();
			Vector3[] positions = new Vector3[100];
			for (int i = 0; i < positions.Length; i++) {
				//positions[i] = new Vector3((float)r.NextDouble() * 20000 - 10000, 400, (float)r.NextDouble() * 20000 - 10000);
				//positions[i] = new Vector3((float)r.NextDouble() * 200 - 100, 256, (float)r.NextDouble() * 200 - 100);
				positions[i] = new Vector3((float)r.NextDouble() * 200 - 100, 10, (float)r.NextDouble() * 200 - 100);
			}
			//trees = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("tree"), new Vector2(800), positions);
			trees = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("tree"), new Vector2(10), positions);

			// Generate clouds
			Vector3[] cloudPositions = new Vector3[350];
			for (int i = 0; i < cloudPositions.Length; i++) {
				cloudPositions[i] = new Vector3(
					//r.Next(-6000, 6000), r.Next(2000, 3000), r.Next(-6000, 6000));
					r.Next(-6000, 6000), r.Next(2000, 3000), r.Next(-6000, 6000));
			}
			clouds = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\cloud"), new Vector2(500), cloudPositions);
			clouds.EnsureOcclusion = false;


			// Generate particles
			ps = new FlameParticleEmitter(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\fire"), Vector3.Zero, 1000, new Vector2(10), 10, Vector3.Zero, 0.01f);// 0.1f
			/*eps = new ExplosionParticleEmitter(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\explosion"), Vector3.Zero, 2000, new Vector2(50), 20, 5f);
			discoid = new DiscoidParticleEmitter(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\sun_1"), Vector3.Zero, 10000, new Vector2(5), 20, 5f);*/
			eps = new ExplosionParticleEmitter(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\nova_2"), Vector3.Zero, 2000, new Vector2(10), 20, 5f);
			discoid = new DiscoidParticleEmitter(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\nova_2"), Vector3.Zero, 10000, new Vector2(5), 20, 5f);
			basicEmitter = new ParticleEmitter(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\Mercury\\Star"), new Vector3(0, 50, 0), 100, new Vector2(3), 3, 0.1f);
			beamEmitter = new ParticleEmitter(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\Mercury\\Beam"), new Vector3(0, 50, 0), 100, new Vector2(10), 3, 0.1f);
			softParticle = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\nova_2"), 1, Models, new Vector2(100), new Vector3[] { new Vector3(0, 30, 0), new Vector3(-100, 0, 0) });

			// Generate lasers
			lbs = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\Laser"), new Vector2(10, 1000), new Vector3[] { Vector3.Zero });
			/*lb = new LaserBillboard(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\Laser2"), new Vector2(300, 3),
				new Vector3(50, 50, 0), new Vector3(-50, -50, 0), new Vector3[] { Vector3.Zero });*/
			lb = new LaserBillboard(GraphicsDevice, Content, Content.Load<Texture2D>("Textures\\Laser2"), new Vector2(300, 3), start, end);/**/
			s = Content.Load<Model>("Models\\Ship"); e = Content.Load<Model>("Models\\Ship");


			treesCross = new BillboardCross(GraphicsDevice, Content, Content.Load<Texture2D>("tree"), new Vector2(10), positions);

			// discoid effect
			DiscoidEffect.game = this;
			discoidEffect = new DiscoidEffect(Content, GraphicsDevice, new Vector3(0, 50, 0), new Vector2(100));

			base.Initialize();
		}

		/// <summary>
		/// LoadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
		/// 読み込みます。
		/// </summary>
		protected override void LoadContent()
		{
			// 新規の SpriteBatch を作成します。これはテクスチャーの描画に使用できます。
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Debug.game = this;
			Debug.spriteBatch = this.spriteBatch;
			PrelightingRenderer._gameInstance = this;
			debug = new Debug();

			// skymap reflect
			Effect cubeMapEffect = Content.Load<Effect>("CubeMapReflect");
			CubeMapReflectMaterial cubeMat = new CubeMapReflectMaterial(Content.Load<TextureCube>("SkyBoxTex"));
			Teapot.SetModelEffect(cubeMapEffect, false);
			Teapot.Material = cubeMat;/**/
			//models[2].SetModelEffect(cubeMapEffect, false);
			//models[2].Material = cubeMat;

			// projection
			/*Effect effect = Content.Load<Effect>("TextureProjectionEffect");
			models[0].SetModelEffect(effect, true);
			models[1].SetModelEffect(effect, true);
			//t = Content.Load<Texture2D>("Checker");//("projectedTexture");
			t = Content.Load<Texture2D>("projectedTexture");
			ProjectedTextureMaterial mat = new ProjectedTextureMaterial(Content.Load<Texture2D>("projectedTexture")
				, GraphicsDevice);
			mat.ProjectorPosition = new Vector3(0, 100, 0);
			mat.ProjectorTarget = new Vector3(0, 0, 0);
			mat.Scale = 0.05f;//2
			models[0].Material = mat;
			models[1].Material = mat;*/

			// light map
			Effect shadowEffect = Content.Load<Effect>("ProjectShadowDepthEffectV4");
			Effect lightingEffect = Content.Load<Effect>("PPModel");	// load Prelighting Effect
			//models[0].SetModelEffect(lightingEffect, true);			// set effect to each modelmeshpart
			//models[1].SetModelEffect(lightingEffect, true);
			Models[0].SetModelEffect(shadowEffect, true);				// set effect to each modelmeshpart
			Models[1].SetModelEffect(shadowEffect, true);
			Ground.SetModelEffect(shadowEffect, true);
			

			renderer = new PrelightingRenderer(GraphicsDevice, Content);
			renderer.Models = Models;
			renderer.Camera = camera;
			renderer.Lights = new List<PPPointLight>() {
				/*new PPPointLight(new Vector3(-100, 100, 0), Color.Red * .85f,
				200),
				new PPPointLight(new Vector3(100, 100, 0), Color.Blue * .85f,
				200),
				new PPPointLight(new Vector3(0, 100, 100), Color.Green * .85f,
				200),*/
				new PPPointLight(new Vector3(0, 200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000),
				new PPPointLight(new Vector3(0, -200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000)/**/
			};
			// setup shadows
			renderer.ShadowLightPosition = new Vector3(500, 500, 0);//new Vector3(1500, 1500, 2000);
			renderer.ShadowLightTarget = new Vector3(0, 300, 0);//new Vector3(0, 150, 0)
			//renderer.ShadowLightPosition = new Vector3(200, 100, 0);//new Vector3(1500, 1500, 2000);
			//renderer.ShadowLightTarget = new Vector3(-50, -50, 0);//new Vector3(0, 150, 0)
			//renderer.ShadowLightPosition = new Vector3(0, 150, 0);//new Vector3(1500, 1500, 2000);
			//renderer.ShadowLightTarget = new Vector3(0, 0, 0);//new Vector3(0, 150, 0)

			renderer.DoShadowMapping = true;
			renderer.ShadowMult = 0.3f;//0.01f;//0.3f;


			//sky = new SkySphere(Content, GraphicsDevice, Content.Load<TextureCube>("OutputCube0"));//("OutputCube0"));
			//sky = new SkySphere(Content, GraphicsDevice, Content.Load<TextureCube>("Cross"));//("OutputCube0"));
			Sky = new SkySphere(Content, GraphicsDevice, Content.Load<TextureCube>("SkyBoxTex"));

			Water.game = this;
			water = new Water(Content, GraphicsDevice, new Vector3(0, 0, 0), new Vector2(1000, 1000));
			//water = new Water(Content, GraphicsDevice, new Vector3(0, 0, 0), new Vector2(1000, 1000), renderer);
			water.Objects.Add(Sky);
			//water.Objects.Add(models[0]); water.Objects.Add(models[1]);
			foreach (Object o in Models) {
				water.Objects.Add(o);
			}
			water.Initialize();

            GlassEffect.game = this;
            glassEffect = new GlassEffect(Content, GraphicsDevice, new Vector3(0, 100, 0), 50);
            glassEffect.Objects.Add(Sky);
            foreach (Object o in Models) {
                glassEffect.Objects.Add(o);
            }
            glassEffect.Initialize();

            // 静的なオブジェクトを全て含めた環境マップ生成
            PreDrawScene(new GameTime());
            EnvironmentalMap = RenderCubeMap(new Vector3(0, 100, 0));
		}
        bool initialized;

		/// <summary>
		/// UnloadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
		/// アンロードします。
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: ここで ContentManager 以外のすべてのコンテンツをアンロードします。
		}

		/// <summary>
		/// ワールドの更新、衝突判定、入力値の取得、オーディオの再生などの
		/// ゲーム ロジックを、実行します。
		/// </summary>
		/// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
		protected override void Update(GameTime gameTime)
		{
			// ゲームの終了条件をチェックします。
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			JoyStick.Update(1);
            KeyInput.Update();
			Sky.Update(gameTime);

			foreach (Object o in Models) {
				o.Update(gameTime);
			}

			renderer.Update();
			camera.UpdateChaseTarget(Target);
			camera.Update(gameTime);
			water.Update();
			//Ground.Update(gameTime);


			// update particles
			ps.Update();
			discoid.Update();
			eps.Update();
			basicEmitter.Update();
			beamEmitter.Update();

			lb.Update(camera.Up, camera.Right, camera.CameraPosition);

			discoidEffect.Update(gameTime);

			base.Update(gameTime);
		}
		

		/// <summary>
		/// GraphicsDeviceのStateをデフォルトの状態に戻す。
		/// spriteBatchがDeviceにした変更を戻すのに使う
		/// </summary>
		public void ResetGraphicDevice()
		{
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		}
		/// <summary>
		/// 単純なモデル描画メソッド。ここで興味深い部分は、
		/// ビュー行列と射影行列がカメラ オブジェクトから取得されることです。
		/// </summary>        
		private void DrawModel(Model model, Matrix world)
		{
			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh mesh in model.Meshes) {
				foreach (BasicEffect effect in mesh.Effects) {
					effect.EnableDefaultLighting();
					effect.World = transforms[mesh.ParentBone.Index] * world;

					// 追尾カメラによって提供される行列を使用します
					effect.View = camera.View;
					effect.Projection = camera.Projection;
				}
				mesh.Draw();
			}
		}
		public TextureCube EnvironmentalMap { get; private set; }
		private TextureCube RenderCubeMap(Vector3 position)
		{
			TextureCube debug;
			//RenderTargetCube RefCubeMap = new RenderTargetCube(this.GraphicsDevice, 256, 1, SurfaceFormat.Color);
			RenderTargetCube RefCubeMap = new RenderTargetCube(this.GraphicsDevice, GraphicsDevice.Viewport.Width, true, SurfaceFormat.Color, DepthFormat.Depth24);
			Matrix viewMatrix = Matrix.Identity;
			TargetCamera camera;

			// Render our cube map, once for each cube face( 6 times ).
			for (int i = 0; i < 6; i++) {
				// render the scene to all cubemap faces
				CubeMapFace cubeMapFace = (CubeMapFace)i;

				switch (cubeMapFace) {
					case CubeMapFace.NegativeX: {
							//Vector3 target = new Vector3
							viewMatrix = Matrix.CreateLookAt(position, position + Vector3.Left, Vector3.Up);
							break;
						}
					case CubeMapFace.NegativeY: {
                        viewMatrix = Matrix.CreateLookAt(position, position + Vector3.Down, Vector3.Forward);
							break;
						}
					case CubeMapFace.NegativeZ: {
                        viewMatrix = Matrix.CreateLookAt(position, position + Vector3.Backward, Vector3.Up);
							break;
						}
					case CubeMapFace.PositiveX: {
                        viewMatrix = Matrix.CreateLookAt(position, position + Vector3.Right, Vector3.Up);
							break;
						}
					case CubeMapFace.PositiveY: {
                        viewMatrix = Matrix.CreateLookAt(position, position + Vector3.Up, Vector3.Backward);
							break;
						}
					case CubeMapFace.PositiveZ: {
                        viewMatrix = Matrix.CreateLookAt(position, position + Vector3.Forward, Vector3.Up);
							break;
						}
				}

				//effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
				camera = new TargetCamera(position, Vector3.Zero, GraphicsDevice);
				camera.View = viewMatrix; camera.Projection = this.camera.Projection;


				// Set the cubemap render target, using the selected face
				//this.GraphicsDevice.SetRenderTarget(RefCubeMap, cubeMapFace);
				this.GraphicsDevice.SetRenderTarget(RefCubeMap, cubeMapFace);
				this.GraphicsDevice.Clear(Color.White);
				this.DrawScene(camera);
				graphics.GraphicsDevice.SetRenderTarget(null);
			}

			graphics.GraphicsDevice.SetRenderTarget(null);
			if (!hasSaved) {
				debug = RefCubeMap;// null!?
				DDSLib.DDSToFile("cubeMapFace_debug.dds", true, debug, false); 
			}


			return RefCubeMap;
		}
		/// <summary>
		/// Draw terrain + objects
		/// </summary>
		private void DrawScene(TargetCamera camera)
		{
			Sky.Draw(camera.View, camera.Projection, camera.Position);
			water.Draw(camera.View, camera.Projection, camera.Position);

			//Ground.Model.Draw(Ground.World, camera.View, camera.Projection);
			foreach (Object o in Models) {
				//if (camera.BoundingVolumeIsInView(model.BoundingSphere)) {
				//string s = o.Scale.ToString();
				o.Draw(camera.View, camera.Projection, camera.Position);
			}
		}
        private void PreDrawScene(GameTime gameTime)
        {
            softParticle.DrawDepth(camera.View, camera.Projection, camera.CameraPosition);
            water.PreDraw(camera, new GameTime());// renderer.Drawとの順番に注意　前に行わないとrendererのパラメータを汚してしまう?
            glassEffect.PreDraw(camera, gameTime);
            renderer.Draw();
        }

		bool hasSaved;
		/// <summary>
		/// ゲームが自身を描画するためのメソッドです。
		/// </summary>
		/// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
		protected override void Draw(GameTime gameTime)
		{
#if DEBUG_MODE			
			GraphicsDevice.Clear(Color.CornflowerBlue);
			renderer.Draw();
			
#else
			string belndState = GraphicsDevice.BlendState.ToString();
			string depthState = GraphicsDevice.DepthStencilState.ToString();
			string rasterizerState = GraphicsDevice.RasterizerState.ToString();
			softParticle.DrawDepth(camera.View, camera.Projection, camera.CameraPosition);
			water.PreDraw(camera, gameTime);// renderer.Drawとの順番に注意　前に行わないとrendererのパラメータを汚してしまう?
            //glassEffect.PreDraw(camera, gameTime);
			renderer.Draw();
			//EnvironmentalMap = RenderCubeMap();// 動的環境マップ生成: 6回シーンを描画するので滅茶苦茶重い
			GraphicsDevice.Clear(Color.Black);

			Sky.Draw(camera.View, camera.Projection, camera.CameraPosition);
			water.Draw(camera.View, camera.Projection, camera.CameraPosition);

			belndState = GraphicsDevice.BlendState.ToString();
			depthState = GraphicsDevice.DepthStencilState.ToString();
			rasterizerState = GraphicsDevice.RasterizerState.ToString();


			//Ground.Model.Draw(Ground.World, camera.View, camera.Projection);
			foreach (Object o in Models) {
				//if (camera.BoundingVolumeIsInView(model.BoundingSphere)) {
				o.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}

			//trees.Draw(camera.View, camera.Projection, camera.Up, camera.Right);
			//treesCross.Draw(camera.View, camera.Projection);
			//clouds.Draw(camera.View, camera.Projection, camera.Up, camera.Right);

			// particles
			//ps.Draw(camera.View, camera.Projection, camera.Up, camera.Right);
			//discoid.Draw(camera.View, camera.Projection, camera.Up, camera.Right);
			//eps.Draw(camera.View, camera.Projection, camera.Up, camera.Right);
			//basicEmitter.Draw(camera.View, camera.Projection, camera.Up, camera.Right);
			//beamEmitter.Draw(camera.View, camera.Projection, camera.Up, camera.Right);

			// test effect
			discoidEffect.Draw(camera.View, camera.Projection, camera.CameraPosition, camera.Direction, camera.Up, camera.Right);
			//softParticle.Draw(camera.View, camera.Projection, camera.Up, camera.Right);

			// laser test
			lb.Draw(camera.View, camera.Projection, camera.Up, camera.Right, camera.CameraPosition);

            // glassEffect test
            //glassEffect.PreDraw(camera, gameTime);
            //glassEffect.Draw(camera.View, camera.Projection, camera.CameraPosition);
            

            // for debug
			belndState = GraphicsDevice.BlendState.ToString();
			depthState = GraphicsDevice.DepthStencilState.ToString();
			s.Draw(Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(start), camera.View, camera.Projection);
			e.Draw(Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(end), camera.View, camera.Projection);


			//debug.Draw(gameTime);
			//ResetGraphicDevice();
#endif

			/*ResetGraphicDevice();
			Ground.Draw();
			//ResetGraphicDevice();
			//DrawModel(groundModel, Matrix.Identity * Matrix.CreateScale(0.05f));
			//debug.Draw(gameTime);
			ResetGraphicDevice();
			//Target.World *= Matrix.CreateRotationZ(MathHelper.ToRadians(-90));
			Target.Draw();*/

			base.Draw(gameTime);
		}
	}
}

