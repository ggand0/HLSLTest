using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
namespace HLSLTest
{
	public class Level0 : Level
	{
		public Object Target { get; private set; }
		public Object Ground { get; private set; }
		public Object Teapot { get; private set; }

		PrelightingRenderer renderer;
		
		Water water;
		BillboardSystem trees;
		BillboardSystem clouds;
		Random r = new Random();

		FlameParticleEmitter ps;
		ExplosionParticleEmitter eps;
		DiscoidParticleEmitter discoid;
		ParticleEmitter basicEmitter, beamEmitter;
		ShockWaveParticleEmitter shockWaveEmitter;

		BillboardSystem lbs;
		LaserBillboard lb;
		Model s, e;
		Vector3 start = new Vector3(200, 50, 0), end = new Vector3(-50, -50, 0);
		BillboardCross treesCross;

		EnergyRingEffect discoidEffect;
		BillboardSystem softParticle;
		GlassEffect glassEffect;

		Terrain terrain;
		Planet planet;
		Model debugModel;
		SphericalTerrain2 sphericalTerrain;



		public Level0(Scene previousScene)
			: base(previousScene)
		{
			
		}
		protected override void Initialize()
		{
			base.Initialize();

			PrelightingRenderer.game = game;
			

			Target = new Object(new Vector3(0, 20, 0), "Models\\cube");
			Target.Scale = 20;
			//Target = new Object(new Vector3(0, 0, 0), "Models\\tank");
			//Target.Scale = 0.1f;
			Ground = new Object(new Vector3(0, -50, 0), 0.05f, "Models\\ground");
			Teapot = new Object(new Vector3(-100, 0, 0), "Models\\UtahTeapotDef");

			Target.Direction = Vector3.UnitX;
			Teapot.Scale = 10;
			//Teapot.RotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-180));
			Teapot.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
				* Matrix.CreateRotationX(MathHelper.ToRadians(-90));
			Models = new List<Object>();
			Models.Add(Ground);
			Models.Add(Target);
			Models.Add(Teapot);
			//Models.Add(new Object(new Vector3(0, 150, 0), "Models\\SkySphereMesh"));

			camera = new ArcBallCamera();
			camera.Initialize(game, Target);
			ParticleEmitter.camera = camera;



			// Generate random tree positions
			Random r = new Random();
			Vector3[] positions = new Vector3[100];
			for (int i = 0; i < positions.Length; i++) {
				//positions[i] = new Vector3((float)r.NextDouble() * 20000 - 10000, 400, (float)r.NextDouble() * 20000 - 10000);
				//positions[i] = new Vector3((float)r.NextDouble() * 200 - 100, 256, (float)r.NextDouble() * 200 - 100);
				positions[i] = new Vector3((float)r.NextDouble() * 200 - 100, 10, (float)r.NextDouble() * 200 - 100);
			}
			//trees = new BillboardSystem(device, content, content.Load<Texture2D>("tree"), new Vector2(800), positions);
			trees = new BillboardSystem(device, content, content.Load<Texture2D>("Textures\\Billboard\\tree"), new Vector2(10), positions);

			// Generate clouds
			Vector3[] cloudPositions = new Vector3[350];
			for (int i = 0; i < cloudPositions.Length; i++) {
				cloudPositions[i] = new Vector3(
					//r.Next(-6000, 6000), r.Next(2000, 3000), r.Next(-6000, 6000));
					r.Next(-6000, 6000), r.Next(2000, 3000), r.Next(-6000, 6000));
			}
			clouds = new BillboardSystem(device, content, content.Load<Texture2D>("Textures\\Billboard\\cloud"), new Vector2(500), cloudPositions);
			clouds.EnsureOcclusion = false;


			// Generate particles
			ps = new FlameParticleEmitter(device, content, content.Load<Texture2D>("Textures\\Particle\\fire"), Vector3.Zero, 1000, new Vector2(10), 10, Vector3.Zero, 0.01f);// 0.1f
			/*eps = new ExplosionParticleEmitter(device, content, content.Load<Texture2D>("Textures\\explosion"), Vector3.Zero, 2000, new Vector2(50), 20, 5f);
			discoid = new DiscoidParticleEmitter(device, content, content.Load<Texture2D>("Textures\\sun_1"), Vector3.Zero, 10000, new Vector2(5), 20, 5f);*/
			eps = new ExplosionParticleEmitter(device, content, Vector3.Zero, content.Load<Texture2D>("Textures\\Particle\\nova_2"), 2000, new Vector2(10), 20, 5f);
			discoid = new DiscoidParticleEmitter(device, content, content.Load<Texture2D>("Textures\\Particle\\nova_2"), Vector3.Zero, 10000, new Vector2(5), 20, 5f);
			basicEmitter = new ParticleEmitter(device, content, content.Load<Texture2D>("Textures\\Mercury\\Star"), new Vector3(0, 50, 0), 100, new Vector2(3), 3, 0.1f);

			beamEmitter = new ParticleEmitter(device, content, content.Load<Texture2D>("Textures\\Mercury\\Beam2"), new Vector3(0, 50, 0), 100, new Vector2(10), 3, 0.1f, BillboardMode.Line, true);
			//beamEmitter = new ParticleEmitter(device, content, BillboardMode.Line, content.Load<Texture2D>("Textures\\Laser2"), new Vector3(0, 50, 0), 100, new Vector2(10, 5), 3, 0.1f, true);
			shockWaveEmitter = new ShockWaveParticleEmitter(device, content, new Vector3(0, 50, 0), content.Load<Texture2D>("Textures\\Particle\\GlowRing"), 1, new Vector2(50), 3, 0, 0, 10, true);
			shockWaveEmitter.Run();


			softParticle = new BillboardSystem(device, content, content.Load<Texture2D>("Textures\\Particle\\nova_2"), 1, Models, new Vector2(100), new Vector3[] { new Vector3(0, 30, 0), new Vector3(-100, 0, 0) });

			// Generate lasers
			lbs = new BillboardSystem(device, content, content.Load<Texture2D>("Textures\\Laser"), new Vector2(10, 1000), new Vector3[] { Vector3.Zero });
			/*lb = new LaserBillboard(device, content, content.Load<Texture2D>("Textures\\Laser2"), new Vector2(300, 3),
				new Vector3(50, 50, 0), new Vector3(-50, -50, 0), new Vector3[] { Vector3.Zero });*/

			//lb = new LaserBillboard(device, content, content.Load<Texture2D>("Textures\\Laser2"), new Vector2(300, 3), start, end);
			lb = new LaserBillboard(device, content, content.Load<Texture2D>("Textures\\Laser2"), new Vector2(300, 50), start, end);
			s = content.Load<Model>("Models\\Ship"); e = content.Load<Model>("Models\\Ship");


			treesCross = new BillboardCross(device, content, content.Load<Texture2D>("Textures\\Billboard\\tree"), new Vector2(10), positions);
		}
		public override void Load()
		{
			base.Load();

			//sky = new SkySphere(content, device, content.Load<TextureCube>("OutputCube0"));//("OutputCube0"));
			//sky = new SkySphere(content, device, content.Load<TextureCube>("Cross"));//("OutputCube0"));
			Sky = new SkySphere(content, device, content.Load<TextureCube>("Textures\\SkyBox\\space4"), 10000);
			//Sky = new SkySphere(content, device, content.Load<TextureCube>("Textures\\Terrain\\CubeWrap"));

			// setup skymap reflection effect
			Effect cubeMapEffect = content.Load<Effect>("CubeMapReflect");
			CubeMapReflectMaterial cubeMat = new CubeMapReflectMaterial(Sky.TextureCube);
			Teapot.SetModelEffect(cubeMapEffect, false);
			Teapot.Material = cubeMat;/**/
			/*Models[3].SetModelEffect(cubeMapEffect, false);
			Models[3].Material = cubeMat;
			Models[3].Scale = 50;*/


			// projection
			/*Effect effect = content.Load<Effect>("TextureProjectionEffect");
			models[0].SetModelEffect(effect, true);
			models[1].SetModelEffect(effect, true);
			//t = content.Load<Texture2D>("Checker");//("projectedTexture");
			t = content.Load<Texture2D>("projectedTexture");
			ProjectedTextureMaterial mat = new ProjectedTextureMaterial(content.Load<Texture2D>("projectedTexture")
				, device);
			mat.ProjectorPosition = new Vector3(0, 100, 0);
			mat.ProjectorTarget = new Vector3(0, 0, 0);
			mat.Scale = 0.05f;//2
			models[0].Material = mat;
			models[1].Material = mat;*/


			// light map
			Effect shadowEffect = content.Load<Effect>("ProjectShadowDepthEffectV4");
			Effect lightingEffect = content.Load<Effect>("PPModel");	// load Prelighting Effect
			foreach (Object o in Models) {
				o.RenderBoudingSphere = false;
				o.SetModelEffect(shadowEffect, true);
			}


			renderer = new PrelightingRenderer(device, content);
			renderer.Models = Models;
			renderer.Camera = camera;
			renderer.Lights = new List<PointLight>() {
				new PointLightCircle(new Vector3(0, 200, 0), 200, Color.White, 2000),
				new PointLight(new Vector3(0, 500, 0), Color.White * .85f, 2000),

				/*new PointLight(new Vector3(-100, 100, 0), Color.Red * .85f,	2000),
				new PointLight(new Vector3(100, 100, 0), Color.Blue * .85f,	2000),
				new PointLight(new Vector3(0, 100, 100), Color.Green * .85f, 2000),*/
				/*new PointLight(new Vector3(-1000, 100, 0), Color.Red * .85f,	2000),
				new PointLight(new Vector3(1000, 1000, 0), Color.Blue * .85f,	2000),
				new PointLight(new Vector3(0, 1000, 1000), Color.Green * .85f, 2000),*/

				/*new PPPointLight(new Vector3(0, 200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000),
				new PPPointLight(new Vector3(0, -200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000)*/
			};

			// setup shadows
			/*//renderer.ShadowLightPosition = new Vector3(200, 100, 0);//new Vector3(1500, 1500, 2000);
			//renderer.ShadowLightTarget = new Vector3(-50, -50, 0);//new Vector3(0, 150, 0)
			renderer.ShadowLightPosition = new Vector3(0, 150, 0);//new Vector3(1500, 1500, 2000);
			renderer.ShadowLightTarget = new Vector3(0, 0, 0);//new Vector3(0, 150, 0)

			renderer.ShadowLightPosition = new Vector3(500, 500, 0);//new Vector3(1500, 1500, 2000);
			renderer.ShadowLightTarget = new Vector3(0, 300, 0);//new Vector3(0, 150, 0)*/
			renderer.ShadowLightPosition = new Vector3(500, 500, 0);
			renderer.ShadowLightTarget = new Vector3(0, 0, 0);

			renderer.DoShadowMapping = true;
			renderer.ShadowMult = 0.3f;//0.01f;//0.3f;
			

			Water.game = game;
			water = new Water(content, device, new Vector3(0, 0, 0), new Vector2(1000, 1000));
			//water = new Water(content, device, new Vector3(0, 0, 0), new Vector2(1000, 1000), renderer);
			water.Objects.Add(Sky);
			//water.Objects.Add(models[0]); water.Objects.Add(models[1]);
			foreach (Object o in Models) {
				water.Objects.Add(o);
			}
			water.Initialize();


			/*GlassEffect.game = this;
			glassEffect = new GlassEffect(content, device, new Vector3(0, 100, 0), 50);
			glassEffect.Objects.Add(Sky);
			foreach (Object o in Models) {
				glassEffect.Objects.Add(o);
			}
			glassEffect.Initialize();

			// 静的なオブジェクトを全て含めた環境マップ生成
			PreDrawScene(new GameTime());
			EnvironmentalMap = RenderCubeMap(new Vector3(0, 100, 0));*/


			// discoid effect : Skyの後に初期化
			EnergyRingEffect.game = game;
			discoidEffect = new EnergyRingEffect(content, device, new Vector3(0, 50, 0), new Vector2(300));

			// Terrain
			//terrain = new Terrain(content.Load<Texture2D>("Textures\\heightmap_01"), 30, 4800,				content.Load<Texture2D>("Textures\\Grass"), 6, new Vector3(1, -1, 0), device, content);
			//terrain = new Terrain(content.Load<Texture2D>("Textures\\terrain"), 30, 380, -200,				content.Load<Texture2D>("Textures\\Grass"), 6, new Vector3(1, -1, 0), device, content);
			//terrain = new Terrain(content.Load<Texture2D>("Textures\\terrain"), 100, 2500, -1500,				content.Load<Texture2D>("Textures\\Grass"), 6, new Vector3(1, -1, 0), device, content);
			terrain = new Terrain(content.Load<Texture2D>("Textures\\Terrain\\terrain"), 100, 2500, -1500,
				content.Load<Texture2D>("Textures\\Terrain\\grass"), 6, new Vector3(1, -1, 0), device, content);
			terrain.WeightMap = content.Load<Texture2D>("Textures\\color1");
			terrain.RTexture = content.Load<Texture2D>("Textures\\Terrain\\sand");
			terrain.GTexture = content.Load<Texture2D>("Textures\\Terrain\\grass");
			terrain.BTexture = content.Load<Texture2D>("Textures\\Terrain\\stone");
			terrain.DetailTexture = content.Load<Texture2D>("Textures\\detail0");



			// Planet test
			debugModel = content.Load<Model>("Models\\sphere2");
			WaterPlanet waterPlanet = new WaterPlanet(Vector3.Zero, device, content);
			IcePlanet icePlanet = new IcePlanet(device, content);
			GasGiant gasGiant = new GasGiant(device, content);
			RockPlanet rockPlanet = new RockPlanet(device, content);
			planet = rockPlanet;


			// Spherical terrain test
			sphericalTerrain = new SphericalTerrain2(content.Load<Texture2D>("Textures\\Terrain\\terrain"), 1, 50, 0,
				content.Load<Texture2D>("Textures\\Terrain\\grass"), 6, new Vector3(1, -1, 0), device, content);
			sphericalTerrain.WeightMap = content.Load<Texture2D>("Textures\\color1");
			sphericalTerrain.RTexture = content.Load<Texture2D>("Textures\\Terrain\\sand");
			sphericalTerrain.GTexture = content.Load<Texture2D>("Textures\\Terrain\\grass");
			sphericalTerrain.BTexture = content.Load<Texture2D>("Textures\\Terrain\\stone");
			sphericalTerrain.DetailTexture = content.Load<Texture2D>("Textures\\detail0");


			debug = new Debug();
		}

		int count = 0;
		public override void Update(GameTime gameTime)
		{
			count++;
			if (count % 300 == 0) {
				shockWaveEmitter.Reset = true;
			}

			base.Update(gameTime);

			Sky.Update(gameTime);

			foreach (Object o in Models) {
				o.Update(gameTime);
			}

			renderer.Update(gameTime);
			camera.UpdateChaseTarget(Target);
			camera.Update(gameTime);
			water.Update(gameTime);
			//Ground.Update(gameTime);


			// Update particles
			ps.Update();
			discoid.Update();
			eps.Update();
			basicEmitter.Update();
			beamEmitter.Update();
			shockWaveEmitter.Update();

			// Other effects
			//lb.Update(camera.Up, camera.Right, camera.CameraPosition);
			discoidEffect.Update(gameTime);
		}


		public TextureCube EnvironmentalMap { get; private set; }
		private TextureCube RenderCubeMap(Vector3 position)
		{
			TextureCube debug;
			//RenderTargetCube RefCubeMap = new RenderTargetCube(device, 256, 1, SurfaceFormat.Color);
			RenderTargetCube RefCubeMap = new RenderTargetCube(device, device.Viewport.Width, true, SurfaceFormat.Color, DepthFormat.Depth24);
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
				camera = new TargetCamera(position, Vector3.Zero, device);
				camera.View = viewMatrix; camera.Projection = this.camera.Projection;


				// Set the cubemap render target, using the selected face
				//device.SetRenderTarget(RefCubeMap, cubeMapFace);
				device.SetRenderTarget(RefCubeMap, cubeMapFace);
				device.Clear(Color.White);
				this.DrawScene(camera);
				device.SetRenderTarget(null);
			}

			device.SetRenderTarget(null);
			/*if (!hasSaved) {
				debug = RefCubeMap;// null!?
				DDSLib.DDSToFile("cubeMapFace_debug.dds", true, debug, false);
			}*/


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
			renderer.PreDraw();
		}
		public override void Draw(GameTime gameTime)
		{

			base.Draw(gameTime);
#if DEBUG_MODE
			device.Clear(Color.CornflowerBlue);
			renderer.Draw();
			
#else
			string belndState = device.BlendState.ToString();
			string depthState = device.DepthStencilState.ToString();
			string rasterizerState = device.RasterizerState.ToString();

			softParticle.DrawDepth(camera.View, camera.Projection, camera.CameraPosition);
			water.PreDraw(camera, gameTime);// renderer.Drawとの順番に注意　前に行わないとrendererのパラメータを汚してしまう?

			//glassEffect.PreDraw(camera, gameTime);
			//EnvironmentalMap = RenderCubeMap();// 動的環境マップ生成: 6回シーンを描画するので滅茶苦茶重い
			renderer.PreDraw();

			device.Clear(Color.Black);
			
			// Draw terrain
			Sky.Draw(camera.View, camera.Projection, camera.CameraPosition);
			//water.Draw(camera.View, camera.Projection, camera.CameraPosition);
			//terrain.Draw(false, camera.View, camera.Projection);
			//sphericalTerrain.Draw(false, camera.View, camera.Projection);

			belndState = device.BlendState.ToString();
			depthState = device.DepthStencilState.ToString();
			rasterizerState = device.RasterizerState.ToString();


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

			//beamEmitter.Draw(camera.View, camera.Projection, camera.CameraPosition, camera.Up, camera.Right);
			shockWaveEmitter.Draw(camera.View, camera.Projection, camera.CameraPosition, camera.Up, camera.Right);

			// test effect
			//discoidEffect.Draw(gameTime, camera.View, camera.Projection, camera.CameraPosition, camera.Direction, camera.Up, camera.Right);
			//softParticle.Draw(camera.View, camera.Projection, camera.Up, camera.Right);


			// laser test
			//BoundingBoxRenderer.Render(new BoundingBox(
			lb.Draw(camera.View, camera.Projection, camera.Up, camera.Right, camera.CameraPosition);
			renderer.Draw(gameTime);

			//planet.Draw(camera.View, Matrix.CreateScale(200) * Matrix.CreateTranslation(new Vector3(-300, 0, -200)), camera.Projection);

			//BoundingBoxRenderer.Render(new BoundingBox(new Vector3(-100, 0, 0), new Vector3(-0, 100, -100)), device, camera.View, camera.Projection, Color.White);

#endif

			/*ResetGraphicDevice();
			Ground.Draw();
			//ResetGraphicDevice();
			//DrawModel(groundModel, Matrix.Identity * Matrix.CreateScale(0.05f));
			//debug.Draw(gameTime);
			ResetGraphicDevice();
			//Target.World *= Matrix.CreateRotationZ(MathHelper.ToRadians(-90));
			Target.Draw();*/
		}
	}
}
